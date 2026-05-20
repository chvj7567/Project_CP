using DG.Tweening;
using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using ChvjUnityInfra;
using UnityEngine.UI;

public class CHGaugeBar : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] CHText textDamage;
    [SerializeField] Image imgBackGaugeBar;
    [SerializeField] Image imgGaugeBar;

    float originPosYText;

    // 게이지 바 트윈 시간 — 뒤쪽 바는 느리게(잔상 효과), 앞쪽 바는 빠르게
    const float BackGaugeFillDuration = 1.5f;
    const float FrontGaugeFillDuration = 1f;
    // 게이지 바 리셋 시 채움 트윈 시간(초)
    const float GaugeResetDuration = 0.1f;
    // 데미지 텍스트 표시 시간(초)
    const float DamageTextShowDuration = 2f;
    // 데미지 텍스트가 위로 떠오르는 거리
    const float DamageTextRiseOffset = 10f;

    private void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
    }

    public void Init(float _posY)
    {
        textDamage.gameObject.SetActive(false);
        textDamage.SetStringID(1);
        canvas.worldCamera = Camera.main;
        transform.localPosition = new Vector3(0f, _posY, 0f);
        originPosYText = _posY;
    }

    public void SetGaugeBar(float _maxValue, float _curValue, float _damage)
    {
        if (imgBackGaugeBar) imgBackGaugeBar.DOFillAmount(_curValue / _maxValue, BackGaugeFillDuration);
        if (imgGaugeBar) imgGaugeBar.DOFillAmount(_curValue / _maxValue, FrontGaugeFillDuration);

        ShowDamageText(_damage, DamageTextShowDuration);
    }

    public void ResetGaugeBar()
    {
        if (imgBackGaugeBar) imgBackGaugeBar.DOFillAmount(1f, GaugeResetDuration);
        if (imgGaugeBar) imgGaugeBar.DOFillAmount(1f, GaugeResetDuration);
    }

    void ShowDamageText(float _damage, float _time)
    {
        if (textDamage)
        {
            var copyTextDamage = CHMResource.Instance.Instantiate(textDamage.gameObject, transform).GetComponent<CHText>();
            var copyTmp = copyTextDamage.GetComponent<TMP_Text>();
            copyTextDamage.gameObject.SetActive(true);
            copyTextDamage.transform.localPosition = Vector3.zero;
            copyTextDamage.SetText(_damage);

            if (_damage < 0)
            {
                copyTextDamage.SetColor(Color.red);
            }
            else if (_damage > 0)
            {
                copyTextDamage.SetColor(Color.green);
            }
            else
            {
                copyTextDamage.SetColor(Color.gray);
            }

            copyTmp.DOFade(0, _time);

            var rtTextDamage = copyTextDamage.GetComponent<RectTransform>();
            if (rtTextDamage)
            {
                rtTextDamage.DOAnchorPosY(originPosYText + DamageTextRiseOffset, _time).OnComplete(() =>
                {
                    copyTmp.alpha = 1f;
                    rtTextDamage.anchoredPosition = new Vector2(rtTextDamage.anchoredPosition.x, originPosYText);
                    CHMResource.Instance.Destroy(copyTextDamage.gameObject);
                });
            }
        }
    }
}
