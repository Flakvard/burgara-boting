using UnityEngine;

public class LoadingScreenController : MonoBehaviour
{
    [Header("Music")]
    public AudioClip LoadingMusic;

    [Header("Key Bindings")]
    public KeyCode RopeSnapKey      = KeyCode.Alpha1; // Band í slittnar
    public KeyCode RopeTensionKey   = KeyCode.Alpha2; // Band í verdur toga
    public KeyCode KnotJoinKey      = KeyCode.Alpha3; // Knútur ljóð tá man koblar saman
    public KeyCode KnotGenericKey   = KeyCode.Alpha4; // Knútur
    public KeyCode KnotCouplingKey  = KeyCode.Alpha5; // ljóð til knút kobling
    public KeyCode RandomKey        = KeyCode.Space;

    // Utility keys
    public KeyCode ToggleMusicMute  = KeyCode.M;
    public KeyCode ToggleSfxMute    = KeyCode.N;
    public KeyCode PauseResumeMusic = KeyCode.P;

    // UI hint
    public bool showHelpOverlay = true;

    void Start()
    {
        if (LoadingMusic) SoundManager.Instance.PlayMusic(LoadingMusic);
    }

    void Update()
    {
        // SFX tests
        if (Input.GetKeyDown(RopeSnapKey))     SoundManager.Instance.PlayRopeSnap();
        if (Input.GetKeyDown(RopeTensionKey))  SoundManager.Instance.PlayRopeTension();
        if (Input.GetKeyDown(KnotJoinKey))     SoundManager.Instance.PlayKnotJoinClick();
        if (Input.GetKeyDown(KnotGenericKey))  SoundManager.Instance.PlayKnot();
        if (Input.GetKeyDown(KnotCouplingKey)) SoundManager.Instance.PlayKnotCoupling();

        // Random SFX (Space)
        if (Input.GetKeyDown(RandomKey))
        {
            var values = (SoundManager.Sfx[])System.Enum.GetValues(typeof(SoundManager.Sfx));
            var pick = values[Random.Range(0, values.Length)];
            SoundManager.Instance.PlaySfx(pick);
        }

        // Utility controls
        if (Input.GetKeyDown(ToggleMusicMute))
            SoundManager.Instance.MusicSource.mute = !SoundManager.Instance.MusicSource.mute;

        if (Input.GetKeyDown(ToggleSfxMute))
            SoundManager.Instance.EffectsSource.mute = !SoundManager.Instance.EffectsSource.mute;

        if (Input.GetKeyDown(PauseResumeMusic))
        {
            var music = SoundManager.Instance.MusicSource;
            if (music.isPlaying) music.Pause();
            else                 music.UnPause();
        }
    }

    void OnGUI()
    {
        if (!showHelpOverlay) return;

        var style = new GUIStyle(GUI.skin.label) { fontSize = 14 };
        float y = 10f;
        GUI.Label(new Rect(10, y, 900, 24), "SFX Test Keys:", style); y += 20;
        GUI.Label(new Rect(10, y, 900, 24), "[1] Rope Snap   [2] Rope Tension   [3] Knot Join   [4] Knot   [5] Knot Coupling   [Space] Random", style); y += 20;
        GUI.Label(new Rect(10, y, 900, 24), "[M] Toggle Music Mute   [N] Toggle SFX Mute   [P] Pause/Resume Music", style);
    }
}
