using UnityEngine;
using System.Collections.Generic;

// Marca roja en el suelo para los pinchos (accesibilidad: "Avisos de peligro")
public class DangerMarker : MonoBehaviour
{
    private MeshRenderer rend;
    private MaterialPropertyBlock mpb;
    private float target;
    private float current;
    private bool pulse;

    public static DangerMarker Create(Transform parent, Vector3 floorPos)
    {
        GameObject go = new GameObject("MarcaPeligro");
        go.transform.SetParent(parent, false);
        go.transform.position = floorPos + Vector3.up * 0.04f;
        go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        go.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
        go.AddComponent<MeshFilter>().sharedMesh = VoxelLib.Quad;
        DangerMarker m = go.AddComponent<DangerMarker>();
        m.rend = go.AddComponent<MeshRenderer>();
        m.rend.sharedMaterial = VoxelLib.UnlitAlpha;
        m.rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        m.mpb = new MaterialPropertyBlock();
        m.Set(0f, false);
        return m;
    }

    // alpha: intensidad objetivo; pulse: parpadeo de aviso
    // always: se ve aunque los avisos estén desactivados (forma parte del ataque, como el del jefe)
    public void Set(float alpha, bool pulsing, bool always = false)
    {
        target = (always || GameSettings.DangerMarkers) ? alpha : 0f;
        pulse = pulsing;
    }

    void Update()
    {
        current = Mathf.MoveTowards(current, target, Time.deltaTime * 4f);
        float a = current;
        if (pulse) a *= 0.55f + 0.45f * Mathf.Sin(Time.time * 18f);
        mpb.SetColor("_Color", new Color(1f, 0.15f, 0.1f, a));
        rend.SetPropertyBlock(mpb);
        rend.enabled = a > 0.01f;
    }
}
