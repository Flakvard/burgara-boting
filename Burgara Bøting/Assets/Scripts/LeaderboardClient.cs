// LeaderboardClient.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardClient : MonoBehaviour
{
    [Header("Server")]
    // You can use HTTP 8080 to avoid self-signed HTTPS issues during local dev,
    // or use HTTPS 8443 (see cert note below).
    public string baseUrl = "http://localhost:8080";

    [Header("Cadence")]
    public float postIntervalSeconds = 1f;

    private string _playerId;
    private bool _joined;
    private bool _loopStarted;
    public static LeaderboardClient Instance { get; private set; }
    void Awake()
    {
        // Singleton + persist across scenes
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    IEnumerator Start()
    {
        // Create/get a stable id for WebGL (PlayerPrefs → IndexedDB/localStorage)
        _playerId = GetOrCreatePlayerId();
        // Wait a moment to allow UI to set PlayerStats.Name during boot
        yield return null;
    }

    public void JoinNow() // call this from your Play button flow
    {
        if (!_joined) StartCoroutine(JoinAndStartLoop());
    }

    // New: call this from your loader and yield it before changing scenes
    public IEnumerator EnsureJoinedThenLoop()
    {
        if (!_joined) yield return Join();
        StartLoopOnce();
    }

    // Optional: push score immediately on big changes (e.g., when a level ends)
    public void PushScoreNow(int score)
    {
        if (_joined) StartCoroutine(PostScore(score));
    }

    IEnumerator JoinAndStartLoop()
    {
        yield return Join();
        StartLoopOnce();
    }

        void StartLoopOnce()
    {
        if (_joined && !_loopStarted)
        {
            _loopStarted = true;
            StartCoroutine(PostScoreLoop());
        }
    }

    string GetOrCreatePlayerId()
    {
        var id = PlayerPrefs.GetString("playerId", "");
        if (string.IsNullOrWhiteSpace(id))
        {
            id = System.Guid.NewGuid().ToString("N"); // 32 hex chars
            PlayerPrefs.SetString("playerId", id);
            PlayerPrefs.Save();
        }
        return id;
    }

    // LeaderboardClient.cs (drop-in replacements/additions)

    // buildUrlHelper
    string BuildUrl(string path)
    {
    #if UNITY_WEBGL && !UNITY_EDITOR
        var abs = Application.absoluteURL;                // e.g. https://server-ip:8443/play/index.html?...
        if (!string.IsNullOrEmpty(abs))
        {
            try {
                var u = new System.Uri(abs);
                return $"{u.Scheme}://{u.Authority}{path}"; // same scheme+host+port as the page
            } catch { /* fall through */ }
        }
        return path; // last resort (relative)
    #else
        // Editor/Standalone: use the serialized baseUrl (default to localhost)
        var baseU = string.IsNullOrWhiteSpace(baseUrl) ? "http://127.0.0.1:8080" : baseUrl.TrimEnd('/');
        return $"{baseU}{path}";
    #endif
    }

    IEnumerator Join()
    {
        // var url = $"{baseUrl}/api/join";
        var url = BuildUrl("/api/join");
        var payload = JsonUtility.ToJson(new JoinBody {
            name = string.IsNullOrWhiteSpace(PlayerStats.Name) ? "Player" : PlayerStats.Name,
            playerId = _playerId,
            platform = Application.platform.ToString()
        });

        using (var req = new UnityWebRequest(url, "POST"))
        {
            var body = System.Text.Encoding.UTF8.GetBytes(payload);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var resp = JsonUtility.FromJson<JoinResponse>(req.downloadHandler.text);
                _joined = resp.ok;
                Debug.Log($"JOIN ok={resp.ok} id={resp.playerId} name={resp.name}");
            }
            else
            {
                Debug.LogWarning($"JOIN failed: {req.responseCode} {req.error}");
            }

#if UNITY_EDITOR
            // If you switch to HTTPS on localhost and use a self-signed cert,
            // allow it in Editor builds (the browser will handle cert for WebGL).
            req.certificateHandler = new AcceptAllCertificates();
#endif
        }
    }

    IEnumerator PostScoreLoop()
    {
        var wait = new WaitForSeconds(postIntervalSeconds);
        while (true)
        {
            yield return PostScore(PlayerStats.Score);
            yield return wait;
        }
    }

    IEnumerator PostScore(int score)
    {
        if (!_joined || string.IsNullOrEmpty(_playerId)) yield break;

        // var url = $"{baseUrl}/api/score";
        var url = BuildUrl("/api/score");
        var payload = JsonUtility.ToJson(new ScoreBody
        {
            playerId = _playerId,
            score = score
        });

        using (var req = new UnityWebRequest(url, "POST"))
        {
            var body = System.Text.Encoding.UTF8.GetBytes(payload);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

#if UNITY_EDITOR
            req.certificateHandler = new AcceptAllCertificates();
#endif

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
                Debug.LogWarning($"SCORE failed: {req.responseCode} {req.error}");
        }
    }

    // --- DTOs ---
    [System.Serializable] class JoinBody { public string name; public string playerId; public string platform; }
    [System.Serializable] class JoinResponse { public bool ok; public string playerId; public string name; }
    [System.Serializable] class ScoreBody { public string playerId; public int score; }

#if UNITY_EDITOR
    // Accept local self-signed certs (Editor only). Do NOT ship this in production.
    class AcceptAllCertificates : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData) => true;
    }
#endif
}
