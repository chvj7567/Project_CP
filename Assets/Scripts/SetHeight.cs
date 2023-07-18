using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SetHeight : MonoBehaviour
{
    private void Awake()
    {
        var rectTransform = GetComponent<RectTransform>();
        var height = rectTransform.rect.height;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, height);
    }
}
