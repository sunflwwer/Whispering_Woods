using System.Collections.Generic;
using UnityEngine;

public class TerrainObjectManager : MonoBehaviour
{
    public Terrain targetTerrain;
    public GameObject treePrefab;

    public GameObject specialTreeApplePrefab;
    public GameObject specialTreePlumPrefab;
    public GameObject specialTreePearPrefab;

    public GameObject mushroomPrefab;
    public GameObject flowerPrefab;
    public GameObject spiderPrefab; // �Ź� ������ �߰�

    public int treeCount = 100;
    public float minTreeHeight = 4f;
    public float maxTreeHeight = 100f;

    public int maxMushrooms = 15;
    public float mushroomRespawnDelay = 10f;

    public float minFlowerHeight = 5f;
    public float maxFlowerHeight = 50f;

    public int maxSpiders = 5; // �ִ� �Ź� �� ����
    public float spiderRespawnDelay = 30f; // �Ź� ����� �ð�

    public int grassLayerIndex = 0;

    private readonly List<Vector3> treePositions = new List<Vector3>();       // �Ϲ� ����
    private readonly List<Vector3> specialTreePositions = new List<Vector3>(); // Ư�� ���� 3����
    private readonly List<Vector3> mushroomPositions = new List<Vector3>();    // ����
    private readonly List<Vector3> flowerPositions = new List<Vector3>();      // ��
    private readonly List<Vector3> spiderPositions = new List<Vector3>();      // �Ź�


    private readonly List<Vector3> placedPositions = new List<Vector3>();
    private readonly List<GameObject> activeSpiders = new List<GameObject>(); // Ȱ��ȭ�� �Ź� ���
    private Bounds terrainBounds;
    private GameObject spawnedFlower;
    private DayNightCycle dayNightCycle;

    private readonly List<Vector3> allPlacedObjects = new List<Vector3>(); // ��� ��ġ�� ������Ʈ�� ��ġ ����Ʈ


    private void Start()
    {
        terrainBounds = targetTerrain.terrainData.bounds;
        terrainBounds.center += targetTerrain.transform.position;

        dayNightCycle = FindObjectOfType<DayNightCycle>();
/*        if (dayNightCycle == null)
        {
            Debug.LogError("DayNightCycle is not found in the scene.");
            return;
        }*/

        PlaceTrees();

        if (HasValidSpecialTreeTag())
        {
            PlaceSpecialTree();
        }


        if (targetTerrain.CompareTag("SpecialFlower"))
        {
            PlaceFlower();
        }

        if (HasValidSpiderTag()) // Terrain �±װ� ��ȿ�ϸ� �Ź� ����
        {
            InitializeSpiders();
        }

        InitializeMushrooms();
    }


    // Terrain �±װ� SpecialTreeApple, SpecialTreePlum, SpecialTreePear, SpecialFlower, Spider �� �ϳ����� Ȯ���ϴ� �Լ�
    private bool HasValidSpiderTag()
    {
        return targetTerrain.CompareTag("SpecialTreeApple") ||
               targetTerrain.CompareTag("SpecialTreePlum") ||
               targetTerrain.CompareTag("SpecialTreePear") ||
               targetTerrain.CompareTag("SpecialFlower");
    }

    private void Update()
    {
        if (spawnedFlower != null && dayNightCycle != null)
        {
            bool targetActiveState = dayNightCycle.IsEvening;

            // ���� Ȱ��ȭ�� �� Ŀ���� �ִϸ��̼�
            if (targetActiveState && !spawnedFlower.activeSelf)
            {
                spawnedFlower.SetActive(true);
                StartCoroutine(ScaleFlower(spawnedFlower, Vector3.zero, Vector3.one, 0.5f)); // 0.5�� ���� ������ ��ȭ
            }
            // ���� ��Ȱ��ȭ�� �� �۾����� �ִϸ��̼�
            else if (!targetActiveState && spawnedFlower.activeSelf)
            {
                StartCoroutine(ScaleFlower(spawnedFlower, Vector3.one, Vector3.zero, 0.5f, () =>
                {
                    spawnedFlower.SetActive(false);
                }));
            }
        }
    }

    private void PlaceTrees()
    {
        for (int i = 0; i < treeCount; i++)
        {
            Vector3 position = GetRandomValidPosition(minTreeHeight, maxTreeHeight);

            // Ư�� ������� ��ġ�� �ʵ��� �߰� �˻�
            if (position != Vector3.zero && !IsTooCloseToOtherTrees(position, specialTreePositions, 7.0f))
            {
                SpawnObject(treePrefab, position);
                treePositions.Add(position);
                allPlacedObjects.Add(position);
            }
        }
    }

