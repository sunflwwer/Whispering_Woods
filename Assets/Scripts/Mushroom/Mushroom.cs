using Sample;
using UnityEngine;

public class Mushroom : MonoBehaviour
{
    [SerializeField] private int healAmount = 1;
    private GameObject interactionUI;
    private bool isPlayerNearby = false;
    private GhostScript player;
    private TerrainObjectManager terrainObjectManager;

    private void Start()
    {
        interactionUI = transform.Find("Canvas")?.gameObject;
        interactionUI?.SetActive(false);

        TerrainObjectManager[] managers = FindObjectsOfType<TerrainObjectManager>();
        foreach (var manager in managers)
        {
            if (manager.targetTerrain.terrainData.bounds.Contains(transform.position - manager.targetTerrain.transform.position))
            {
                terrainObjectManager = manager;
                break;
            }
        }

        if (terrainObjectManager == null)
        {
            Debug.LogError("TerrainObjectManager not found for mushroom position.");
        }
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            ConsumeMushroom();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out GhostScript ghost))
        {
            isPlayerNearby = true;
            player = ghost;
            interactionUI?.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out GhostScript ghost))
        {
            isPlayerNearby = false;
            player = null;
            interactionUI?.SetActive(false);
        }
    }

    private void ConsumeMushroom()
    {
        player?.Heal(healAmount);
        interactionUI?.SetActive(false);
        terrainObjectManager?.OnMushroomConsumed(transform.position);
        Destroy(gameObject);
    }
}
