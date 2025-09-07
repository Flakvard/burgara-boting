// ServerEventsBehaviour.cs
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerEventsBehaviour : MonoBehaviour
{
    public static ServerEventsBehaviour Instance { get; private set; }

    [Header("Scenes")]
    [SerializeField] string gameSceneName = "GameScene";
    [SerializeField] string endSceneName  = "EndScene";

    [Header("Refs (rebounds per scene)")]
    [SerializeField] Timer timer;  // will auto-find after each load

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void ServerEvents_Init(string goName);
    [DllImport("__Internal")] private static extern void ServerEvents_Close();
#else
    private static void ServerEvents_Init(string goName) {}
    private static void ServerEvents_Close() {}
#endif

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // rebind scene objects whenever a scene loads
        SceneManager.sceneLoaded += (_, __) => RebindSceneObjects();
    }

    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ServerEvents_Init(gameObject.name);
#endif
        RebindSceneObjects();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        ServerEvents_Close();
        SceneManager.sceneLoaded -= (_, __) => RebindSceneObjects();
    }

    void RebindSceneObjects()
    {
        if (timer == null) timer = FindAnyObjectByType<Timer>();
    }

    // ===== JS → C# callbacks (called by ServerEvents.jslib) =====

    // "start": optionally load GameScene, then set timer and start it
    public void OnServerStartTimer(string secondsStr)
    {
        if (!float.TryParse(secondsStr, out var secs)) secs = 0f;
        StartCoroutine(EnsureGameSceneThenStart(secs));
    }

    public void OnServerStopTimer(string _)
    {
        RebindSceneObjects();
        timer?.StopTimer();
    }

    // "end": go to end scene (server may send a winner list if you want)
    public void OnServerEnd(string jsonPayload)
    {
        // optional: stash payload for podium UI
        PlayerPrefs.SetString("endPayload", jsonPayload ?? "");
        PlayerPrefs.Save();
        SceneManager.LoadScene(endSceneName);
    }

    IEnumerator EnsureGameSceneThenStart(float seconds)
    {
        if (SceneManager.GetActiveScene().name != gameSceneName)
        {
            var op = SceneManager.LoadSceneAsync(gameSceneName);
            while (!op.isDone) yield return null;
            // scene changed—rebinding happens via sceneLoaded event, but ensure:
            RebindSceneObjects();
        }

        if (timer == null) timer = FindAnyObjectByType<Timer>();
        if (timer != null)
        {
            timer.ResetTimer(seconds);
            timer.StartTimer();
        }
    }
}
