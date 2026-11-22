using UnityEngine;
using System.Collections.Generic;

// Flecha flotante sobre la salida cuando se abre la puerta
public class ExitArrow : MonoBehaviour
{
    private Vector3 basePos;

    public static void Create(Transform parent, Vector3 exitPos)
    {
        GameObject go = VoxelLib.Spawn("FlechaSalida", VoxelModels.ExitArrow(), parent, exitPos + Vector3.up * 1.6f, Quaternion.identity, false);
        Camera cam = Camera.main;
        if (cam != null) go.transform.rotation = Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f);
        go.AddComponent<ExitArrow>();
        go.AddComponent<PopIn>();
    }

    void Start() { basePos = transform.position; }

    void Update()
    {
        transform.position = basePos + Vector3.up * Mathf.Abs(Mathf.Sin(Time.time * 4f)) * 0.25f;
    }
}
