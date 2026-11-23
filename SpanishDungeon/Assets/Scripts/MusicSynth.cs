using UnityEngine;
using System.Collections.Generic;

// Banda sonora original generada por código: cada sala tiene su propio estilo
// (charanga, pasodoble, rumba, sevillanas, disco de chiringuito...) con melodía,
// acordes, bajo y percusión. Todo se sintetiza al vuelo, sin archivos de audio.
public static class MusicSynth
{
    public const int Rate = 22050;

    public enum Style { Menu, Charanga, Pasodoble, Rumba, Disco, Mancha, Carretera, NightDisco, Sevillanas, Verano, Boss, Victory }

    // ---------- Definición de cada estilo ----------

    private class Song
    {
        public float bpm = 120f;
        public int beatsPerBar = 4;
        public int keyMidi = 57;              // La 3
        public int[] scale = Major;
        public Chord[] partA, partB;          // 4 acordes por parte, 2 compases cada uno
        public int seed = 1;
        public Lead lead = Lead.Brass;
        public Accomp accomp = Accomp.Stabs;
        public Drums drums = Drums.March;
        public float leadGain = 0.32f;
        public bool castanets, palmas;
        public int bars = 16;
        public bool loop = true;
    }

    private struct Chord
    {
        public int root;   // semitonos sobre la tónica
        public bool minor;
        public Chord(int root, bool minor) { this.root = root; this.minor = minor; }
    }

    private enum Lead { Brass, Square, Pluck, Flute, Organ }
    private enum Accomp { Stabs, Strum, Rumba, Arp, Organ, Sevillanas }
    private enum Drums { None, March, Paso, Rumba, Disco, Soft, Epic, Sevillanas }

    private static readonly int[] Major = { 0, 2, 4, 5, 7, 9, 11 };
    private static readonly int[] Minor = { 0, 2, 3, 5, 7, 8, 10 };
    private static readonly int[] Phrygian = { 0, 1, 3, 5, 7, 8, 10 };   // modo frigio (cadencia andaluza)

    private static Chord M(int r) => new Chord(r, false);
    private static Chord m(int r) => new Chord(r, true);

