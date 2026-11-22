using UnityEngine;
using System.Collections.Generic;

// ======================================================================
//  Palomas: picotean el suelo y salen volando cuando te acercas
// ======================================================================
public class Pigeon : MonoBehaviour
{
    private Transform model;
    private bool flying;
    private Vector3 velocity;
    private float peckTimer;
    private float life = 3f;
    private Vector3 baseScale;

    public static void Create(Transform parent, Vector3 objectPos)
    {
        GameObject go = new GameObject("Paloma");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(objectPos.x + Random.Range(-0.25f, 0.25f), 0f, objectPos.z + Random.Range(-0.25f, 0.25f));
        go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        Pigeon p = go.AddComponent<Pigeon>();
        GameObject m = VoxelLib.Spawn("Model", VoxelModels.Paloma(Random.value < 0.15f ? 1 : 0), go.transform, go.transform.position, go.transform.rotation);
        p.model = m.transform;
        p.baseScale = m.transform.localScale;
    }

    void Update()
    {
        if (!flying)
        {
            peckTimer -= Time.deltaTime;
            if (peckTimer <= 0f)
            {
                peckTimer = Random.Range(0.6f, 2f);
                if (Random.value < 0.3f) transform.Rotate(0f, Random.Range(-90f, 90f), 0f);
                StartCoroutine(Peck());
            }

            PlayerController player = PlayerController.Instance;
            if (player != null && Vector3.Distance(Flat(player.transform.position), Flat(transform.position)) < 1.7f)
            {
                FlyAway(transform.position - player.transform.position);
            }
        }
        else
        {
            velocity += Vector3.up * 2f * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            float flap = 0.6f + 0.4f * Mathf.Abs(Mathf.Sin(Time.time * 30f));
            if (model != null) model.localScale = new Vector3(baseScale.x * (1.3f - flap * 0.3f), baseScale.y * flap, baseScale.z);
            life -= Time.deltaTime;
            if (life <= 0f) Destroy(gameObject);
        }
    }

    private System.Collections.IEnumerator Peck()
    {
        if (model == null) yield break;
        Quaternion baseRot = model.localRotation;
        for (int i = 0; i < 2; i++)
        {
            model.localRotation = baseRot * Quaternion.Euler(25f, 0f, 0f);
            yield return new WaitForSeconds(0.08f);
            if (model == null) yield break;
            model.localRotation = baseRot;
            yield return new WaitForSeconds(0.08f);
        }
    }

    public void FlyAway(Vector3 away)
    {
        if (flying) return;
        flying = true;
        StopAllCoroutines();
        away.y = 0f;
        if (away.sqrMagnitude < 0.01f) away = Random.insideUnitSphere;
        away.y = 0f;
        velocity = away.normalized * 2.5f + Vector3.up * 2.5f;
        transform.rotation = Quaternion.LookRotation(-away.normalized);
        transform.SetParent(null, true);
        Sfx.Play(SfxType.Flap, 0.5f, Random.Range(0.9f, 1.2f));
        if (Random.value < 0.35f)
        {
            Sfx.Play(SfxType.Coo, 0.5f);
            GameFeel.FloatingText(transform.position, Random.value < 0.5f ? "¡Currucucú!" : "¡Cu-cú!", new Color(0.8f, 0.85f, 0.9f), 3f);
        }
        GameFeel.Dust(transform.position, new Color(0.85f, 0.85f, 0.88f), 3);
        Achievements.OnPigeon();
    }

    private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
}
