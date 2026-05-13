using TMPro;
using UnityEngine;

// 글로벌 CHTMPro는 패키지 ChvjUnityInfra.CHText를 상속해 prefab의 script GUID를 보존하면서
// 기존 stringID/rtText/text 필드도 함께 유지한다. P9 후속 정리에서 패키지 CHText로 직접 재바인딩 후 삭제 예정.
public class CHTMPro : ChvjUnityInfra.CHText
{
    [SerializeField] int stringID = 1;
    [SerializeField] public RectTransform rtText;
    [SerializeField] public TMP_Text text;

    private void Awake()
    {
        rtText = GetComponent<RectTransform>();
        text = GetComponent<TMP_Text>();

        if (stringID != -1)
        {
            SetStringID(stringID);
        }
    }
}
