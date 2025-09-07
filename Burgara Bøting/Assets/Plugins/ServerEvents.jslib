mergeInto(LibraryManager.library, {
  ServerEvents_Init: function(goPtr) {
    var go = UTF8ToString(goPtr);
    if (window.__sse) { try { window.__sse.close(); } catch(e){} }
    var es = new EventSource('/api/stream');
    window.__sse = es;

    es.onmessage = function(ev){
      try {
        var m = JSON.parse(ev.data || '{}');

        if (m.type === 'game') {
          // Late-join sync: if a game is running, compute remaining and start
          var st = m.state || {};
          if (st.running && st.startedAt && st.duration) {
            var now = m.now || Date.now();
            var elapsed = Math.max(0, (now - st.startedAt) / 1000);
            var remain = Math.max(0, st.duration - elapsed);
            if (remain > 0) {
              SendMessage(go, 'OnServerStartTimer', remain.toString());
            }
          }
          return;
        }

        if (m.type !== 'cmd') return;
        var cmd = m.cmd || '';
        if (cmd === 'start') {
          var secs = (m.seconds || 0).toString();
          SendMessage(go, 'OnServerStartTimer', secs);
        } else if (cmd === 'stop') {
          SendMessage(go, 'OnServerStopTimer', '');
        } else if (cmd === 'end') {
          var payload = JSON.stringify({ scene: m.scene || 'EndScene', winner: m.winner || null });
          SendMessage(go, 'OnServerEnd', payload);
        }
      } catch(e) { /* ignore */ }
    };
  },
  ServerEvents_Close: function() {
    if (window.__sse) { try { window.__sse.close(); } catch(e){} window.__sse = null; }
  }
});
