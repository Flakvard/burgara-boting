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

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    IEnumerator Start()
    {
        // Create/get a stable id for WebGL (PlayerPrefs → IndexedDB/localStorage)
        _playerId = GetOrCreatePlayerId();

        // Wait a moment to allow UI to set PlayerStats.Name during boot
        yield return null;

        // Do NOT auto-join here; we’ll join when the player presses Play (after name is set)
        // If you still want auto-join fallback, uncomment:
        // if (string.IsNullOrWhiteSpace(PlayerStats.Name)) yield break;
        // yield return Join();
        // if (_joined) StartCoroutine(PostScoreLoop());
    }

    public void JoinNow() // call this from your Play button flow
    {
        if (!_joined) StartCoroutine(JoinAndStartLoop());
    }

    IEnumerator JoinAndStartLoop()
    {
        yield return Join();
        if (_joined) StartCoroutine(PostScoreLoop());
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

    IEnumerator Join()
    {
        var url = $"{baseUrl}/api/join";
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

        var url = $"{baseUrl}/api/score";
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