    // Ư�� �������� �ּ� �Ÿ� ����
    private bool IsTooCloseToOtherTrees(Vector3 position, List<Vector3> otherTrees, float minDistance)
    {
        foreach (Vector3 otherTree in otherTrees)
        {
            if ((position - otherTree).sqrMagnitude < minDistance * minDistance)
                return true;
        }
        return false;
    }




    private bool HasValidSpecialTreeTag()
    {
        return targetTerrain.CompareTag("SpecialTreeApple") ||
               targetTerrain.CompareTag("SpecialTreePlum") ||
               targetTerrain.CompareTag("SpecialTreePear");
    }


    private void PlaceSpecialTree()
    {
        Vector3 position = GetRandomValidPosition(minTreeHeight, maxTreeHeight);
        if (position == Vector3.zero) return;

        GameObject treeToSpawn = null;

        if (targetTerrain.CompareTag("SpecialTreeApple"))
        {
            treeToSpawn = specialTreeApplePrefab;
        }
        else if (targetTerrain.CompareTag("SpecialTreePlum"))
        {
            treeToSpawn = specialTreePlumPrefab;
        }
        else if (targetTerrain.CompareTag("SpecialTreePear"))
        {
            treeToSpawn = specialTreePearPrefab;
        }

        if (treeToSpawn != null)
        {
            SpawnObject(treeToSpawn, position);
            specialTreePositions.Add(position); // Ư�� ���� ����Ʈ �߰�
            allPlacedObjects.Add(position); // ��ü ����Ʈ �߰�
        }
    }



    private void PlaceFlower()
    {
        Vector3 position = GetRandomValidPosition(minFlowerHeight, maxFlowerHeight);
        if (position != Vector3.zero)
        {
            spawnedFlower = SpawnObject(flowerPrefab, position);
            spawnedFlower.SetActive(false);
            flowerPositions.Add(position); // �� ����Ʈ �߰�
            allPlacedObjects.Add(position); // ��ü ����Ʈ �߰�
        }
    }



    private void InitializeMushrooms()
    {
        for (int i = 0; i < maxMushrooms; i++)
        {
            Vector3 position = GetRandomValidPosition();
            if (position != Vector3.zero)
            {
                SpawnMushroom(position);
            }
        }
    }

    public void OnMushroomConsumed(Vector3 consumedPosition)
    {
        StartCoroutine(RespawnMushroomAfterDelay());
    }

    private System.Collections.IEnumerator RespawnMushroomAfterDelay()
    {
        yield return new WaitForSeconds(mushroomRespawnDelay);

        Vector3 newPosition = GetRandomValidPosition();
        if (newPosition != Vector3.zero)
        {
            SpawnMushroom(newPosition);
        }
        else
        {
            Debug.LogWarning($"Failed to respawn mushroom on terrain: {targetTerrain.name}. No valid position found.");
        }
    }

    private void InitializeSpiders()
    {
        for (int i = 0; i < maxSpiders; i++)
        {
            Vector3 position = GetRandomValidPosition();
            if (position != Vector3.zero)
            {
                SpawnSpider(position);
            }
        }
    }

    private void SpawnSpider(Vector3 position)
    {
        GameObject spider = Instantiate(spiderPrefab, position, Quaternion.identity);
        activeSpiders.Add(spider);
        spiderPositions.Add(position); // �Ź� ����Ʈ �߰�
        allPlacedObjects.Add(position); // ��ü ����Ʈ �߰�
    }


    public void OnSpiderRemoved(GameObject spider)
    {
        activeSpiders.Remove(spider);
        Destroy(spider);

        StartCoroutine(RespawnSpiderAfterDelay());
    }

    private System.Collections.IEnumerator RespawnSpiderAfterDelay()
    {
        yield return new WaitForSeconds(spiderRespawnDelay);

        if (activeSpiders.Count < maxSpiders)
        {
            Vector3 newPosition = GetRandomValidPosition();
            if (newPosition != Vector3.zero)
            {
                SpawnSpider(newPosition);
            }
            else
            {
                Debug.LogWarning($"Failed to respawn spider on terrain: {targetTerrain.name}. No valid position found.");
            }
        }
    }

