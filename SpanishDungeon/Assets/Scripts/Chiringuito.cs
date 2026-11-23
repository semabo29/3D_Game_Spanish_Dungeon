using UnityEngine;
using System;
using System.Collections.Generic;

// "El chiringuito de Paco": tienda entre salas (al salir de las salas 3, 6 y 9).
// Las monedas por fin sirven para algo más que para la lotería.
public static class Chiringuito
{
    public class Item
    {
        public string name, desc;
        public int price;
        public Func<PlayerStats, string> blocked;   // motivo por el que no se puede comprar (null = se puede)
        public Action<PlayerStats> buy;
    }

    public static bool OpensAfterRoom(int room) => room == 3 || room == 6 || room == 9;

    public static string Greeting(int room)
    {
        string[] lines =
        {
            "¿Qué te pongo, campeón?",
            "Hoy tenemos el bocata a precio de 2007.",
            "Aquí se paga en monedas de la mazmorra, ¿eh? Nada de Bizum.",
            "El seguro es buenísimo. Lo vende mi cuñado.",
            "El décimo de hoy tiene muy buena pinta. Como todos.",
            "Si vas a por Pedro, llévate algo. Créeme.",
        };
        if (room >= 9) return "Si vas a por Pedro, llévate algo. Créeme.";
        return lines[UnityEngine.Random.Range(0, lines.Length - 1)];
    }

    public static readonly List<Item> Items = new List<Item>
    {
        new Item
        {
            name = "Bocadillo de calamares", desc = "+1 corazón", price = 5,
            blocked = s => s.CurrentHealth >= s.healthCap ? "Estás a reventar" : null,
            buy = s => { s.Heal(1); Achievements.OnBocadillo(); },
        },
        new Item
        {
            name = "Caña y tapa", desc = "Recupera todos los corazones", price = 9,
            blocked = s => s.CurrentHealth >= s.maxHealth ? "Ya estás como nuevo" : null,
            buy = s => s.Heal(Mathf.Max(1, s.maxHealth - s.CurrentHealth)),
        },
        new Item
        {
            name = "Abanico flamenco", desc = "Para el siguiente golpe", price = 4,
            blocked = s => { PlayerPowerUps p = s.GetComponent<PlayerPowerUps>(); return p != null && p.HasShield ? "Ya llevas uno" : null; },
            buy = s => { PlayerPowerUps p = s.GetComponent<PlayerPowerUps>(); if (p != null) p.Apply(PowerUpType.Abanico); },
        },
        new Item
        {
            name = "Churros para llevar", desc = "Más velocidad al empezar la sala", price = 3,
            blocked = s => null,
            buy = s => { PlayerPowerUps p = s.GetComponent<PlayerPowerUps>(); if (p != null) p.Apply(PowerUpType.Churros); },
        },
        new Item
        {
            name = "Seguro a todo riesgo", desc = "Te salva una vez de morir de un golpe (no cubre caídas: letra pequeña)", price = 12,
            blocked = s => { PlayerPowerUps p = s.GetComponent<PlayerPowerUps>(); return p != null && p.HasInsurance ? "Ya estás asegurado" : null; },
            buy = s => { PlayerPowerUps p = s.GetComponent<PlayerPowerUps>(); if (p != null) p.GiveInsurance(); },
        },
        new Item
        {
            name = "Décimo de lotería", desc = "Puede tocar algo... o no", price = 2,
            blocked = s => null,
            buy = s => Decimo(s),
        },
    };

    public static string LastResult;

    private static void Decimo(PlayerStats s)
    {
        float r = UnityEngine.Random.value;
        if (r < 0.06f)
        {
            LastResult = "¡EL GORDO! Paco no se lo cree.";
            s.WinLotteryPrize();
        }
        else if (r < 0.3f)
        {
            LastResult = "¡Ha tocado! +6 monedas";
            for (int i = 0; i < 6; i++) s.AddCoin();
        }
        else if (r < 0.5f)
        {
            LastResult = "Reintegro: te devuelven lo jugado";
            for (int i = 0; i < 2; i++) s.AddCoin();
        }
        else
        {
            string[] nada = { "Ni la pedrea.", "Nada. Como siempre.", "El año que viene seguro.", "Paco: «Uy, por un número»." };
            LastResult = nada[UnityEngine.Random.Range(0, nada.Length)];
        }
    }

    // Devuelve el mensaje que mostrar tras intentar comprar
    public static string TryBuy(Item item, PlayerStats s)
    {
        if (s == null) return "";
        string why = item.blocked(s);
        if (why != null) { Sfx.Play(SfxType.Blip, 0.7f, 0.6f, true); return why; }
        if (!s.SpendCoins(item.price)) { Sfx.Play(SfxType.Blip, 0.7f, 0.5f, true); return "No te llega. Paco no fía."; }
        LastResult = null;
        item.buy(s);
        RunStats.ShopPurchases++;
        Sfx.Play(SfxType.Pickup, 0.9f, 1f, true);
        return LastResult ?? ("¡Marchando " + item.name.ToLowerInvariant() + "!");
    }
}
