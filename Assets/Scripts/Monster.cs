using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Monster : MonoBehaviour
{
    [SerializeField] Image gaugeBar;
    [SerializeField] GameObject moveObj;
    [SerializeField] GameObject cryObj;
    [SerializeField] int maxHp;
    [SerializeField, ReadOnly] int curHp;

    private void Start()
    {
        cryObj.SetActive(false);

        var rt = GetComponent<RectTransform>();
        rt.DOAnchorPos(new Vector2(rt.anchoredPosition.x - 100, rt.anchoredPosition.y), 15f);
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
        cryObj.SetActive(true);

        gaugeBar.DOFillAmount((float)curHp / maxHp, .5f).OnComplete(() =>
        {
            cryObj.SetActive(false);
        });
    }
}
