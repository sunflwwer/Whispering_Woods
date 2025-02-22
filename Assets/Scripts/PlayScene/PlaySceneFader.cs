using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlaySceneFader : MonoBehaviour
{
    [SerializeField] private Image fadeImage; // ���̵� ��/�ƿ��� �̹���
    [SerializeField] private float fadeDuration = 1f; // ���̵� �ð�

    void Start()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true); // ���̵� �̹��� Ȱ��ȭ
            StartCoroutine(FadeIn()); // �� ���� �� ���̵� �� ����
        }
    }

    // ���̵� �� ȿ��
    IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        color.a = 1f; // ���� �� ������ (���� ȭ��)

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - (elapsedTime / fadeDuration); // ���� ��������
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color; // ���̵� �� �Ϸ� �� ���� ����
        fadeImage.gameObject.SetActive(false); // �Ϸ� �� ��Ȱ��ȭ
    }
}
