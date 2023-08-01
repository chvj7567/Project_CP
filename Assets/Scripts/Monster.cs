using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Monster : MonoBehaviour
{
    [SerializeField] Image gaugeBar;
    [SerializeField] int maxHp;
    [SerializeField, ReadOnly] int curHp;

    private void Start()
    {
        var rt = GetComponent<RectTransform>();
        rt.DOAnchorPos(new Vector2(rt.anchoredPosition.x - 500, rt.anchoredPosition.y), 15f);
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
        gaugeBar.DOFillAmount((float)curHp / maxHp, 1f);
    }
}
