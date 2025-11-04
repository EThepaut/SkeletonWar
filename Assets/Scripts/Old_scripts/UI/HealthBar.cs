using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;
    public Image backgroundImage;

    void Awake()
    {
        if (fillImage != null)
        {
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 1f;
            fillImage.rectTransform.localScale = Vector3.one;
            if (fillImage.transform.parent != null)
                ((RectTransform)fillImage.transform.parent).localScale = Vector3.one;
        }
    }
    public void SetHealth(float current, float max)
    {
        float ratio = Mathf.Clamp01(current / max);

        float bgWidth = ((RectTransform)backgroundImage.transform).rect.width;

        RectTransform fillRect = (RectTransform)fillImage.transform;
        fillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bgWidth * ratio);

        gameObject.SetActive(current < max);
    }

    public void SetWorldPosition(Vector3 worldPos)
    {
        transform.position = worldPos;
    }
}