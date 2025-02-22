using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class ButtonSceneLoader : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Button button;
    private TextMeshProUGUI buttonText;

    // ���� ����
    private Color normalColor = new Color(0f, 0f, 0f); // ������
    private Color highlightedColor = new Color(0.3f, 0.3f, 0.3f); // ȸ��
    private Color pressedColor = new Color(0f, 0f, 0f); // ���� ȸ��

    // ���̵� ȿ�� ����
    [SerializeField] private Image fadeImage; // ���̵� ��/�ƿ��� �̹��� (ĵ���� ��ܿ� ��ġ�ؾ� ��)
    [SerializeField] private float fadeDuration = 1f; // ���̵� �ð�

    void Awake()
    {
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();

        if (button != null && buttonText != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("Button �Ǵ� TextMeshProUGUI ������Ʈ�� ã�� �� �����ϴ�.");
        }

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true); // ���̵� �̹��� Ȱ��ȭ
            Color color = fadeImage.color;
            color.a = 0f; // ���� �� �����ϰ� ����
            fadeImage.color = color;
        }
    }


    void OnEnable()
    {
        if (button != null)
        {
            buttonText.color = normalColor;
        }
    }

    // ���콺�� ��ư ���� �ö���� ��
    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.color = highlightedColor;
    }

    // ���콺�� ��ư���� ���������� ��
    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.color = normalColor;
    }

    // ��ư�� ������ ��
    public void OnPointerDown(PointerEventData eventData)
    {
        buttonText.color = pressedColor;
    }

    // ��ư�� ������ ���� ��
    public void OnPointerUp(PointerEventData eventData)
    {
        buttonText.color = highlightedColor;
    }

    // ��ư Ŭ�� �� ������ �Լ�
    void OnButtonClick()
    {
        Debug.Log("��ư Ŭ�� �� ���̵� �ƿ� ����");
        StartCoroutine(FadeOutAndLoadScene());
    }

    // �� �ε� �� ���̵� �ƿ� �� �� ��ȯ
    IEnumerator FadeOutAndLoadScene()
    {
        yield return StartCoroutine(FadeOut());
        //yield return new WaitForSeconds(2f); // ���̵� �ƿ� �� 1�� ���
        SceneManager.LoadScene("Play Scene");
    }

/*    // ���̵� �� ȿ�� (�� �ε� ��)
    IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        color.a = 1f; // ������ �������� �ʰ� ����
        fadeImage.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - (elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
    }*/

    // ���̵� �ƿ� ȿ�� (�� ��ȯ ��)
    IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        color.a = 0f; // ������ ���·� ����
        fadeImage.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = elapsedTime / fadeDuration;
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }
}