    private static Song Define(Style style)
    {
        Song s = new Song();
        switch (style)
        {
            case Style.Menu:
                s.bpm = 112; s.keyMidi = 55; s.scale = Major; s.seed = 11;
                s.partA = new[] { M(0), M(7), M(5), M(7) };
                s.partB = new[] { M(0), M(5), M(7), M(0) };
                s.lead = Lead.Brass; s.accomp = Accomp.Stabs; s.drums = Drums.Paso; s.castanets = true;
                break;
            case Style.Charanga:   // San Fermín: charanga de peñas
                s.bpm = 138; s.keyMidi = 58; s.scale = Major; s.seed = 21;
                s.partA = new[] { M(0), M(7), M(0), M(7) };
                s.partB = new[] { M(5), M(0), M(7), M(0) };
                s.lead = Lead.Brass; s.accomp = Accomp.Stabs; s.drums = Drums.March; s.leadGain = 0.34f;
                break;
            case Style.Pasodoble:  // Plaza de toros
                s.bpm = 120; s.keyMidi = 52; s.scale = Phrygian; s.seed = 31;
                s.partA = new[] { M(0), m(5), M(1), M(0) };
                s.partB = new[] { M(0), m(5), M(8), M(0) };
                s.lead = Lead.Brass; s.accomp = Accomp.Stabs; s.drums = Drums.Paso; s.castanets = true;
                break;
            case Style.Rumba:      // Las Ramblas: rumba catalana
                s.bpm = 112; s.keyMidi = 52; s.scale = Phrygian; s.seed = 41;
                s.partA = new[] { m(5), M(3), M(1), M(0) };
                s.partB = new[] { m(5), M(3), M(1), M(0) };
                s.lead = Lead.Pluck; s.accomp = Accomp.Rumba; s.drums = Drums.Rumba; s.palmas = true; s.leadGain = 0.36f;
                break;
            case Style.Disco:      // Benidorm: chiringuito disco
                s.bpm = 124; s.keyMidi = 57; s.scale = Major; s.seed = 51;
                s.partA = new[] { M(0), m(9), M(5), M(7) };
                s.partB = new[] { M(5), M(7), m(9), M(7) };
                s.lead = Lead.Square; s.accomp = Accomp.Arp; s.drums = Drums.Disco;
                break;
            case Style.Mancha:     // La Mancha: guitarra y flauta, más pausado
                s.bpm = 96; s.beatsPerBar = 3; s.keyMidi = 50; s.scale = Minor; s.seed = 61;
                s.partA = new[] { m(0), M(10), M(8), M(7) };
                s.partB = new[] { m(5), m(0), M(8), M(7) };
                s.lead = Lead.Flute; s.accomp = Accomp.Strum; s.drums = Drums.Soft; s.leadGain = 0.3f;
                break;
            case Style.Carretera:  // A-7: rumba rockera de gasolinera
                s.bpm = 132; s.keyMidi = 52; s.scale = Phrygian; s.seed = 71;
                s.partA = new[] { M(0), M(1), M(0), M(1) };
                s.partB = new[] { m(5), M(3), M(1), M(0) };
                s.lead = Lead.Square; s.accomp = Accomp.Rumba; s.drums = Drums.Disco; s.palmas = true;
                break;
            case Style.NightDisco: // Magaluf de noche
                s.bpm = 126; s.keyMidi = 53; s.scale = Minor; s.seed = 81;
                s.partA = new[] { m(0), M(8), M(3), M(10) };
                s.partB = new[] { m(5), M(8), M(10), m(0) };
                s.lead = Lead.Square; s.accomp = Accomp.Arp; s.drums = Drums.Disco;
                break;
            case Style.Sevillanas: // Feria de Abril
                s.bpm = 150; s.beatsPerBar = 3; s.keyMidi = 52; s.scale = Phrygian; s.seed = 91;
                s.partA = new[] { m(5), M(3), M(1), M(0) };
                s.partB = new[] { M(3), M(8), M(1), M(0) };
                s.lead = Lead.Pluck; s.accomp = Accomp.Sevillanas; s.drums = Drums.Sevillanas; s.castanets = true; s.palmas = true;
                break;
            case Style.Verano:     // Lloret: pop de verano
                s.bpm = 116; s.keyMidi = 55; s.scale = Major; s.seed = 101;
                s.partA = new[] { M(0), M(7), m(9), M(5) };
                s.partB = new[] { M(5), M(7), M(0), M(7) };
                s.lead = Lead.Pluck; s.accomp = Accomp.Strum; s.drums = Drums.Disco; s.leadGain = 0.34f;
                break;
            case Style.Boss:       // El Congreso: marcha épica con órgano
                s.bpm = 128; s.keyMidi = 50; s.scale = Minor; s.seed = 111;
                s.partA = new[] { m(0), M(8), M(10), M(7) };
                s.partB = new[] { m(5), m(0), M(8), M(7) };
                s.lead = Lead.Brass; s.accomp = Accomp.Organ; s.drums = Drums.Epic; s.leadGain = 0.3f;
                break;
            case Style.Victory:
                s.bpm = 132; s.keyMidi = 58; s.scale = Major; s.seed = 121; s.bars = 4; s.loop = false;
                s.partA = new[] { M(0), M(5), M(7), M(0) };
                s.partB = s.partA;
                s.lead = Lead.Brass; s.accomp = Accomp.Stabs; s.drums = Drums.March;
                break;
        }
        return s;
    }

