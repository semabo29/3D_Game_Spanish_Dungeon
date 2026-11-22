using UnityEngine;
using System.Collections.Generic;

// Balanceo suave por el "viento"
public class Sway : MonoBehaviour
{
    private Quaternion baseRot;
    private float phase;

    void Start()
    {
        baseRot = transform.localRotation;
        phase = transform.position.z * 0.7f + Random.value;
    }

    void Update()
    {
        transform.localRotation = baseRot * Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 2.2f + phase) * 9f);
    }
}
