using ChvjUnityInfra;
using UnityEngine;
using UnityEngine.UI;

// 게임 실패 화면 '남은 목표 블록'의 아이콘 1개 위젯.
// 화면 최상위가 아닌 위젯이므로 UIBase가 아닌 일반 MonoBehaviour로 둔다.
public class FailBlockIconItem : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private CHText countText;

    // 블록 종류와 개수를 설정한다. 스프라이트는 비동기로 로드된다.
    public void Setup(Defines.EBlockState state, int count)
    {
        if (countText != null)
            countText.SetText($"x{count}");

        if (icon == null)
            return;

        // 스프라이트 로드 완료 전 잘못된 이미지가 1프레임 보이는 것을 방지
        icon.enabled = false;
        CHMResource.Instance.LoadSprite(state, sprite =>
        {
            // 콜백 도착 전 UI가 파괴됐을 수 있으므로 방어
            if (icon == null || sprite == null)
                return;

            icon.sprite = sprite;
            icon.enabled = true;
        });
    }
}