    public static Style StyleForRoom(int room)
    {
        switch (room)
        {
            case 1: return Style.Charanga;
            case 2: return Style.Pasodoble;
            case 3: return Style.Rumba;
            case 4: return Style.Disco;
            case 5: return Style.Mancha;
            case 6: return Style.Carretera;
            case 7: return Style.NightDisco;
            case 8: return Style.Sevillanas;
            case 9: return Style.Verano;
            case 10: return Style.Boss;
            default: return Style.Menu;
        }
    }

    public static string Title(Style style)
    {
        switch (style)
        {
            case Style.Charanga: return "Charanga de la peña El Botellín";
            case Style.Pasodoble: return "Pasodoble del miura despistado";
            case Style.Rumba: return "Rumba del mantero";
            case Style.Disco: return "Chiringuito Disco Inferno";
            case Style.Mancha: return "Romance del hidalgo con ciática";
            case Style.Carretera: return "Rumba de área de servicio";
            case Style.NightDisco: return "Magaluf, 4:00 AM";
            case Style.Sevillanas: return "Sevillanas del rebujito";
            case Style.Verano: return "Canción del verano (otra más)";
            case Style.Boss: return "Marcha del Decreto Ley";
            default: return "Pasodoble del menú";
        }
    }

    // ---------- Generación ----------

