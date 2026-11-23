using UnityEngine;
using System.Collections;
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
        public Chord[] partA, partB, partC;   // 4 acordes por parte, 2 compases cada uno (C = puente)
        public int seed = 1;
        public Lead lead = Lead.Brass;
        public Lead lead2 = Lead.Flute;       // instrumento del puente
        public Accomp accomp = Accomp.Stabs;
        public Drums drums = Drums.March;
        public float leadGain = 0.32f;
        public bool castanets, palmas;
        public bool ornament;                 // adornos flamencos en la última vuelta
        public int drive;                     // 0 = tranquilo, 1 = acción, 2 = mucha acción
        public int bars = 40;
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
                s.partC = new[] { m(9), M(5), m(2), M(7) };
                s.lead = Lead.Brass; s.lead2 = Lead.Pluck; s.accomp = Accomp.Stabs; s.drums = Drums.Paso; s.castanets = true;
                break;
            case Style.Charanga:   // San Fermín: charanga de peñas
                s.bpm = 146; s.keyMidi = 58; s.scale = Major; s.seed = 21; s.drive = 1;
                s.partA = new[] { M(0), M(7), M(0), M(7) };
                s.partB = new[] { M(5), M(0), M(7), M(0) };
                s.partC = new[] { m(9), M(5), M(2), M(7) };
                s.lead = Lead.Brass; s.lead2 = Lead.Flute; s.accomp = Accomp.Stabs; s.drums = Drums.March; s.leadGain = 0.34f;
                break;
            case Style.Pasodoble:  // Plaza de toros
                s.bpm = 132; s.keyMidi = 52; s.scale = Phrygian; s.seed = 31; s.drive = 1; s.ornament = true;
                s.partA = new[] { M(0), m(5), M(1), M(0) };
                s.partB = new[] { M(0), m(5), M(8), M(0) };
                s.partC = new[] { m(5), M(3), M(8), M(1) };
                s.lead = Lead.Brass; s.lead2 = Lead.Pluck; s.accomp = Accomp.Stabs; s.drums = Drums.March; s.castanets = true;
                break;
            case Style.Rumba:      // Las Ramblas: rumba catalana
                s.bpm = 124; s.keyMidi = 52; s.scale = Phrygian; s.seed = 41; s.drive = 1; s.ornament = true;
                s.partA = new[] { m(5), M(3), M(1), M(0) };
                s.partB = new[] { M(3), M(8), M(1), M(0) };
                s.partC = new[] { M(0), M(1), M(0), M(1) };
                s.lead = Lead.Pluck; s.lead2 = Lead.Flute; s.accomp = Accomp.Rumba; s.drums = Drums.Rumba; s.palmas = true; s.leadGain = 0.36f;
                break;
            case Style.Disco:      // Benidorm: chiringuito disco
                s.bpm = 128; s.keyMidi = 57; s.scale = Major; s.seed = 51; s.drive = 1;
                s.partA = new[] { M(0), m(9), M(5), M(7) };
                s.partB = new[] { M(5), M(7), m(9), M(7) };
                s.partC = new[] { m(2), M(7), m(4), m(9) };
                s.lead = Lead.Square; s.lead2 = Lead.Brass; s.accomp = Accomp.Arp; s.drums = Drums.Disco;
                break;
            case Style.Mancha:     // La Mancha: jota rápida de guitarra y flauta
                s.bpm = 172; s.beatsPerBar = 3; s.keyMidi = 50; s.scale = Minor; s.seed = 61; s.drive = 1;
                s.partA = new[] { m(0), M(10), M(8), M(7) };
                s.partB = new[] { m(5), m(0), M(8), M(7) };
                s.partC = new[] { M(3), M(10), m(5), M(7) };
                s.lead = Lead.Flute; s.lead2 = Lead.Pluck; s.accomp = Accomp.Sevillanas; s.drums = Drums.Sevillanas; s.castanets = true; s.leadGain = 0.3f;
                break;
            case Style.Carretera:  // A-7: rumba rockera de gasolinera
                s.bpm = 142; s.keyMidi = 52; s.scale = Phrygian; s.seed = 71; s.drive = 2; s.ornament = true;
                s.partA = new[] { M(0), M(1), M(0), M(1) };
                s.partB = new[] { m(5), M(3), M(1), M(0) };
                s.partC = new[] { M(8), M(1), M(8), M(1) };
                s.lead = Lead.Square; s.lead2 = Lead.Pluck; s.accomp = Accomp.Rumba; s.drums = Drums.Disco; s.palmas = true;
                break;
            case Style.NightDisco: // Magaluf de noche
                s.bpm = 132; s.keyMidi = 53; s.scale = Minor; s.seed = 81; s.drive = 2;
                s.partA = new[] { m(0), M(8), M(3), M(10) };
                s.partB = new[] { m(5), M(8), M(10), m(0) };
                s.partC = new[] { M(8), M(10), m(0), m(0) };
                s.lead = Lead.Square; s.lead2 = Lead.Organ; s.accomp = Accomp.Arp; s.drums = Drums.Disco;
                break;
            case Style.Sevillanas: // Feria de Abril
                s.bpm = 162; s.beatsPerBar = 3; s.keyMidi = 52; s.scale = Phrygian; s.seed = 91; s.drive = 1; s.ornament = true;
                s.partA = new[] { m(5), M(3), M(1), M(0) };
                s.partB = new[] { M(3), M(8), M(1), M(0) };
                s.partC = new[] { M(0), m(5), M(1), M(0) };
                s.lead = Lead.Pluck; s.lead2 = Lead.Flute; s.accomp = Accomp.Sevillanas; s.drums = Drums.Sevillanas; s.castanets = true; s.palmas = true;
                break;
            case Style.Verano:     // Lloret: pop de verano
                s.bpm = 126; s.keyMidi = 55; s.scale = Major; s.seed = 101; s.drive = 1;
                s.partA = new[] { M(0), M(7), m(9), M(5) };
                s.partB = new[] { M(5), M(7), M(0), M(7) };
                s.partC = new[] { m(9), m(4), M(5), M(7) };
                s.lead = Lead.Pluck; s.lead2 = Lead.Square; s.accomp = Accomp.Strum; s.drums = Drums.Disco; s.leadGain = 0.34f;
                break;
            case Style.Boss:       // El Congreso: marcha épica con órgano
                s.bpm = 146; s.keyMidi = 50; s.scale = Minor; s.seed = 111; s.drive = 2;
                s.partA = new[] { m(0), M(8), M(10), M(7) };
                s.partB = new[] { m(5), m(0), M(8), M(7) };
                s.partC = new[] { m(0), M(1), m(0), M(7) };
                s.lead = Lead.Brass; s.lead2 = Lead.Organ; s.accomp = Accomp.Organ; s.drums = Drums.Epic; s.leadGain = 0.3f;
                break;
            case Style.Victory:
                s.bpm = 132; s.keyMidi = 58; s.scale = Major; s.seed = 121; s.bars = 4; s.loop = false;
                s.partA = new[] { M(0), M(5), M(7), M(0) };
                s.partB = s.partA; s.partC = s.partA;
                s.lead = Lead.Brass; s.accomp = Accomp.Stabs; s.drums = Drums.March;
                break;
        }
        if (s.partC == null) s.partC = s.partB;
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
            case Style.Mancha: return "Jota del hidalgo con ciática";
            case Style.Carretera: return "Rumba de área de servicio";
            case Style.NightDisco: return "Magaluf, 4:00 AM";
            case Style.Sevillanas: return "Sevillanas del rebujito";
            case Style.Verano: return "Canción del verano (otra más)";
            case Style.Boss: return "Marcha del Decreto Ley";
            default: return "Pasodoble del menú";
        }
    }

    // ---------- Generación ----------

    // Secciones de la canción: A, A' (con segunda voz), B (otra melodía),
    // C (puente: parón y redoble que sube) y A'' (todo a tope, con adornos)
    private const int SecA1 = 0, SecA2 = 1, SecB = 2, SecC = 3, SecA3 = 4;
    private static readonly int[] Form = { SecA1, SecA2, SecB, SecC, SecA3 };

    private class Motif
    {
        public float[] call, answer, altAnswer;
        public int[] contour = new int[16];
    }

    private static Motif NewMotif(System.Random rng, int bpb, bool syncopated)
    {
        Motif mo = new Motif
        {
            call = RandomRhythm(rng, bpb, false, syncopated),
            answer = RandomRhythm(rng, bpb, true, false),
            altAnswer = RandomRhythm(rng, bpb, true, false),
        };
        for (int i = 0; i < mo.contour.Length; i++)
            mo.contour[i] = rng.Next(0, 10) < 6 ? (rng.Next(0, 2) == 0 ? 1 : -1) : (rng.Next(0, 3) - 1) * 2;
        return mo;
    }

    // Generación de golpe (para pruebas o si hace falta ya)
    public static float[] Generate(Style style)
    {
        float[] result = null;
        IEnumerator it = GenerateSteps(style, r => result = r);
        while (it.MoveNext()) { }
        return result;
    }

    // Generación por trozos: un compás por paso, para repartirla entre fotogramas sin tirones
    public static IEnumerator GenerateSteps(Style style, System.Action<float[]> done)
    {
        Song s = Define(style);
        System.Random rng = new System.Random(s.seed);
        noiseSeed = 22222u + (uint)s.seed;

        float beat = 60f / s.bpm;
        int bpb = s.beatsPerBar;
        int secLen = s.loop ? 8 : s.bars;
        int totalBeats = s.bars * bpb;
        int length = Mathf.CeilToInt(totalBeats * beat * Rate);
        if (!s.loop) length += Rate * 2; // cola final
        float[] mix = new float[length];
        Ctx c = new Ctx { buf = mix, loop = s.loop };

        Motif motifA = NewMotif(rng, bpb, false);
        Motif motifB = NewMotif(rng, bpb, true);
        Motif motifC = NewMotif(rng, bpb, false);

        int leadBase = s.keyMidi + 12;
        int prevDegree = 7; // grado en índice de escala relativo (empieza en la octava)

        for (int bar = 0; bar < s.bars; bar++)
        {
            int sec = s.loop ? Form[Mathf.Min(bar / secLen, Form.Length - 1)] : SecA1;
            int inSec = bar % secLen;
            Chord[] part = sec == SecB ? s.partB : sec == SecC ? s.partC : s.partA;
            Chord ch = s.loop ? part[(inSec / 2) % 4] : part[bar % 4];
            float t0 = BarStart(bar, bpb, beat);
            bool breakdown = s.loop && sec == SecC && inSec < 4;
            bool build = s.loop && sec == SecC && inSec >= 4;
            bool lastBar = inSec == secLen - 1;

            // ----- Melodía: motivo de 2 compases reutilizado y adaptado a cada acorde -----
            Motif mo = sec == SecB ? motifB : sec == SecC ? motifC : motifA;
            Lead leadInst = sec == SecC ? s.lead2 : s.lead;
            int pair = inSec / 2;
            bool isAnswer = inSec % 2 == 1;
            float[] rhythm = !isAnswer ? mo.call : (pair == 3 && sec != SecA1 ? mo.altAnswer : mo.answer);
            bool variation = pair == 3;
            bool cadence = lastBar;
            bool harmony = s.loop && (sec == SecA2 || sec == SecA3);
            bool grace = s.ornament && sec == SecA3;
            float secGain = breakdown ? 0.85f : 1f;
            {
                float t = t0;
                for (int n = 0; n < rhythm.Length; n++)
                {
                    float dur = rhythm[n];
                    if (dur < 0f) { t += -dur * beat; continue; } // silencio
                    int step = mo.contour[(n + (variation ? 3 : 0) + (isAnswer ? 5 : 0)) % mo.contour.Length];
                    int deg = prevDegree + step;
                    // Primera nota del compás: nota del acorde más cercana
                    if (n == 0 || (cadence && n == rhythm.Length - 1)) deg = NearestChordDegree(s.scale, ch, prevDegree);
                    deg = Mathf.Clamp(deg, 2, 13);
                    prevDegree = deg;
                    float f = MidiToFreq(leadBase + DegreeToSemitone(s.scale, deg));
                    float noteLen = dur * beat * (cadence && n == rhythm.Length - 1 ? 0.98f : 0.85f);
                    float amp = s.leadGain * secGain * (n == 0 ? 1f : 0.85f);
                    if (grace && n == 0 && dur >= 0.5f)
                    {
                        // Adorno: nota de paso rapidita desde arriba
                        float g = beat * 0.12f;
                        PlayLead(c, leadInst, t, MidiToFreq(leadBase + DegreeToSemitone(s.scale, deg + 1)), g * 0.9f, amp * 0.7f);
                        PlayLead(c, leadInst, t + g, f, noteLen - g, amp);
                    }
                    else PlayLead(c, leadInst, t, f, noteLen, amp);
                    // Segunda voz una tercera por debajo
                    if (harmony && dur >= 0.5f)
                        PlayLead(c, leadInst, t, MidiToFreq(leadBase + DegreeToSemitone(s.scale, deg - 2)), noteLen, amp * 0.42f);
                    t += dur * beat;
                }
            }

            // ----- Acompañamiento y bajo -----
            int root = s.keyMidi - 12 + ch.root;
            if (ch.root > 6) root -= 12;
            int third = ch.minor ? 3 : 4;
            int[] chordMidi = { s.keyMidi + ch.root, s.keyMidi + ch.root + third, s.keyMidi + ch.root + 7 };
            if (ch.root > 7) for (int i = 0; i < 3; i++) chordMidi[i] -= 12;

            // Bajo
            if (breakdown)
            {
                Bass(c, t0, MidiToFreq(root), beat * bpb * 0.95f, 0.32f);
            }
            else if (s.drive > 0 && s.accomp != Accomp.Rumba && s.accomp != Accomp.Arp)
            {
                // Bajo de acción: corcheas que empujan, con salto a la quinta al final de cada frase
                for (int i = 0; i < bpb * 2; i++)
                {
                    int note = root;
                    if (i % 2 == 1) note = s.accomp == Accomp.Organ ? root : root + 12;
                    if (isAnswer && i == bpb * 2 - 1) note = root + 7;
                    Bass(c, t0 + i * beat * 0.5f, MidiToFreq(note), beat * 0.42f, i % 2 == 0 ? 0.34f : 0.24f);
                }
            }
            else
            {
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
            }

            // Acordes
            if (breakdown)
            {
                foreach (int nm in chordMidi) OrganNote(c, t0, MidiToFreq(nm), beat * bpb * 0.98f, 0.04f);
                if (s.accomp != Accomp.Organ) Strum(c, t0, chordMidi, 0.12f, false);
            }
            else
            {
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
                        // En la marcha del jefe, golpes de metal a contratiempo para meter tensión
                        if (s.drive > 1 && sec != SecA1)
                            for (int b = 0; b < bpb; b++) foreach (int nm in chordMidi) Brass(c, t0 + (b + 0.5f) * beat, MidiToFreq(nm), beat * 0.22f, 0.05f);
                        break;
                }
            }

            yield return null;

            // Percusión
            Percussion(c, s, rng, t0, beat, bpb, sec, inSec, secLen, breakdown, build, lastBar);

            yield return null;
        }

        // Final con acorde largo para la fanfarria de victoria
        if (!s.loop)
        {
            float tEnd = BarStart(s.bars, bpb, beat);
            int[] fin = { s.keyMidi, s.keyMidi + 4, s.keyMidi + 7, s.keyMidi + 12 };
            foreach (int nm in fin) Brass(c, tEnd, MidiToFreq(nm), 1.6f, 0.12f);
            Kick(c, tEnd, 0.6f);
            Crash(c, tEnd, 0.2f);
        }

        Master(mix);
        done?.Invoke(mix);
    }

    private static void Percussion(Ctx c, Song s, System.Random rng, float t0, float beat, int bpb,
                                   int sec, int inSec, int secLen, bool breakdown, bool build, bool lastBar)
    {
        bool action = s.drive > 0;

        // Platillo al empezar cada sección
        if (s.loop && inSec == 0 && sec != SecC && (action || sec == SecB)) Crash(c, t0, 0.2f);

        if (breakdown && action)
        {
            // Parón: solo bombo al principio y charles suave
            Kick(c, t0, 0.45f);
            for (int k = 0; k < bpb * 2; k++) Hat(c, t0 + k * beat * 0.5f, 0.04f);
            if (s.castanets) for (int b = 0; b < bpb; b++) Castanets(c, t0 + (b + 0.5f) * beat, 0.09f, rng);
            return;
        }

        if (build && action)
        {
            // Redoble que sube: corcheas y luego semicorcheas, cada vez más fuerte
            int sub = inSec >= secLen - 2 ? 4 : 2;
            for (int b = 0; b < bpb; b++)
            {
                float tb = t0 + b * beat;
                Kick(c, tb, 0.5f);
                for (int k = 0; k < sub; k++)
                {
                    float prog = ((inSec - 4) * bpb * sub + b * sub + k) / (4f * bpb * sub);
                    Snare(c, tb + k * beat / sub, 0.07f + 0.25f * Mathf.Clamp01(prog));
                }
            }
            return;
        }

        for (int b = 0; b < bpb; b++)
        {
            float tb = t0 + b * beat;
            bool fill = inSec % 4 == 3 && b == bpb - 1 && !(lastBar && action);
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
                    if (b == 0 || b == 2) Kick(c, tb, 0.4f);
                    if (action && (b == 1 || b == 3)) Snare(c, tb, 0.16f);
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
                    if (s.drive > 1) Kick(c, tb + beat * 0.5f, 0.32f); // doble bombo
                    Hat(c, tb + beat * 0.5f, 0.06f);
                    if (fill) for (int k = 0; k < 6; k++) Snare(c, tb + k * beat / 6f, 0.15f + k * 0.03f);
                    break;
                case Drums.Sevillanas:
                    if (b == 0) Kick(c, tb, 0.4f);
                    if (action && b > 0) Snare(c, tb, 0.1f);
                    break;
            }
            if (s.palmas && (bpb == 3 ? b > 0 : b % 2 == 1)) Clap(c, tb, 0.16f);
            if (s.castanets) Castanets(c, tb + beat * 0.5f, 0.12f, rng);
        }

        if (!action) return;

        // Charles en corcheas (y semicorcheas en las partes fuertes de los temas más cañeros)
        bool sixteenths = s.drive > 1 && (sec == SecB || sec == SecA3);
        int hatSub = sixteenths ? 4 : 2;
        for (int k = 0; k < bpb * hatSub; k++)
            Hat(c, t0 + k * beat / hatSub, k % hatSub == 0 ? 0.035f : 0.055f);

        // Redoble de timbales al final de cada sección
        if (lastBar && s.loop)
        {
            float tl = t0 + (bpb - 1) * beat;
            float[] toms = { 260f, 210f, 165f, 125f };
            for (int k = 0; k < 4; k++) Tom(c, tl + k * beat * 0.25f, toms[k], 0.3f + k * 0.03f);
        }
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
    private static float[] RandomRhythm(System.Random rng, int bpb, bool answer, bool syncopated)
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
            float[][] a3 = { new[] { 2f, 1f }, new[] { 1f, 2f }, new[] { 0.5f, 0.5f, 2f } };
            return answer ? a3[rng.Next(a3.Length)] : r3[rng.Next(r3.Length)];
        }
        float[][] r4 =
        {
            new[] { 1f, 0.5f, 0.5f, 1f, 1f },
            new[] { 0.5f, 0.5f, 0.5f, 0.5f, 1f, 1f },
            new[] { 1.5f, 0.5f, 1f, 0.5f, 0.5f },
            new[] { 0.75f, 0.25f, 0.5f, 0.5f, 1f, 1f },
            new[] { 1f, 1f, 0.5f, 0.5f, 0.5f, 0.5f },
        };
        float[][] synco =
        {
            new[] { -0.5f, 0.5f, 0.5f, 0.5f, 1f, 1f },
            new[] { 0.5f, 1f, 0.5f, 1f, 1f },
            new[] { 0.75f, 0.75f, 0.5f, 1f, 1f },
            new[] { -0.5f, 1f, 0.5f, 0.5f, 0.5f, 1f },
        };
        if (syncopated && !answer) return synco[rng.Next(synco.Length)];
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

    // Timbal: tono que cae rápido
    private static void Tom(Ctx c, float start, float f, float amp)
    {
        int s0 = Mathf.RoundToInt(start * Rate);
        int len = Mathf.RoundToInt(0.3f * Rate);
        float phase = 0f;
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)Rate;
            float fr = f * (1f + 0.6f * Mathf.Exp(-t * 25f));
            phase += fr / Rate;
            float v = Mathf.Sin(phase * Mathf.PI * 2f) + Noise() * 0.15f * Mathf.Exp(-t * 60f);
            Add(c, s0 + i, v * Mathf.Exp(-t * 9f) * amp);
        }
    }

    // Platillo: ruido agudo con cola larga
    private static void Crash(Ctx c, float start, float amp)
    {
        int s0 = Mathf.RoundToInt(start * Rate);
        int len = Mathf.RoundToInt(1.4f * Rate);
        float prev = 0f, prev2 = 0f;
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)Rate;
            float n = Noise();
            float hp = n - 2f * prev + prev2;
            prev2 = prev; prev = n;
            float env = Mathf.Min(1f, t * 400f) * Mathf.Exp(-t * 3.2f);
            Add(c, s0 + i, hp * 0.5f * env * amp);
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
