using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

// Pone la música de cada sala con fundido cruzado. Usa la banda sonora generada por
// MusicSynth o, si el jugador lo prefiere en Opciones, la música original de las escenas.
public class MusicDirector : MonoBehaviour
{
    public static MusicDirector Instance { get; private set; }

    private AudioSource a, b;
    private AudioSource active;
    private MusicSynth.Style? current;
    private readonly Dictionary<MusicSynth.Style, AudioClip> cache = new Dictionary<MusicSynth.Style, AudioClip>();
    private readonly List<AudioSource> sceneMusic = new List<AudioSource>();
    private readonly Dictionary<AudioSource, float> sceneMusicBaseVolume = new Dictionary<AudioSource, float>();
    private Coroutine fade;
    private float targetPitch = 1f;

    private const float BaseVolume = 0.55f;

    private static MusicDirector Ensure()
    {
        if (Instance != null) return Instance;
        GameObject go = new GameObject("MusicDirector");
        DontDestroyOnLoad(go);
        return go.AddComponent<MusicDirector>();
    }

    void Awake()
    {
        Instance = this;
        a = gameObject.AddComponent<AudioSource>();
        b = gameObject.AddComponent<AudioSource>();
        foreach (AudioSource s in new[] { a, b })
        {
            s.loop = true;
            s.playOnAwake = false;
            s.spatialBlend = 0f;
            s.volume = 0f;
        }
        active = a;
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameSettings.Changed += ApplyVolumes;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameSettings.Changed -= ApplyVolumes;
        if (Instance == this) Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneMusic.Clear();
        sceneMusicBaseVolume.Clear();
        targetPitch = 1f;
    }

    // ---------- API ----------

    public static void PlayMenu() => Ensure().Play(MusicSynth.Style.Menu);
    public static void PlayRoom(int room) => Ensure().Play(MusicSynth.StyleForRoom(room));

    // Fase final del jefe: la música se acelera un poco
    public static void SetIntensity(bool high)
    {
        if (Instance == null) return;
        Instance.targetPitch = high ? 1.08f : 1f;
    }

    public static void PlayVictory()
    {
        MusicDirector d = Ensure();
        if (!GameSettings.NewSoundtrack) return;
        d.StopAll(0.4f);
        AudioClip clip = d.GetClip(MusicSynth.Style.Victory);
        if (clip == null) return;
        GameObject go = new GameObject("Fanfarria");
        AudioSource s = go.AddComponent<AudioSource>();
        s.clip = clip;
        s.volume = BaseVolume * GameSettings.MusicVolume;
        s.ignoreListenerPause = true;
        s.Play();
        Destroy(go, clip.length + 0.5f);
    }

    public static void Stop(float time = 0.6f)
    {
        if (Instance != null) Instance.StopAll(time);
    }

    // ---------- Lógica ----------

    private void Play(MusicSynth.Style style)
    {
        lastRequested = style;
        CollectSceneMusic();
        if (!GameSettings.NewSoundtrack)
        {
            // Banda sonora original: dejamos sonar la música de la escena
            StopGenerated(0.5f);
            current = null;
            ApplyVolumes();
            return;
        }
        foreach (AudioSource s in sceneMusic) if (s != null) s.Stop();

        if (current.HasValue && current.Value == style && active.isPlaying) return;
        current = style;

        AudioClip clip = GetClip(style);
        if (clip == null) return;

        AudioSource from = active;
        AudioSource to = active == a ? b : a;
        to.clip = clip;
        to.time = 0f;
        to.volume = 0f;
        to.pitch = targetPitch;
        to.Play();
        active = to;
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(Crossfade(from, to, 1.2f));
    }

    private AudioClip GetClip(MusicSynth.Style style)
    {
        if (cache.TryGetValue(style, out AudioClip c) && c != null) return c;
        float[] data = MusicSynth.Generate(style);
        if (data == null || data.Length == 0) return null;
        AudioClip clip = AudioClip.Create("Musica_" + style, data.Length, 1, MusicSynth.Rate, false);
        clip.SetData(data, 0);
        cache[style] = clip;
        return clip;
    }

    private IEnumerator Crossfade(AudioSource from, AudioSource to, float time)
    {
        float t = 0f;
        float fromStart = from != null ? from.volume : 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / time);
            float target = BaseVolume * GameSettings.MusicVolume;
            to.volume = target * k;
            if (from != null && from != to) from.volume = fromStart * (1f - k);
            yield return null;
        }
        if (from != null && from != to) from.Stop();
        to.volume = BaseVolume * GameSettings.MusicVolume;
        fade = null;
    }

    private void StopAll(float time)
    {
        StopGenerated(time);
        current = null;
    }

    private void StopGenerated(float time)
    {
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(FadeOut(time));
    }

    private IEnumerator FadeOut(float time)
    {
        float t = 0f;
        float va = a.volume, vb = b.volume;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            float k = 1f - Mathf.Clamp01(t / time);
            a.volume = va * k;
            b.volume = vb * k;
            yield return null;
        }
        a.Stop();
        b.Stop();
        fade = null;
    }

    // Música que venía puesta en la escena (la original del juego)
    private void CollectSceneMusic()
    {
        if (sceneMusic.Count > 0) return;
        foreach (AudioSource s in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
        {
            if (s == a || s == b || s.clip == null || !s.loop) continue;
            sceneMusic.Add(s);
            sceneMusicBaseVolume[s] = s.volume;
        }
    }

    private void ApplyVolumes()
    {
        float mv = GameSettings.MusicVolume;
        if (fade == null && active != null && active.isPlaying) active.volume = BaseVolume * mv;
        foreach (AudioSource s in sceneMusic)
        {
            if (s == null) continue;
            s.volume = sceneMusicBaseVolume.TryGetValue(s, out float v) ? v * mv : s.volume;
            if (!GameSettings.NewSoundtrack && !s.isPlaying) s.Play();
        }
        // Cambiar de banda sonora desde las opciones surte efecto al momento
        if (GameSettings.NewSoundtrack && current == null && lastRequested.HasValue) Play(lastRequested.Value);
        if (!GameSettings.NewSoundtrack && (a.isPlaying || b.isPlaying)) { StopGenerated(0.4f); current = null; }
    }

    private MusicSynth.Style? lastRequested;

    void Update()
    {
        if (active != null && Mathf.Abs(active.pitch - targetPitch) > 0.001f)
            active.pitch = Mathf.MoveTowards(active.pitch, targetPitch, Time.unscaledDeltaTime * 0.1f);
    }
}
