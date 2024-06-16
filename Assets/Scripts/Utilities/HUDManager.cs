using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private CameraController cameraController;
    [SerializeField] private GameObject mapCamera;
    [SerializeField] private Player playerPrefab;
    [SerializeField] private MapGenerator mapGenerator;

    private static HUDManager instance;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);
    }
    
    public void OnGenerateButtonPressed()
    {
        cameraController.gameObject.SetActive(false);
        mapCamera.SetActive(true);

        var players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            PlayerManager.UnregisterPlayer(player);
            Destroy(player.gameObject);
        }

        mapGenerator.Generate();
    }
    
    public void OnPlayButtonPressed()
    {
        cameraController.gameObject.SetActive(true);
        mapCamera.SetActive(false);
        
        var player = Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        cameraController.SetTarget(player.transform);
        
        var enemies = FindObjectsByType<Creature>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.gameObject.SetActive(true);
        }
    }
}
