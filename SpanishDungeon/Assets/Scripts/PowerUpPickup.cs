using UnityEngine;
using System.Collections.Generic;

public class PowerUpPickup : MonoBehaviour
{
    private PowerUpType type;
    private Transform model;
    private float phase;
    private float sparkleTimer;

    public static PowerUpPickup Create(Transform parent, Vector3 objectPos, PowerUpType type)
    {
        GameObject go = new GameObject("PowerUp_" + type);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(objectPos.x, 0f, objectPos.z);
        PowerUpPickup p = go.AddComponent<PowerUpPickup>();
        p.type = type;

        Mesh mesh;
        switch (type)
        {
            case PowerUpType.Bocadillo: mesh = VoxelModels.Bocadillo(); break;
            case PowerUpType.Churros: mesh = VoxelModels.Churros(); break;
            case PowerUpType.Abanico: mesh = VoxelModels.Abanico(); break;
            case PowerUpType.Siesta: mesh = VoxelModels.Almohada(); break;
            default: mesh = VoxelModels.Jamon(); break;
        }
        GameObject m = VoxelLib.Spawn("Model", mesh, go.transform, go.transform.position + Vector3.up * 0.35f, Quaternion.identity);
        p.model = m.transform;

        // Brillito para que se vea desde lejos
        GameObject lightGo = new GameObject("Glow");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = Vector3.up * 0.8f;
        Light l = lightGo.AddComponent<Light>();
        l.type = LightType.Point;
        l.range = 1.8f;
        l.intensity = 1.6f;
        l.color = TintOf(type);
        l.shadows = LightShadows.None;
        return p;
    }

    public static Color TintOf(PowerUpType t)
    {
        switch (t)
        {
            case PowerUpType.Bocadillo: return new Color(1f, 0.45f, 0.45f);
            case PowerUpType.Churros: return new Color(1f, 0.8f, 0.3f);
            case PowerUpType.Abanico: return new Color(1f, 0.3f, 0.35f);
            case PowerUpType.Siesta: return new Color(0.55f, 0.75f, 1f);
            default: return new Color(1f, 0.65f, 0.4f);
        }
    }

    public static string Name(PowerUpType t)
    {
        switch (t)
        {
            case PowerUpType.Bocadillo: return "Bocadillo de calamares";
            case PowerUpType.Churros: return "Churros con chocolate";
            case PowerUpType.Abanico: return "Abanico flamenco";
            case PowerUpType.Siesta: return "Almohada de la siesta";
            default: return "Jamón pata negra";
        }
    }

    void Start()
    {
        phase = Random.value * 6f;
        gameObject.AddComponent<PopIn>();
    }

    void Update()
    {
        if (model != null)
        {
            phase += Time.deltaTime;
            model.localPosition = Vector3.up * (0.35f + Mathf.Sin(phase * 2.5f) * 0.08f);
            model.Rotate(0f, 80f * Time.deltaTime, 0f, Space.World);
        }

        sparkleTimer -= Time.deltaTime;
        if (sparkleTimer <= 0f)
        {
            sparkleTimer = Random.Range(0.4f, 0.8f);
            if (GameSettings.AmbientEffects) GameFeel.Dust(transform.position + Vector3.up * 0.6f, Color.Lerp(TintOf(type), Color.white, 0.5f), 1);
        }

        PlayerController player = PlayerController.Instance;
        if (player == null) return;
        if (Mathf.Abs(player.transform.position.x - transform.position.x) < 0.35f &&
            Mathf.Abs(player.transform.position.z - transform.position.z) < 0.35f)
        {
            PlayerPowerUps pu = player.GetComponent<PlayerPowerUps>();
            if (pu == null) pu = player.gameObject.AddComponent<PlayerPowerUps>();
            pu.Apply(type);
            GameFeel.VoxelBurst(transform.position + Vector3.up * 0.4f, TintOf(type), 12, 3f);
            Destroy(gameObject);
        }
    }
}
