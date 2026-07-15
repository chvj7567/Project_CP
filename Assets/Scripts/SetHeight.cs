using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SetHeight : MonoBehaviour
{
    private void Awake()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        float height = rectTransform.rect.height;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, height);
    }
}
