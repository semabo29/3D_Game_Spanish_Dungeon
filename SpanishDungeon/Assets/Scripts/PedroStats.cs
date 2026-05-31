using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class PedroStats : MonoBehaviour
{
    [Header("Estadísticas de Pedro")]
    public int maxVidas = 3;
    private int vidasActuales;

    [Header("Inmunidad y Efecto Visual")]
    public float invincibilityTime = 1.5f;
    public float blinkInterval = 0.1f;

    private bool isDead = false;
    private bool isInvincible = false;

    private List<Renderer> activeRenderers = new List<Renderer>();

    void Start()
    {
        vidasActuales = maxVidas;

        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer ren in allRenderers)
        {
            if (ren != null && ren.enabled)
            {
                activeRenderers.Add(ren); 
            }
        }
    }

    public void TakeDamage()
    {
        if (isDead || isInvincible) return;

        vidasActuales--;
        Debug.Log("¡Pedro recibe un impacto! Vidas restantes: " + vidasActuales);

        if (vidasActuales <= 0)
        {
            Die();
        }
        else
        {
            StartInvincibility();
        }
    }

    public void StartInvincibility()
    {
        if (isDead) return;

        StopAllCoroutines();
        StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float timer = 0f;

        while (timer < invincibilityTime)
        {
            foreach (Renderer ren in activeRenderers)
            {
                if (ren != null) ren.enabled = !ren.enabled;
            }
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        foreach (Renderer ren in activeRenderers)
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
        Debug.Log("¡Pedro ha sido derrotado!");
        Destroy(gameObject);
    }
}