// server-https.mjs
import https from 'https';
import http from 'http';
import fs from 'fs/promises';
import fssync from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Serve the folder that contains index.html (parent of Build/)
const ROOT = __dirname;
const HTTPS_PORT = 8443;
const HTTP_PORT  = 8080;

// If you built with Brotli in Unity, leave 'br'. If you used Gzip with .unityweb, set 'gzip'.
const UNITY_ENCODING = 'br';

// load certs (created by mkcert)
const key  = fssync.readFileSync(path.join(__dirname, 'localhost-key.pem'));
const cert = fssync.readFileSync(path.join(__dirname, 'localhost.pem'));

const mime = {
  '.html':'text/html', '.js':'application/javascript', '.wasm':'application/wasm',
  '.json':'application/json', '.data':'application/octet-stream',
  '.png':'image/png', '.jpg':'image/jpeg', '.jpeg':'image/jpeg', '.svg':'image/svg+xml',
  '.css':'text/css', '.txt':'text/plain'
};

const baseHeaders = {
  'Cross-Origin-Opener-Policy': 'same-origin',
  'Cross-Origin-Embedder-Policy': 'require-corp',
  'Cache-Control': 'no-store'
};

function headersFor(filePath) {
  let enc;
  let candidate = filePath;
  if (candidate.endsWith('.br')) { enc = 'br'; candidate = candidate.slice(0, -3); }
  if (candidate.endsWith('.gz')) { enc = 'gzip'; candidate = candidate.slice(0, -3); }
  const ext = path.extname(candidate).toLowerCase();
  const type = mime[ext] || 'application/octet-stream';

  const h = { ...baseHeaders, 'Content-Type': type };
  if (enc) h['Content-Encoding'] = enc;
  if (filePath.endsWith('.unityweb')) h['Content-Encoding'] = UNITY_ENCODING;
  return h;
}

// --- Simple in-memory "db" ---
const players = new Map(); // playerId -> { name, platform, ts, ua, score }

const sseClients = new Set(); // live admin viewers

function snapshotPlayers() {
  const list = [];
  for (const [id, p] of players) {
    list.push({
      playerId: id,
      name: p.name,
      platform: p.platform,
      score: p.score ?? 0,
      lastSeen: p.ts,
      ua: p.ua,
    });
  }
  // sort by score desc, then most recent
  list.sort((a, b) => (b.score - a.score) || (b.lastSeen - a.lastSeen));
  return list;
}

function broadcastPlayers() {
  const payload = `data: ${JSON.stringify({ type: 'players', players: snapshotPlayers() })}\n\n`;
  for (const res of sseClients) {
    res.write(payload);
  }
}

// --- JSON/CORS helpers ---
function jsonHeaders() {
  return {
    'Content-Type': 'application/json; charset=utf-8',
    'Access-Control-Allow-Origin': '*',
    'Access-Control-Allow-Methods': 'POST, OPTIONS',
    'Access-Control-Allow-Headers': 'Content-Type',
  };
}

// --- Static file server ---
async function serveStatic(req, res) {
  try {
    const urlPath = decodeURIComponent(new URL(req.url, `https://${req.headers.host}`).pathname);
    let filePath = path.join(ROOT, urlPath === '/' ? 'index.html' : urlPath);
    if (!filePath.startsWith(ROOT)) { res.writeHead(403); res.end('Forbidden'); return; }

    let data;
    try { data = await fs.readFile(filePath); }
    catch {
      if (path.extname(filePath)) { res.writeHead(404); res.end('Not found'); return; }
      filePath = path.join(ROOT, 'index.html');
      data = await fs.readFile(filePath);
    }

    res.writeHead(200, headersFor(filePath));
    res.end(data);
  } catch (e) {
    res.writeHead(500); res.end(String(e));
  }
}

