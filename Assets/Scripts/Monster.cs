using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.ComponentModel;
using UniRx.Triggers;
using System;
using UniRx;

public class Monster : MonoBehaviour
{
    [SerializeField] Game game;
    [SerializeField] public RectTransform rectTransform;
    [SerializeField] Image gaugeBarBack;
    [SerializeField] Image gaugeBar;
    [SerializeField] GameObject moveObj;
    [SerializeField] GameObject cryObj;
    [SerializeField] int maxHp;
    [SerializeField] float destX = 110f;
    [SerializeField, ReadOnly] int curHp;

    bool isDie = false;

    private void Start()
    {
        if (cryObj)
            cryObj.SetActive(false);

        SetHp(maxHp);

        Observable.Timer(System.TimeSpan.FromSeconds(1), System.TimeSpan.FromSeconds(1))
            .Subscribe(_ => ExecuteFunction())
            .AddTo(gameObject);
    }

    public void ExecuteFunction()
    {
        if (rectTransform)
        {
            if (rectTransform.anchoredPosition.x < destX)
            {
                if (isDie == false)
                {
                    game.gameOver.Value = true;
                }
                else
                {
                    CHMMain.Resource.Destroy(gameObject);
                }
            }
        }
    }

    public void SetHp(int _maxHp)
    {
        maxHp = _maxHp;
        curHp = _maxHp;
    }

    public int GetMaxHp()
    {
        return maxHp;
    }

    public int GetHp()
    {
        return curHp;
    }

    public void Move(float _time)
    {
        isDie = false;

        if (rectTransform)
            rectTransform.DOAnchorPos(new Vector2(0, rectTransform.anchoredPosition.y), _time).SetEase(Ease.Linear);
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

            if (curHp <= 0 && isDie == false)
            {
                isDie = true;
                transform.DOScale(Vector3.zero, 1f);
                game.killCount.Value += 1;
            }
        });

        gaugeBarBack.DOFillAmount((float)curHp / maxHp, 1.5f);
    }
}
