using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager instance;

    private readonly HashSet<Player> players = new();

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);
    }

    public static Player GetClosestPlayer(Vector3 position)
    {
        Player closestPlayer = null;
        float closestDistance = float.MaxValue;
        foreach (Player player in instance.players)
        {
            float distance = Vector3.Distance(player.transform.position, position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    public static void RegisterPlayer(Player player)
    {
        instance.players.Add(player);
    }

    public static void UnregisterPlayer(Player player)
    {
        instance.players.Remove(player);
    }
}