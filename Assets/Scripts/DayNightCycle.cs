using UnityEngine;
using TMPro; // TextMeshPro�� ����ϱ� ���� �߰�

public class DayNightCycle : MonoBehaviour
{
    public Light directionalLight; // �¾� ����Ʈ (Directional Light)
    public Light moonLight; // �� ����Ʈ (MoonLight)
    public Material skyboxMaterial; // Skybox Material (���̴� ����� ��Ƽ����)
    public float dayDuration = 20f; // �Ϸ� �ð� (�� ����)
    public TMP_Text dayText; // Canvas�� Day �ؽ�Ʈ (TextMeshPro)
    public Color dayFogColor = Color.cyan; // ���� Fog ����
    public Color nightFogColor = new Color(0.05f, 0.05f, 0.2f); // ���� Fog ���� (£�� ����)

    private float rotationSpeed;
    private float fogLerpValue = 0f; // Fog ��ȯ ����
    private int currentDay = 1; // ���� Day (Day 1���� ����)
    private float elapsedTime = 0f; // �Ϸ� ��� �ð� ����
    private bool isEvening = false; // ���� ����

    public bool IsEvening => isEvening; // �ܺο��� ���� ���¸� Ȯ���ϴ� ������Ƽ

    void Start()
    {
        if (directionalLight == null || moonLight == null)
        {
            Debug.LogError("Directional Light or MoonLight is not assigned!");
            return;
        }

        if (skyboxMaterial == null)
        {
            Debug.LogError("Skybox Material is not assigned!");
            return;
        }

        if (dayText == null)
        {
            Debug.LogError("Day Text (TMP_Text) is not assigned!");
            return;
        }

        rotationSpeed = 360f / dayDuration;

        directionalLight.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        moonLight.transform.rotation = Quaternion.Euler(-180f, 0f, 0f);

        RenderSettings.fogColor = dayFogColor;
        UpdateDayText();
    }

    void Update()
    {
        float deltaRotation = rotationSpeed * Time.deltaTime;

        // �¾�� �� ȸ��
        directionalLight.transform.Rotate(Vector3.right, deltaRotation);
        moonLight.transform.rotation = Quaternion.Euler(
            directionalLight.transform.eulerAngles.x - 180f,
            directionalLight.transform.eulerAngles.y,
            directionalLight.transform.eulerAngles.z
        );

        UpdateLightIntensities();
        skyboxMaterial.SetVector("_LightDirection", -directionalLight.transform.forward);
        UpdateFogColor();
        UpdateEveningStatus();

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= dayDuration)
        {
            elapsedTime = 0f;
            currentDay++;
            UpdateDayText();
        }
    }

    private void UpdateLightIntensities()
    {
        float sunHeight = Vector3.Dot(directionalLight.transform.forward, Vector3.down);
        float moonHeight = Vector3.Dot(moonLight.transform.forward, Vector3.down);

        directionalLight.intensity = Mathf.Clamp01(sunHeight);
        moonLight.intensity = Mathf.Clamp01(moonHeight * 0.3f);
        fogLerpValue = Mathf.Clamp01(sunHeight);
    }

    private void UpdateFogColor()
    {
        RenderSettings.fogColor = Color.Lerp(nightFogColor, dayFogColor, fogLerpValue);
    }

    private void UpdateDayText()
    {
        dayText.text = $"Day {currentDay}";
    }

    private void UpdateEveningStatus()
    {
        // �¾��� X �� ȸ�� ������ �������� ���� ���¸� �Ǵ�
        float sunRotationX = directionalLight.transform.eulerAngles.x;

        // ������ �¾��� 180�� ~ 360���� ��
        isEvening = sunRotationX >= 180f && sunRotationX < 360f;
    }
}