    public static float[] Generate(Style style)
    {
        Song s = Define(style);
        System.Random rng = new System.Random(s.seed);

        float beat = 60f / s.bpm;
        int bpb = s.beatsPerBar;
        int totalBeats = s.bars * bpb;
        int length = Mathf.CeilToInt(totalBeats * beat * Rate);
        if (!s.loop) length += Rate * 2; // cola final
        float[] mix = new float[length];
        Ctx c = new Ctx { buf = mix, loop = s.loop };

        // Progresión completa: 16 compases = A A B A' (cada acorde dura 2 compases en A/B de 8)
        Chord[] prog = new Chord[s.bars];
        for (int bar = 0; bar < s.bars; bar++)
        {
            int section = (bar / 8) % 2;
            Chord[] part = section == 0 ? s.partA : s.partB;
            prog[bar] = part[(bar / 2) % 4];
        }
        if (!s.loop) for (int bar = 0; bar < s.bars; bar++) prog[bar] = s.partA[bar % 4];

        // ----- Melodía: motivo de 2 compases reutilizado y adaptado a cada acorde -----
        List<float[]> motifRhythm = new List<float[]>();
        for (int k = 0; k < 2; k++) motifRhythm.Add(RandomRhythm(rng, bpb, k == 1));
        int[] contour = new int[16];
        for (int i = 0; i < contour.Length; i++) contour[i] = rng.Next(0, 10) < 6 ? (rng.Next(0, 2) == 0 ? 1 : -1) : (rng.Next(0, 3) - 1) * 2;

        int leadBase = s.keyMidi + 12;
        int prevDegree = 7; // grado en índice de escala relativo (empieza en la octava)
        for (int bar = 0; bar < s.bars; bar++)
        {
            Chord ch = prog[bar];
            float[] rhythm = motifRhythm[bar % 2];
            bool variation = (bar / 2) % 4 == 3;           // cada 4º par de compases varía
            bool cadence = bar == s.bars - 1 || (bar % 8 == 7);
            float t = BarStart(bar, bpb, beat);
            for (int n = 0; n < rhythm.Length; n++)
            {
                float dur = rhythm[n];
                if (dur < 0f) { t += -dur * beat; continue; } // silencio
                int step = contour[(n + (variation ? 3 : 0)) % contour.Length];
                int deg = prevDegree + step;
                // Primera nota del compás: nota del acorde más cercana
                if (n == 0 || (cadence && n == rhythm.Length - 1)) deg = NearestChordDegree(s.scale, ch, prevDegree);
                deg = Mathf.Clamp(deg, 2, 13);
                prevDegree = deg;
                int midi = leadBase + DegreeToSemitone(s.scale, deg);
                float f = MidiToFreq(midi);
                float noteLen = dur * beat * (cadence && n == rhythm.Length - 1 ? 0.98f : 0.85f);
                PlayLead(c, s.lead, t, f, noteLen, s.leadGain * (n == 0 ? 1f : 0.85f));
                t += dur * beat;
            }
        }

        // ----- Acompañamiento y bajo -----
        for (int bar = 0; bar < s.bars; bar++)
        {
            Chord ch = prog[bar];
            float t0 = BarStart(bar, bpb, beat);
            int root = s.keyMidi - 12 + ch.root;
            if (ch.root > 6) root -= 12;
            int third = ch.minor ? 3 : 4;
            int[] chordMidi = { s.keyMidi + ch.root, s.keyMidi + ch.root + third, s.keyMidi + ch.root + 7 };
            if (ch.root > 7) for (int i = 0; i < 3; i++) chordMidi[i] -= 12;

            // Bajo
            for (int b = 0; b < bpb; b++)
            {
                float tb = t0 + b * beat;
                switch (s.accomp)
                {
                    case Accomp.Arp:
                        Bass(c, tb, MidiToFreq(root), beat * 0.45f, 0.34f);
                        Bass(c, tb + beat * 0.5f, MidiToFreq(root + 12), beat * 0.4f, 0.26f);
                        break;
                    case Accomp.Organ:
                        Bass(c, tb, MidiToFreq(root), beat * 0.9f, 0.34f);
                        break;
                    case Accomp.Rumba:
                        if (b == 0 || b == 2) Bass(c, tb, MidiToFreq(root), beat * 0.7f, 0.36f);
                        if (b == 1 || b == 3) Bass(c, tb + beat * 0.5f, MidiToFreq(root + 7), beat * 0.45f, 0.3f);
                        break;
                    default:
                        if (bpb == 3) { if (b == 0) Bass(c, tb, MidiToFreq(root), beat * 0.9f, 0.36f); }
                        else Bass(c, tb, MidiToFreq(b % 2 == 0 ? root : root + 7), beat * 0.8f, 0.34f);
                        break;
                }
            }

            // Acordes
            switch (s.accomp)
            {
                case Accomp.Stabs:
                    for (int b = 0; b < bpb; b++)
                        if (b % 2 == 1) foreach (int nm in chordMidi) Brass(c, t0 + b * beat, MidiToFreq(nm), beat * 0.35f, 0.09f);
                    break;
                case Accomp.Strum:
                    for (int b = 0; b < bpb; b++) Strum(c, t0 + b * beat, chordMidi, b == 0 ? 0.16f : 0.1f, b % 2 == 1);
                    break;
                case Accomp.Rumba:
                    float[] pattern = { 0f, 0.5f, 0.75f, 1.25f, 1.5f, 2f, 2.5f, 2.75f, 3.25f, 3.5f };
                    for (int i = 0; i < pattern.Length; i++)
                        Strum(c, t0 + pattern[i] * beat, chordMidi, (pattern[i] % 1f == 0f) ? 0.15f : 0.09f, i % 2 == 1);
                    break;
                case Accomp.Sevillanas:
                    for (int b = 0; b < bpb; b++)
                    {
                        Strum(c, t0 + b * beat, chordMidi, b == 0 ? 0.17f : 0.11f, false);
                        Strum(c, t0 + (b + 0.5f) * beat, chordMidi, 0.06f, true);
                    }
                    break;
                case Accomp.Arp:
                    for (int i = 0; i < bpb * 2; i++)
                        Pluck(c, t0 + i * 0.5f * beat, MidiToFreq(chordMidi[i % 3] + 12), beat * 0.45f, 0.08f, 0.6f);
                    for (int b = 0; b < bpb; b += 2) foreach (int nm in chordMidi) OrganNote(c, t0 + b * beat, MidiToFreq(nm), beat * 1.9f, 0.035f);
                    break;
                case Accomp.Organ:
                    foreach (int nm in chordMidi) OrganNote(c, t0, MidiToFreq(nm), beat * bpb * 0.98f, 0.06f);
                    break;
            }

            // Percusión
            for (int b = 0; b < bpb; b++)
            {
                float tb = t0 + b * beat;
                bool fill = bar % 4 == 3 && b == bpb - 1;
                switch (s.drums)
                {
                    case Drums.March:
                        if (b % 2 == 0) Kick(c, tb, 0.5f);
                        Snare(c, tb + beat * 0.5f, 0.12f);
                        if (b % 2 == 1) Snare(c, tb, 0.3f);
                        if (fill) for (int k = 0; k < 4; k++) Snare(c, tb + k * beat * 0.25f, 0.18f + k * 0.04f);
                        break;
                    case Drums.Paso:
                        if (b % 2 == 0) Kick(c, tb, 0.45f);
                        if (b % 2 == 1) Snare(c, tb, 0.22f);
                        if (fill) { Snare(c, tb + beat * 0.5f, 0.25f); Snare(c, tb + beat * 0.75f, 0.3f); }
                        break;
                    case Drums.Rumba:
                        if (b == 0 || b == 2) Kick(c, tb, 0.35f);
                        Hat(c, tb + beat * 0.5f, 0.08f);
                        break;
                    case Drums.Disco:
                        Kick(c, tb, 0.55f);
                        Hat(c, tb + beat * 0.5f, 0.12f);
                        if (b % 2 == 1) Clap(c, tb, 0.22f);
                        if (fill) { Snare(c, tb + beat * 0.5f, 0.2f); Snare(c, tb + beat * 0.75f, 0.25f); }
                        break;
                    case Drums.Soft:
                        if (b == 0) Kick(c, tb, 0.3f);
                        Hat(c, tb, 0.04f);
                        break;
                    case Drums.Epic:
                        if (b % 2 == 0) Kick(c, tb, 0.6f); else Snare(c, tb, 0.28f);
                        Hat(c, tb + beat * 0.5f, 0.06f);
                        if (fill) for (int k = 0; k < 6; k++) Snare(c, tb + k * beat / 6f, 0.15f + k * 0.03f);
                        break;
                    case Drums.Sevillanas:
                        if (b == 0) Kick(c, tb, 0.35f);
                        break;
                }
                if (s.palmas && (bpb == 3 ? b > 0 : b % 2 == 1)) Clap(c, tb, 0.16f);
                if (s.castanets) Castanets(c, tb + beat * 0.5f, 0.12f, rng);
            }
        }

        // Final con acorde largo para la fanfarria de victoria
        if (!s.loop)
        {
            float tEnd = BarStart(s.bars, bpb, beat);
            int[] fin = { s.keyMidi, s.keyMidi + 4, s.keyMidi + 7, s.keyMidi + 12 };
            foreach (int nm in fin) Brass(c, tEnd, MidiToFreq(nm), 1.6f, 0.12f);
            Kick(c, tEnd, 0.6f);
        }

        Master(mix);
        return mix;
    }