    private GameObject SpawnObject(GameObject prefab, Vector3 position)
    {
        return Instantiate(prefab, position, Quaternion.identity);
    }

    private void SpawnMushroom(Vector3 position)
    {
        Instantiate(mushroomPrefab, position, Quaternion.identity);
        mushroomPositions.Add(position); // ���� ����Ʈ �߰�
        allPlacedObjects.Add(position); // ��ü ����Ʈ �߰�
    }


    private Vector3 GetRandomValidPosition(float minHeight = 0f, float maxHeight = float.MaxValue)
    {
        for (int i = 0; i < 200; i++)
        {
            float randomX = Random.Range(terrainBounds.min.x, terrainBounds.max.x);
            float randomZ = Random.Range(terrainBounds.min.z, terrainBounds.max.z);
            float terrainHeight = targetTerrain.SampleHeight(new Vector3(randomX, 0, randomZ));

            if (terrainHeight < minHeight || terrainHeight > maxHeight)
                continue;

            Vector3 position = new Vector3(randomX, terrainHeight, randomZ);

            if (IsPositionValid(position, 6.0f)) // ��ü ������Ʈ �ּ� �Ÿ� 6.0f ����
                return position;

        }

        return Vector3.zero;
    }


    private bool IsPositionValid(Vector3 position, float minDistance)
    {
        float treeMinDist = 5f;
        float specialTreeMinDist = 6f;
        float mushroomMinDist = 3f;
        float flowerMinDist = 4f;
        float spiderMinDist = 7f;

        // �Ϲ� �������� �Ÿ� ����
        foreach (Vector3 placed in treePositions)
        {
            if ((position - placed).sqrMagnitude < treeMinDist * treeMinDist)
                return false;
        }

        // Ư�� �������� �Ÿ� ����
        foreach (Vector3 placed in specialTreePositions)
        {
            if ((position - placed).sqrMagnitude < specialTreeMinDist * specialTreeMinDist)
                return false;
        }

        // �������� �Ÿ� ����
        foreach (Vector3 placed in mushroomPositions)
        {
            if ((position - placed).sqrMagnitude < mushroomMinDist * mushroomMinDist)
                return false;
        }

        // �ɳ��� �Ÿ� ����
        foreach (Vector3 placed in flowerPositions)
        {
            if ((position - placed).sqrMagnitude < flowerMinDist * flowerMinDist)
                return false;
        }

        // �Ź̳��� �Ÿ� ����
        foreach (Vector3 placed in spiderPositions)
        {
            if ((position - placed).sqrMagnitude < spiderMinDist * spiderMinDist)
                return false;
        }

        float minDistanceSquared = minDistance * minDistance;

        // Ư�� �±�(Fence, LightFlower, Gravestone, FinalTree, Object)�� ���� ������Ʈ ��ó���� ��ġ���� ����
        Collider[] colliders = Physics.OverlapSphere(position, minDistance);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Fence") || collider.CompareTag("Gravestone") || collider.CompareTag("LightFlower") || collider.CompareTag("FinalTree") || collider.CompareTag("Object"))
                return false;
        }

        // ������ �ùٸ��� Ȯ��
        if (!IsOnGrassLayer(position))
        {
            return false;
        }

        return true;
    }


    private bool IsOnGrassLayer(Vector3 position)
    {
        Vector3 terrainPosition = targetTerrain.transform.position;
        TerrainData terrainData = targetTerrain.terrainData;

        float normalizedX = (position.x - terrainPosition.x) / terrainData.size.x;
        float normalizedZ = (position.z - terrainPosition.z) / terrainData.size.z;

        int mapX = Mathf.FloorToInt(normalizedX * terrainData.alphamapWidth);
        int mapZ = Mathf.FloorToInt(normalizedZ * terrainData.alphamapHeight);

        if (mapX < 0 || mapZ < 0 || mapX >= terrainData.alphamapWidth || mapZ >= terrainData.alphamapHeight)
            return false;

        float[,,] splatMap = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);
        return splatMap[0, 0, grassLayerIndex] > 0.5f;
    }

    // �� ũ�� ���� �ڷ�ƾ
    private System.Collections.IEnumerator ScaleFlower(GameObject flower, Vector3 startScale, Vector3 endScale, float duration, System.Action onComplete = null)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            flower.transform.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        flower.transform.localScale = endScale;

        onComplete?.Invoke();
    }
}
