using UnityEngine;
using System.Collections.Generic;

public enum SfxType { Pickup, PowerUp, Smash, Achievement, Coo, Blip, Select, ShieldBreak, Thunder, Snore, Whoosh, Combo, Land, Flap, Meow }

// Efectos de sonido sintetizados por código (sin archivos de audio nuevos):
// notas de onda cuadrada/triangular y ruido filtrado con envolventes.
public class Sfx : MonoBehaviour
{
    private const int Rate = 22050;
    private static Sfx instance;

    private readonly Dictionary<SfxType, AudioClip> clips = new Dictionary<SfxType, AudioClip>();
    private AudioSource[] pool;
    private int next;

    private static Sfx Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("Sfx");
                instance = go.AddComponent<Sfx>();
            }
            return instance;
        }
    }

    void Awake()
    {
        pool = new AudioSource[8];
        for (int i = 0; i < pool.Length; i++)
        {
            pool[i] = gameObject.AddComponent<AudioSource>();
            pool[i].playOnAwake = false;
            pool[i].spatialBlend = 0f;
        }
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    // ui = suena aunque el juego esté en pausa
    public static void Play(SfxType type, float volume = 1f, float pitch = 1f, bool ui = false)
    {
        Sfx s = Instance;
        AudioClip clip = s.GetClip(type);
        AudioSource src = s.pool[s.next];
        s.next = (s.next + 1) % s.pool.Length;
        src.ignoreListenerPause = ui;
        src.pitch = pitch * Random.Range(0.97f, 1.03f);
        src.PlayOneShot(clip, volume * 0.5f);
    }

    private AudioClip GetClip(SfxType t)
    {
        if (clips.TryGetValue(t, out AudioClip c) && c != null) return c;
        float[] data;
        switch (t)
        {
            case SfxType.Pickup: data = Notes(new[] { 988f, 1319f }, 0.06f, Wave.Square, 0.35f); break;
            case SfxType.PowerUp: data = Notes(new[] { 523f, 659f, 784f, 1047f, 1319f }, 0.05f, Wave.Triangle, 0.6f); break;
            case SfxType.Smash: data = Mix(Noise(0.28f, 18f, 0.35f), Sweep(160f, 50f, 0.2f, Wave.Sine, 0.7f)); break;
            case SfxType.Achievement: data = Notes(new[] { 784f, 988f, 1175f, 1568f }, 0.09f, Wave.Triangle, 0.6f, 0.25f); break;
            case SfxType.Coo: data = Concat(Vibrato(390f, 0.12f, 0.5f), Silence(0.04f), Vibrato(340f, 0.26f, 0.5f)); break;
            case SfxType.Blip: data = Notes(new[] { 660f }, 0.035f, Wave.Square, 0.25f); break;
            case SfxType.Select: data = Notes(new[] { 660f, 990f }, 0.045f, Wave.Square, 0.3f); break;
            case SfxType.ShieldBreak: data = Mix(Noise(0.2f, 14f, 0.8f), Sweep(1400f, 300f, 0.25f, Wave.Square, 0.25f)); break;
            case SfxType.Thunder: data = Noise(1.6f, 2.2f, 0.08f, 0.9f); break;
            case SfxType.Snore: data = Concat(Sweep(90f, 140f, 0.45f, Wave.Saw, 0.25f), Silence(0.1f), Sweep(140f, 80f, 0.4f, Wave.Saw, 0.2f)); break;
            case SfxType.Whoosh: data = Noise(0.25f, 0f, 0.5f, 0.6f, true); break;
            case SfxType.Combo: data = Notes(new[] { 1175f, 1568f, 2093f }, 0.04f, Wave.Square, 0.3f); break;
            case SfxType.Meow: data = Concat(Sweep(520f, 900f, 0.12f, Wave.Triangle, 0.4f), Sweep(900f, 470f, 0.28f, Wave.Triangle, 0.4f)); break;
            case SfxType.Land: data = Noise(0.06f, 40f, 0.15f, 0.5f); break;
            default: data = Noise(0.12f, 0f, 0.6f, 0.35f, true); break; // Flap
        }
        AudioClip clip = AudioClip.Create("sfx_" + t, data.Length, 1, Rate, false);
        clip.SetData(data, 0);
        clips[t] = clip;
        return clip;
    }

    // ---------- Síntesis ----------

    private enum Wave { Square, Triangle, Sine, Saw }

    private static float Osc(Wave w, float phase)
    {
        float p = phase - Mathf.Floor(phase);
        switch (w)
        {
            case Wave.Square: return p < 0.5f ? 1f : -1f;
            case Wave.Triangle: return 1f - 4f * Mathf.Abs(p - 0.5f);
            case Wave.Saw: return 2f * p - 1f;
            default: return Mathf.Sin(p * Mathf.PI * 2f);
        }
    }

    private static float Env(int i, int n)
    {
        int attack = Mathf.Min(n / 10, Rate / 200);
        if (i < attack) return i / (float)Mathf.Max(1, attack);
        return 1f - (i - attack) / (float)Mathf.Max(1, n - attack);
    }

    private static float[] Notes(float[] freqs, float each, Wave w, float vol, float tail = 0f)
    {
        int n = Mathf.RoundToInt(each * Rate);
        int total = n * freqs.Length + Mathf.RoundToInt(tail * Rate);
        float[] d = new float[total];
        for (int k = 0; k < freqs.Length; k++)
        {
            int len = (k == freqs.Length - 1) ? total - k * n : n;
            for (int i = 0; i < len; i++)
            {
                float e = (k == freqs.Length - 1) ? Env(i, len) : Mathf.Min(1f, (n - i) / (n * 0.3f) + 0.4f);
                d[k * n + i] = Osc(w, freqs[k] * i / Rate) * e * vol;
            }
        }
        return d;
    }

    private static float[] Sweep(float from, float to, float dur, Wave w, float vol)
    {
        int n = Mathf.RoundToInt(dur * Rate);
        float[] d = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float f = Mathf.Lerp(from, to, i / (float)n);
            phase += f / Rate;
            d[i] = Osc(w, phase) * Env(i, n) * vol;
        }
        return d;
    }

    private static float[] Vibrato(float freq, float dur, float vol)
    {
        int n = Mathf.RoundToInt(dur * Rate);
        float[] d = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Rate;
            phase += (freq + Mathf.Sin(t * 40f) * 25f) / Rate;
            float e = Mathf.Sin(Mathf.PI * i / n);
            d[i] = Osc(Wave.Sine, phase) * e * vol;
        }
        return d;
    }

    // decay: velocidad de caída exponencial; smooth: 0..1 (más alto = más grave)
    private static float[] Noise(float dur, float decay, float smooth, float vol = 0.6f, bool swell = false)
    {
        int n = Mathf.RoundToInt(dur * Rate);
        float[] d = new float[n];
        float last = 0f;
        System.Random rng = new System.Random(n);
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Rate;
            float white = (float)rng.NextDouble() * 2f - 1f;
            last = Mathf.Lerp(white, last, smooth);
            float e = swell ? Mathf.Sin(Mathf.PI * i / n) : Mathf.Exp(-t * decay) * Mathf.Min(1f, i / (Rate * 0.004f));
            d[i] = last * e * vol * (1f + smooth * 2f);
        }
        return d;
    }

    private static float[] Silence(float dur) => new float[Mathf.RoundToInt(dur * Rate)];

    private static float[] Concat(params float[][] parts)
    {
        int total = 0;
        foreach (float[] p in parts) total += p.Length;
        float[] d = new float[total];
        int o = 0;
        foreach (float[] p in parts) { p.CopyTo(d, o); o += p.Length; }
        return d;
    }

    private static float[] Mix(float[] a, float[] b)
    {
        float[] d = new float[Mathf.Max(a.Length, b.Length)];
        for (int i = 0; i < d.Length; i++)
            d[i] = Mathf.Clamp((i < a.Length ? a[i] : 0f) + (i < b.Length ? b[i] : 0f), -1f, 1f);
        return d;
    }
}