    // ---------- Utilidades musicales ----------

    private static float BarStart(int bar, int bpb, float beat) => bar * bpb * beat;
    private static float MidiToFreq(int midi) => 440f * Mathf.Pow(2f, (midi - 69) / 12f);

    private static int DegreeToSemitone(int[] scale, int degree)
    {
        int oct = Mathf.FloorToInt(degree / 7f);
        int idx = degree - oct * 7;
        return scale[idx] + 12 * (oct - 1);
    }

    private static int NearestChordDegree(int[] scale, Chord ch, int fromDegree)
    {
        int best = fromDegree, bestDist = 999;
        int third = ch.minor ? 3 : 4;
        for (int d = 0; d <= 15; d++)
        {
            int semi = ((DegreeToSemitone(scale, d) % 12) + 12) % 12;
            int rel = ((semi - ch.root) % 12 + 12) % 12;
            if (rel != 0 && rel != third && rel != 7) continue;
            int dist = Mathf.Abs(d - fromDegree);
            if (dist < bestDist) { bestDist = dist; best = d; }
        }
        return best;
    }

    // Ritmos de un compás en negras (valores negativos = silencios)
    private static float[] RandomRhythm(System.Random rng, int bpb, bool answer)
    {
        if (bpb == 3)
        {
            float[][] r3 =
            {
                new[] { 1f, 0.5f, 0.5f, 1f },
                new[] { 0.5f, 0.5f, 0.5f, 0.5f, 1f },
                new[] { 1.5f, 0.5f, 1f },
                new[] { 1f, 1f, 1f },
            };
            return answer ? new[] { 2f, 1f } : r3[rng.Next(r3.Length)];
        }
        float[][] r4 =
        {
            new[] { 1f, 0.5f, 0.5f, 1f, 1f },
            new[] { 0.5f, 0.5f, 0.5f, 0.5f, 1f, 1f },
            new[] { 1.5f, 0.5f, 1f, 0.5f, 0.5f },
            new[] { 0.75f, 0.25f, 0.5f, 0.5f, 1f, 1f },
            new[] { 1f, 1f, 0.5f, 0.5f, 0.5f, 0.5f },
        };
        float[][] answers =
        {
            new[] { 1f, 1f, 2f },
            new[] { 0.5f, 0.5f, 1f, 2f },
            new[] { 1.5f, 0.5f, 2f },
        };
        return answer ? answers[rng.Next(answers.Length)] : r4[rng.Next(r4.Length)];
    }

