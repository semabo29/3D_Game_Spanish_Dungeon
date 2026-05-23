using UnityEngine;

public class CameraController : MonoBehaviour
{
    // El objeto que la cámara debe seguir (nuestro Player)
    [SerializeField] private Transform target;

    // Velocidad de amortiguación (cuanto más bajo, más suave y "perezosa" sigue al jugador)
    [SerializeField] private float smoothSpeed = 0.125f;

    // Distancia fija que mantendrá la cámara respecto al jugador
    private Vector3 offset;

    void Start()
    {
        if (target != null)
        {
            // Calculamos la distancia inicial en base a cómo colocaste la cámara en el editor
            offset = transform.position - target.position;
        }
    }

    // LateUpdate se ejecuta justo después del Update del jugador. Ideal para cámaras.
    void LateUpdate()
    {
        if (target == null) return;

        // La posición ideal a la que debería ir la cámara
        Vector3 desiredPosition = target.position + offset;

        // Interpolación lineal (goma elástica) entre la posición actual de la cámara y la ideal
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Aplicamos la posición corregida
        transform.position = smoothedPosition;
    }
}