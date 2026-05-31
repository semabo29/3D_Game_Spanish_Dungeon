using UnityEngine;
using System.Collections;

public class ReiStats : MonoBehaviour
{
    [Header("Estadísticas del Jefe")]
    public int maxVidas = 3;
    private int vidasActuales;

    [Header("Invulnerabilidad y Parpadeo")]
    public float invincibilityTime = 1.5f; // Mismo tiempo que dura la inmunidad
    public float blinkInterval = 0.1f;

    private bool isDead = false;
    private bool isInvincible = false;

    // Lista para guardar todos los renderers del modelo del jefe
    private Renderer[] bossRenderers;

    void Start()
    {
        vidasActuales = maxVidas;

        // Buscamos TODOS los renderers en el objeto y en sus hijos (cuerpo, ropa, armas, etc.)
        bossRenderers = GetComponentsInChildren<Renderer>();
    }

    public void TakeDamage()
    {
        if (isDead || isInvincible) return;

        vidasActuales--;
        Debug.Log("¡El Rey Emérito recibe un golpe! Vidas restantes: " + vidasActuales);

        if (vidasActuales <= 0)
        {
            Die();
        }
        else
        {
            StartInvincibility(); // Activa inmunidad y parpadeo al recibir daño
        }
    }

    // Método público para poder activar la invulnerabilidad desde fuera (cuando el rey ataca)
    public void StartInvincibility()
    {
        if (isDead) return;

        // Detenemos cualquier parpadeo previo activo para evitar conflictos
        StopAllCoroutines();
        StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float timer = 0f;

        while (timer < invincibilityTime)
        {
            // Alternamos la visibilidad de TODOS los renderers encontrados
            foreach (Renderer ren in bossRenderers)
            {
                if (ren != null) ren.enabled = !ren.enabled;
            }

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        // Al terminar, nos aseguramos por seguridad de que todos vuelvan a estar visibles
        foreach (Renderer ren in bossRenderers)
        {
            if (ren != null) ren.enabled = true;
        }

        isInvincible = false;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    public bool IsDead()
    {
        return isDead;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("¡Jefe derrotado de manera absoluta!");
        Destroy(gameObject);
    }
}