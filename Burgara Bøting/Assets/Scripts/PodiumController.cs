// PodiumController.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class PodiumController : MonoBehaviour
{
    [SerializeField] TMP_Text first, second, third;

    void Start() { StartCoroutine(LoadTop3()); }

    string BuildUrl(string path)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        var abs = Application.absoluteURL;
        if (!string.IsNullOrEmpty(abs))
        {
            try { var u = new System.Uri(abs); return $"{u.Scheme}://{u.Authority}{path}"; } catch {}
        }
        return path;
#else
        var baseU = "http://127.0.0.1:8080";
        return $"{baseU}{path}";
#endif
    }

    IEnumerator LoadTop3()
    {
        using (var req = UnityWebRequest.Get(BuildUrl("/api/players")))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) yield break;
            var wrapper = JsonUtility.FromJson<PlayersResp>(req.downloadHandler.text);
            if (wrapper != null && wrapper.ok && wrapper.players != null)
            {
                if (wrapper.players.Length > 0) first.text  = $"{wrapper.players[0].name} ({wrapper.players[0].score})";
                if (wrapper.players.Length > 1) second.text = $"{wrapper.players[1].name} ({wrapper.players[1].score})";
                if (wrapper.players.Length > 2) third.text  = $"{wrapper.players[2].name} ({wrapper.players[2].score})";
            }
        }
    }

    [System.Serializable] class PlayersResp { public bool ok; public Player[] players; }
    [System.Serializable] class Player { public string name; public int score; }
}
