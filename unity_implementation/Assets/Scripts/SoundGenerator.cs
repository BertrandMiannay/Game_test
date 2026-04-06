using System;
using UnityEngine;

/// <summary>
/// Generates all game sounds procedurally as AudioClips using PCM data.
/// Ported from Godot's sound_generator.gd (16-bit logic → Unity float[-1,1]).
/// </summary>
public static class SoundGenerator
{
    const int SAMPLE_RATE = 44100;

    // ── P90 gunshot: white noise burst + low thump ────────────────────────────
    public static AudioClip MakeP90()
    {
        float dur = 0.07f;
        int   n   = (int)(SAMPLE_RATE * dur);
        var   buf = new float[n];
        var   rng = new System.Random(42);

        for (int i = 0; i < n; i++)
        {
            float t        = (float)i / SAMPLE_RATE;
            float envelope = Mathf.Exp(-t * 25f);
            float noise    = (float)(rng.NextDouble() * 2.0 - 1.0) * 0.2f;
            float thump    = Mathf.Sin(2f * Mathf.PI * 32f * t) * 0.85f;
            float sub      = Mathf.Sin(2f * Mathf.PI * 18f * t) * 0.4f;
            buf[i]         = Mathf.Clamp((noise + thump + sub) * envelope, -1f, 1f);
        }
        return MakeClip("P90", buf);
    }

    // ── Zat'nik'tel: frequency sweep 700→180 Hz ───────────────────────────────
    public static AudioClip MakeZat()
    {
        float dur = 0.24f;
        int   n   = (int)(SAMPLE_RATE * dur);
        var   buf = new float[n];
        float phase = 0f;

        for (int i = 0; i < n; i++)
        {
            float t        = (float)i / SAMPLE_RATE;
            float envelope = Mathf.Exp(-t * 9f);
            // Instantaneous frequency: 700 → 180 Hz linear sweep
            float freq  = 700f + (180f - 700f) * (t / dur);
            phase      += 2f * Mathf.PI * freq / SAMPLE_RATE;
            float main  = Mathf.Sin(phase) * 0.85f;
            float harm  = Mathf.Sin(phase * 2f) * 0.15f;
            buf[i]      = Mathf.Clamp((main + harm) * envelope, -1f, 1f);
        }
        return MakeClip("Zat", buf);
    }

    // ── Staff weapon: plasma crackle 220→60 Hz ───────────────────────────────
    public static AudioClip MakeStaff()
    {
        float dur = 0.30f;
        int   n   = (int)(SAMPLE_RATE * dur);
        var   buf = new float[n];
        float phase = 0f;

        for (int i = 0; i < n; i++)
        {
            float t        = (float)i / SAMPLE_RATE;
            float envelope = Mathf.Exp(-t * 12f);
            float freq     = 220f + (60f - 220f) * (t / dur);
            phase         += 2f * Mathf.PI * freq / SAMPLE_RATE;
            float body     = Mathf.Sin(phase) * 0.75f;
            float crack    = Mathf.Sin(phase * 3f) * 0.2f * Mathf.Exp(-t * 30f);
            buf[i]         = Mathf.Clamp((body + crack) * envelope, -1f, 1f);
        }
        return MakeClip("Staff", buf);
    }

    // ─────────────────────────────────────────────────────────────────────────
    static AudioClip MakeClip(string clipName, float[] data)
    {
        var clip = AudioClip.Create(clipName, data.Length, 1, SAMPLE_RATE, false);
        clip.SetData(data, 0);
        return clip;
    }
}
