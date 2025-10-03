using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class GlobalCounter : MonoBehaviour
{
    public static int TreeCounter = 0;
    public static int MushroomCounter = 0;
    public static int FlowerCounter = 4;
    public static int SpiderCounter = 0;

    // 비석 활성화 여부
    public static bool IsTreeGravestoneActivated = false;
    public static bool IsMushroomGravestoneActivated = false;
    public static bool IsFlowerGravestoneActivated = false;
    public static bool IsSpiderGravestoneActivated = false;

    // 모든 비석이 활성화되었을 때 호출될 이벤트
    public static event Action OnAllGravestonesActivated;

    public static bool AreAllGravestonesActivated()
    {
        bool allActivated = IsTreeGravestoneActivated &&
                            IsMushroomGravestoneActivated &&
                            IsFlowerGravestoneActivated &&
                            IsSpiderGravestoneActivated;

        if (allActivated)
        {
            OnAllGravestonesActivated?.Invoke(); // 모든 비석 활성화 시 이벤트 호출
        }

        return allActivated;
    }

    public static void ResetCounters()
    {
        TreeCounter = 0;
        MushroomCounter = 0;
        FlowerCounter = 4; // 기본값 유지
        SpiderCounter = 0;

        IsTreeGravestoneActivated = false;
        IsMushroomGravestoneActivated = false;
        IsFlowerGravestoneActivated = false;
        IsSpiderGravestoneActivated = false;
    }
    static GlobalCounter()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetCounters();
    }
}
