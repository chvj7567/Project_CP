using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Monster : MonoBehaviour
{
    [SerializeField] public RectTransform rectTransform;
    [SerializeField] Image gaugeBarBack;
    [SerializeField] Image gaugeBar;
    [SerializeField] GameObject moveObj;
    [SerializeField] GameObject cryObj;
    [SerializeField] int maxHp;
    [SerializeField, ReadOnly] int curHp;

    private void Start()
    {
        if (cryObj)
            cryObj.SetActive(false);

        if (rectTransform)
            rectTransform.DOAnchorPos(new Vector2(0, rectTransform.anchoredPosition.y), 60f);
        SetHp(maxHp);
    }

    public void SetHp(int _maxHp)
    {
        maxHp = _maxHp;
        curHp = _maxHp;
    }

    public int GetHp()
    {
        return curHp;
    }

    public void TakeDamage(int _damage)
    {
        curHp -= _damage;
        if (cryObj)
            cryObj.SetActive(true);

        gaugeBar.DOFillAmount((float)curHp / maxHp, .5f).OnComplete(() =>
        {
            if (cryObj)
                cryObj.SetActive(false);

            if (curHp <= 0)
            {
                transform.DOScale(Vector3.zero, 1f);
                gaugeBar.DOFillAmount(1f, .1f);
            }
        });

        gaugeBarBack.DOFillAmount((float)curHp / maxHp, 1f).OnComplete(() =>
        {
            if (curHp <= 0)
            {
                gaugeBar.DOFillAmount(1f, .2f);
            }
        });
    }
}
