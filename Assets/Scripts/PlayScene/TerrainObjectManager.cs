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
    public GameObject spiderPrefab; // 거미 프리팹 추가

    public int treeCount = 100;
    public float minTreeHeight = 4f;
    public float maxTreeHeight = 100f;

    public int maxMushrooms = 15;
    public float mushroomRespawnDelay = 10f;

    public float minFlowerHeight = 5f;
    public float maxFlowerHeight = 50f;

    public int maxSpiders = 5; // 최대 거미 수 제한
    public float spiderRespawnDelay = 30f; // 거미 재생성 시간

    public int grassLayerIndex = 0;

    private readonly List<Vector3> treePositions = new List<Vector3>();       // 일반 나무
    private readonly List<Vector3> specialTreePositions = new List<Vector3>(); // 특별 나무 3종류
    private readonly List<Vector3> mushroomPositions = new List<Vector3>();    // 버섯
    private readonly List<Vector3> flowerPositions = new List<Vector3>();      // 꽃
    private readonly List<Vector3> spiderPositions = new List<Vector3>();      // 거미


    private readonly List<Vector3> placedPositions = new List<Vector3>();
    private readonly List<GameObject> activeSpiders = new List<GameObject>(); // 활성화된 거미 목록
    private Bounds terrainBounds;
    private GameObject spawnedFlower;
    private DayNightCycle dayNightCycle;

    private readonly List<Vector3> allPlacedObjects = new List<Vector3>(); // 모든 배치된 오브젝트의 위치 리스트


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

        if (HasValidSpiderTag()) // Terrain 태그가 유효하면 거미 생성
        {
            InitializeSpiders();
        }

        InitializeMushrooms();
    }


    // Terrain 태그가 SpecialTreeApple, SpecialTreePlum, SpecialTreePear, SpecialFlower, Spider 중 하나인지 확인하는 함수
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

            // 꽃이 활성화될 때 커지는 애니메이션
            if (targetActiveState && !spawnedFlower.activeSelf)
            {
                spawnedFlower.SetActive(true);
                StartCoroutine(ScaleFlower(spawnedFlower, Vector3.zero, Vector3.one, 0.5f)); // 0.5초 동안 스케일 변화
            }
            // 꽃이 비활성화될 때 작아지는 애니메이션
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

            // 특별 나무들과 겹치지 않도록 추가 검사
            if (position != Vector3.zero && !IsTooCloseToOtherTrees(position, specialTreePositions, 7.0f))
            {
                SpawnObject(treePrefab, position);
                treePositions.Add(position);
                allPlacedObjects.Add(position);
            }
        }
    }

    // 특별 나무와의 최소 거리 유지
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
            specialTreePositions.Add(position); // 특별 나무 리스트 추가
            allPlacedObjects.Add(position); // 전체 리스트 추가
        }
    }



    private void PlaceFlower()
    {
        Vector3 position = GetRandomValidPosition(minFlowerHeight, maxFlowerHeight);
        if (position != Vector3.zero)
        {
            spawnedFlower = SpawnObject(flowerPrefab, position);
            spawnedFlower.SetActive(false);
            flowerPositions.Add(position); // 꽃 리스트 추가
            allPlacedObjects.Add(position); // 전체 리스트 추가
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
        spiderPositions.Add(position); // 거미 리스트 추가
        allPlacedObjects.Add(position); // 전체 리스트 추가
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
        mushroomPositions.Add(position); // 버섯 리스트 추가
        allPlacedObjects.Add(position); // 전체 리스트 추가
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

            if (IsPositionValid(position, 6.0f)) // 전체 오브젝트 최소 거리 6.0f 유지
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

        // 일반 나무끼리 거리 유지
        foreach (Vector3 placed in treePositions)
        {
            if ((position - placed).sqrMagnitude < treeMinDist * treeMinDist)
                return false;
        }

        // 특별 나무끼리 거리 유지
        foreach (Vector3 placed in specialTreePositions)
        {
            if ((position - placed).sqrMagnitude < specialTreeMinDist * specialTreeMinDist)
                return false;
        }

        // 버섯끼리 거리 유지
        foreach (Vector3 placed in mushroomPositions)
        {
            if ((position - placed).sqrMagnitude < mushroomMinDist * mushroomMinDist)
                return false;
        }

        // 꽃끼리 거리 유지
        foreach (Vector3 placed in flowerPositions)
        {
            if ((position - placed).sqrMagnitude < flowerMinDist * flowerMinDist)
                return false;
        }

        // 거미끼리 거리 유지
        foreach (Vector3 placed in spiderPositions)
        {
            if ((position - placed).sqrMagnitude < spiderMinDist * spiderMinDist)
                return false;
        }

        float minDistanceSquared = minDistance * minDistance;

        // 특정 태그(Fence, LightFlower, Gravestone, FinalTree, Object)를 가진 오브젝트 근처에는 배치하지 않음
        Collider[] colliders = Physics.OverlapSphere(position, minDistance);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Fence") || collider.CompareTag("Gravestone") || collider.CompareTag("LightFlower") || collider.CompareTag("FinalTree") || collider.CompareTag("Object"))
                return false;
        }

        // 지형이 올바른지 확인
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

    // 꽃 크기 조절 코루틴
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
