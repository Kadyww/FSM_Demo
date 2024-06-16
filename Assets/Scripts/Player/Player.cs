using Unity.Netcode;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    private void Start()
    {
        PlayerManager.RegisterPlayer(this);
    }
}