// --- API router (shared by both servers) ---
function handleApi(req, res, proto = 'https') {
  const { pathname } = new URL(req.url, `${proto}://${req.headers.host}`);

  // CORS preflight
  if (req.method === 'OPTIONS' && (pathname === '/api/join' || pathname === '/api/score')) {
    res.writeHead(204, jsonHeaders()); res.end(); return true;
  }

  if (pathname === '/api/join') {
    if (req.method !== 'POST') { res.writeHead(405, jsonHeaders()); res.end(JSON.stringify({ ok:false, error:'Method not allowed' })); return true; }
    let body = '';
    req.on('data', chunk => { body += chunk; if (body.length > 1e6) req.socket.destroy(); });
    req.on('end', () => {
      try {
        const data = JSON.parse(body || '{}');
        const nameIn = (data.name ?? '').toString().trim();
        const inId   = (data.playerId ?? '').toString().trim();
        const pid    = (inId.length >= 6 ? inId : cryptoRandomId());

        const prev = players.get(pid) || {};
        const record = {
          ...prev,
          name: nameIn || prev.name || 'Player',
          platform: (data.platform || prev.platform || 'unknown'),
          ts: Date.now(),
          ua: req.headers['user-agent'] || '',
          score: typeof prev.score === 'number' ? prev.score : 0,
        };
        players.set(pid, record);

        res.writeHead(200, jsonHeaders());
        res.end(JSON.stringify({ ok:true, playerId: pid, name: record.name }));

        broadcastPlayers();
      } catch {
        res.writeHead(400, jsonHeaders());
        res.end(JSON.stringify({ ok:false, error:'Bad JSON' }));
      }
    });

    return true;
  }

  if (pathname === '/api/score') {
    if (req.method !== 'POST') { res.writeHead(405, jsonHeaders()); res.end(JSON.stringify({ ok:false, error:'Method not allowed' })); return true; }
    let body = '';
    req.on('data', chunk => { body += chunk; if (body.length > 1e6) req.socket.destroy(); });
    req.on('end', () => {
      try {
        const { playerId, score } = JSON.parse(body || '{}');
        if (!playerId || typeof score !== 'number') {
          res.writeHead(400, jsonHeaders());
          res.end(JSON.stringify({ ok:false, error:'playerId and numeric score required' }));
          return;
        }
        const p = players.get(playerId);
        if (!p) {
          res.writeHead(404, jsonHeaders());
          res.end(JSON.stringify({ ok:false, error:'Unknown playerId' }));
          return;
        }
        p.score = score;
        p.ts = Date.now();
        players.set(playerId, p);
        broadcastPlayers();
        res.writeHead(200, jsonHeaders());
        res.end(JSON.stringify({ ok:true }));
      } catch {
        res.writeHead(400, jsonHeaders());
        res.end(JSON.stringify({ ok:false, error:'Bad JSON' }));
      }
    });
    return true;
  }

   // GET /api/players (simple JSON snapshot)
  if (pathname === '/api/players' && req.method === 'GET') {
    res.writeHead(200, jsonHeaders());
    res.end(JSON.stringify({ ok:true, players: snapshotPlayers() }));
    return true;
  }

  // GET /api/stream (Server-Sent Events for live updates)
  if (pathname === '/api/stream' && req.method === 'GET') {
    res.writeHead(200, {
      'Content-Type': 'text/event-stream',
      'Cache-Control': 'no-cache',
      'Connection': 'keep-alive',
      'Access-Control-Allow-Origin': '*',
    });
    // send initial snapshot
    res.write(`retry: 2000\n`);
    res.write(`data: ${JSON.stringify({ type: 'players', players: snapshotPlayers() })}\n\n`);

    sseClients.add(res);
    req.on('close', () => sseClients.delete(res));
    return true;
  }
 

  // not an API route
  return false;
}

// small id helper
function cryptoRandomId() {
  return Math.random().toString(36).slice(2) + Math.random().toString(36).slice(2);
}

// HTTPS server (serves static + API)
https.createServer({ key, cert }, (req, res) => {
  if (handleApi(req, res, 'https')) return;
  serveStatic(req, res);
}).listen(HTTPS_PORT, () => {
  console.log(`🔒 HTTPS: https://localhost:${HTTPS_PORT}`);
  console.log(`Serving: ${ROOT}`);
});

// HTTP server (redirects to https EXCEPT for API which stays plain HTTP if you want it)
http.createServer((req, res) => {
  if (handleApi(req, res, 'http')) return;
  const host = req.headers.host?.replace(/:\d+$/, `:${HTTPS_PORT}`) || `localhost:${HTTPS_PORT}`;
  res.writeHead(301, { Location: `https://${host}${req.url}` }); res.end();
}).listen(HTTP_PORT, () => console.log(`↪  http://localhost:${HTTP_PORT} (redirects → HTTPS; API served on both)`));
