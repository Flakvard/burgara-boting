using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // --- Singleton ---
    public static SoundManager Instance { get; private set; }

    [Header("Sources")]
    public AudioSource EffectsSource;
    public AudioSource MusicSource;

    [Header("Random pitch jitter for spammy SFX (±)")]
    [Range(0f, 0.2f)] public float PitchJitter = 0.05f;

    // --- Strongly-typed IDs for your SFX ---
    public enum Sfx
    {
        RopeSnap,      // Band í slittnar
        RopeTension,   // Band í verdur toga
        KnotJoinClick, // Knútur ljóð tá man koblar saman
        KnotGeneric,   // Knútur
        KnotCoupling   // ljóð til knút kobling
    }

    [Header("SFX Library (assign in Inspector)")]
    [SerializeField] private AudioClip sfxRopeSnap;
    [SerializeField] private AudioClip sfxRopeTension;
    [SerializeField] private AudioClip sfxKnotJoinClick;
    [SerializeField] private AudioClip sfxKnotGeneric;
    [SerializeField] private AudioClip sfxKnotCoupling;

    private Dictionary<Sfx, AudioClip> _sfxMap;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Safety checks
        if (!EffectsSource) Debug.LogWarning("[SoundManager] EffectsSource not assigned.");
        if (!MusicSource)   Debug.LogWarning("[SoundManager] MusicSource not assigned.");

        // Build map
        _sfxMap = new Dictionary<Sfx, AudioClip>
        {
            { Sfx.RopeSnap,     sfxRopeSnap },
            { Sfx.RopeTension,  sfxRopeTension },
            { Sfx.KnotJoinClick,sfxKnotJoinClick },
            { Sfx.KnotGeneric,  sfxKnotGeneric },
            { Sfx.KnotCoupling, sfxKnotCoupling }
        };
    }

    // -------- Public API --------

    // Music
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (!clip || !MusicSource) return;
        MusicSource.clip = clip;
        MusicSource.loop = loop;
        MusicSource.volume = 1f;
        MusicSource.Play();
    }

    public void StopMusic() { if (MusicSource) MusicSource.Stop(); }

    // SFX by id (preferred)
    public void PlaySfx(Sfx id, float volume = 1f, bool jitterPitch = true)
    {
        if (!_sfxMap.TryGetValue(id, out var clip) || !clip) return;
        PlayOneShot(clip, volume, jitterPitch);
    }

    // SFX by clip (fallback)
    public void PlayOneShot(AudioClip clip, float volume = 1f, bool jitterPitch = true)
    {
        if (!clip || !EffectsSource) return;
        var originalPitch = EffectsSource.pitch;
        if (jitterPitch && PitchJitter > 0f)
        {
            float delta = Random.Range(-PitchJitter, PitchJitter);
            EffectsSource.pitch = 1f + delta;
        }
        EffectsSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        EffectsSource.pitch = originalPitch;
    }

    // Convenience named methods (super simple)
    public void PlayRopeSnap()      => PlaySfx(Sfx.RopeSnap);
    public void PlayRopeTension()   => PlaySfx(Sfx.RopeTension);
    public void PlayKnotJoinClick() => PlaySfx(Sfx.KnotJoinClick);
    public void PlayKnot()          => PlaySfx(Sfx.KnotGeneric);
    public void PlayKnotCoupling()  => PlaySfx(Sfx.KnotCoupling);
}
