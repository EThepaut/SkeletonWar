using UnityEngine;
using TMPro;

public class FloatingDamageText : MonoBehaviour
{
    public float floatSpeed = 50f;
    public float lifetime = 1f;
    public TMP_Text textMesh;

    private float timer = 0f;

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        timer += Time.deltaTime;
        if (timer >= lifetime)
            Destroy(gameObject);
    }

    public void SetText(string text)
    {
        if (textMesh != null)
            textMesh.text = text;
    }

    public static class FloatingDamageTextSpawner
    {
        // le code marche pas il est ultra guez
        public static void Spawn(GameObject prefab, Vector3 worldPosition, float amount)
        {
            var canvasGO = GameObject.Find("DamageTextCanvas");
            var canvas = canvasGO?.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("no damagetextcanvas l'écriture compte btw");
                return;
            }

            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

            Vector2 localPoint;
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, null, out localPoint))
            {
                var go = Object.Instantiate(prefab, canvas.transform);
                var rectTransform = go.GetComponent<RectTransform>();
                if (rectTransform != null)
                    rectTransform.anchoredPosition = localPoint;

                var floatingText = go.GetComponent<FloatingDamageText>();
                if (floatingText != null)
                    floatingText.SetText(amount.ToString("F0"));

                Debug.Log("Spawned damage text at localPoint: " + localPoint);
            }
            else
            {
                Debug.LogWarning("Failed to convert screenPos to localPoint: " + screenPos);
            }
        }
    }
}