    // ---------- Instrumentos ----------

    private class Ctx
    {
        public float[] buf;
        public bool loop;
    }

    private static void Add(Ctx c, int i, float v)
    {
        int n = c.buf.Length;
        if (i < 0) return;
        if (i >= n)
        {
            if (!c.loop) return;
            i %= n; // las colas se enrollan al principio para que el bucle sea perfecto
        }
        c.buf[i] += v;
    }

    private static float Env(float t, float dur, float attack, float release)
    {
        if (t < attack) return t / attack;
        if (t > dur) return Mathf.Max(0f, 1f - (t - dur) / release);
        return 1f;
    }

    private static void PlayLead(Ctx c, Lead lead, float start, float f, float dur, float amp)
    {
        switch (lead)
        {
            case Lead.Brass: Brass(c, start, f, dur, amp); break;
            case Lead.Square: Square(c, start, f, dur, amp * 0.6f); break;
            case Lead.Pluck: Pluck(c, start, f, Mathf.Max(dur, 0.4f), amp, 0.5f); break;
            case Lead.Flute: Flute(c, start, f, dur, amp); break;
            case Lead.Organ: OrganNote(c, start, f, dur, amp * 0.6f); break;
        }
    }

    // Metal: diente de sierra con filtro que se abre al atacar
    private static void Brass(Ctx c, float start, float f, float dur, float amp)
    {
        int s0 = Mathf.RoundToInt(start * Rate);
        float release = 0.06f;
        int len = Mathf.RoundToInt((dur + release) * Rate);
        float phase = 0f, lp = 0f;
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)Rate;
            float vib = 1f + (t > 0.15f ? Mathf.Sin(t * 33f) * 0.004f : 0f);
            phase += f * vib / Rate;
            float saw = 2f * (phase - Mathf.Floor(phase + 0.5f));
            float env = Env(t, dur, 0.025f, release);
            float cutoff = Mathf.Lerp(0.05f, 0.35f, Mathf.Min(1f, t / 0.06f)) * (0.7f + 0.3f * env);
            lp += (saw - lp) * cutoff;
            Add(c, s0 + i, lp * env * amp);
        }
    }

    private static void Square(Ctx c, float start, float f, float dur, float amp)
    {
        int s0 = Mathf.RoundToInt(start * Rate);
        float release = 0.04f;
        int len = Mathf.RoundToInt((dur + release) * Rate);
        float phase = 0f, lp = 0f;
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)Rate;
            phase += f * (1f + Mathf.Sin(t * 35f) * 0.003f) / Rate;
            float sq = (phase - Mathf.Floor(phase)) < 0.3f ? 1f : -1f;
            lp += (sq - lp) * 0.35f;
            Add(c, s0 + i, lp * Env(t, dur, 0.008f, release) * amp);
        }
    }

    private static void Flute(Ctx c, float start, float f, float dur, float amp)
    {
        int s0 = Mathf.RoundToInt(start * Rate);
        float release = 0.12f;
        int len = Mathf.RoundToInt((dur + release) * Rate);
        float phase = 0f;
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)Rate;
            phase += f * (1f + Mathf.Sin(t * 30f) * 0.006f * Mathf.Min(1f, t * 4f)) / Rate;
            float v = Mathf.Sin(phase * Mathf.PI * 2f) + 0.15f * Mathf.Sin(phase * Mathf.PI * 4f);
            Add(c, s0 + i, v * Env(t, dur, 0.05f, release) * amp);
        }
    }

    private static void OrganNote(Ctx c, float start, float f, float dur, float amp)
    {
        int s0 = Mathf.RoundToInt(start * Rate);
        float release = 0.1f;
        int len = Mathf.RoundToInt((dur + release) * Rate);
        float w = Mathf.PI * 2f * f / Rate;
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)Rate;
            float v = Mathf.Sin(w * i) + 0.5f * Mathf.Sin(2f * w * i) + 0.25f * Mathf.Sin(3f * w * i);
            Add(c, s0 + i, v * Env(t, dur, 0.03f, release) * amp);
        }
    }

    // Guitarra: Karplus-Strong
    private static void Pluck(Ctx c, float start, float f, float dur, float amp, float brightness)
    {
        int s0 = Mathf.RoundToInt(start * Rate);
        int period = Mathf.Max(2, Mathf.RoundToInt(Rate / f));
        float[] ring = new float[period];
        uint seed = (uint)(s0 * 2654435761u) ^ (uint)period;
        for (int i = 0; i < period; i++)
        {
            seed = seed * 1664525u + 1013904223u;
            ring[i] = ((seed >> 9) / 4194304f) * 2f - 1f;
        }
        int len = Mathf.RoundToInt(dur * Rate);
        float decay = 0.996f;
        int idx = 0;
        float blend = Mathf.Lerp(0.5f, 0.9f, brightness);
        for (int i = 0; i < len; i++)
        {
            int next = (idx + 1) % period;
            float v = ring[idx];
            ring[idx] = decay * (blend * ring[idx] + (1f - blend) * ring[next]);
            idx = next;
            float fade = i > len - 400 ? (len - i) / 400f : 1f;
            Add(c, s0 + i, v * amp * fade);
        }
    }

    // Rasgueo: las cuerdas suenan con un pelín de retraso entre ellas
    private static void Strum(Ctx c, float start, int[] chord, float amp, bool up)
    {
        for (int k = 0; k < chord.Length; k++)
        {
            int idx = up ? chord.Length - 1 - k : k;
            Pluck(c, start + k * 0.012f, MidiToFreq(chord[idx]), 0.45f, amp, 0.35f);
        }
    }

    private static void Bass(Ctx c, float start, float f, float dur, float amp)
    {
        int s0 = Mathf.RoundToInt(start * Rate);
        float release = 0.03f;
        int len = Mathf.RoundToInt((dur + release) * Rate);
        float phase = 0f;
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)Rate;
            phase += f / Rate;
            float p = phase - Mathf.Floor(phase);
            float tri = 4f * Mathf.Abs(p - 0.5f) - 1f;
            float env = Env(t, dur, 0.005f, release) * Mathf.Lerp(1f, 0.6f, Mathf.Min(1f, t / dur));
            Add(c, s0 + i, tri * env * amp);
        }
    }

    private static uint noiseSeed = 22222u;
    private static float Noise()
    {
        noiseSeed = noiseSeed * 1664525u + 1013904223u;
        return ((noiseSeed >> 9) / 4194304f) * 2f - 1f;
    }

    private static void Kick(Ctx c, float start, float amp)
    {
        int s0 = Mathf.RoundToInt(start * Rate);
        int len = Mathf.RoundToInt(0.28f * Rate);
        float phase = 0f;
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)Rate;
            float f = 45f + 90f * Mathf.Exp(-t * 30f);
            phase += f / Rate;
            Add(c, s0 + i, Mathf.Sin(phase * Mathf.PI * 2f) * Mathf.Exp(-t * 11f) * amp);
        }
    }

    private static void Snare(Ctx c, float start, float amp)
    {
        int s0 = Mathf.RoundToInt(start * Rate);
        int len = Mathf.RoundToInt(0.16f * Rate);
        float lp = 0f;
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)Rate;
            float n = Noise();
            lp += (n - lp) * 0.6f;
            float tone = Mathf.Sin(t * 190f * Mathf.PI * 2f) * Mathf.Exp(-t * 30f) * 0.5f;
            Add(c, s0 + i, (lp * Mathf.Exp(-t * 22f) + tone) * amp);
        }
    }

    private static void Hat(Ctx c, float start, float amp)
    {
        int s0 = Mathf.RoundToInt(start * Rate);
        int len = Mathf.RoundToInt(0.045f * Rate);
        float prev = 0f;
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)Rate;
            float n = Noise();
            float hp = n - prev;
            prev = n;
            Add(c, s0 + i, hp * Mathf.Exp(-t * 80f) * amp);
        }
    }

    private static void Clap(Ctx c, float start, float amp)
    {
        int s0 = Mathf.RoundToInt(start * Rate);
        int len = Mathf.RoundToInt(0.14f * Rate);
        float lp = 0f, prev = 0f;
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)Rate;
            float n = Noise();
            float hp = n - prev;
            prev = n;
            lp += (hp - lp) * 0.5f;
            // Tres golpecitos rápidos y una cola corta
            float env = t < 0.03f ? (Mathf.Repeat(t, 0.01f) < 0.004f ? 1f : 0.3f) : Mathf.Exp(-(t - 0.03f) * 35f);
            Add(c, s0 + i, lp * env * amp * 1.6f);
        }
    }

    private static void Castanets(Ctx c, float start, float amp, System.Random rng)
    {
        int clicks = rng.Next(0, 3) == 0 ? 3 : 2;
        for (int k = 0; k < clicks; k++)
        {
            int s0 = Mathf.RoundToInt((start + k * 0.035f) * Rate);
            int len = Mathf.RoundToInt(0.02f * Rate);
            float prev = 0f;
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)Rate;
                float n = Noise();
                float hp = n - prev;
                prev = n;
                float tone = Mathf.Sin(t * 2600f * Mathf.PI * 2f);
                Add(c, s0 + i, (hp * 0.6f + tone * 0.4f) * Mathf.Exp(-t * 220f) * amp);
            }
        }
    }

    // Normaliza y suaviza los picos
    private static void Master(float[] mix)
    {
        float peak = 0.0001f;
        for (int i = 0; i < mix.Length; i++) peak = Mathf.Max(peak, Mathf.Abs(mix[i]));
        float gain = 0.85f / peak;
        for (int i = 0; i < mix.Length; i++)
        {
            float v = mix[i] * gain * 1.3f;
            mix[i] = v / (1f + Mathf.Abs(v)) * 1.15f; // saturación suave
        }
    }
}
