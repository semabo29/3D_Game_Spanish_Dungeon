using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class PedroStats : MonoBehaviour
{
    [Header("Estadísticas de Pedro")]
    public int maxVidas = 3;
    private int vidasActuales;

    [Header("Botín")]
    public int bonusCoinsOnDeath = 10;

    [Header("Inmunidad y Efecto Visual")]
    public float invincibilityTime = 1.5f;
    public float blinkInterval = 0.1f;

    private bool isDead = false;
    private bool isInvincible = false;

    private List<Renderer> activeRenderers = new List<Renderer>();

    public int VidasActuales => vidasActuales;
    public bool IsLastLife => vidasActuales == 1;

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

        GameHUD hud = GameHUD.Create();
        if (hud != null)
        {
            hud.RegisterBoss(this);
            hud.ShowToast("¡Pedro, el Presidente, no piensa dimitir!", new Color(1f, 0.35f, 0.3f), 2f);
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

        Vector3 pos = transform.position;
        GameFeel.VoxelBurst(pos, new Color(0.12f, 0.14f, 0.25f), 30, 6f);  // traje
        GameFeel.VoxelBurst(pos, new Color(0.8f, 0.1f, 0.15f), 12, 5f);    // corbata
        GameFeel.VoxelBurst(pos, new Color(1f, 0.82f, 0.15f), 16, 5f);     // monedas
        GameFeel.Shake(0.45f, 0.6f);
        GameFeel.Hitstop(0.25f);
        GameFeel.FloatingText(pos, "¡HA DIMITIDO!", new Color(1f, 0.82f, 0.15f), 7f);
        if (GameHUD.Instance != null)
        {
            GameHUD.Instance.ShowToast("¡PEDRO HA DIMITIDO! Cruza la puerta", new Color(1f, 0.82f, 0.15f), 2.2f);
        }

        // Botín: monedas para el jugador
        PlayerController player = PlayerController.Instance;
        if (player != null)
        {
            PlayerStats ps = player.GetComponent<PlayerStats>();
            if (ps != null) for (int i = 0; i < bonusCoinsOnDeath; i++) ps.AddCoin();
        }

        Destroy(gameObject);
    }
}