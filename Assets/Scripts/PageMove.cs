using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PageMove : MonoBehaviour
{
    [SerializeField] RectTransform standard;
    [SerializeField] RectTransform stageSelect1RectTransform;
    [SerializeField] StageSelect stageSelect1;
    [SerializeField] RectTransform stageSelect2RectTransform;
    [SerializeField] StageSelect stageSelect2;

    [SerializeField] Button leftBtn;
    [SerializeField] Button rightBtn;
    [SerializeField] float moveSpeed;
    [SerializeField, ReadOnly] int page;
    [SerializeField, ReadOnly] float width;

    bool moving = false;
    // stageSelectÀÇ index;
    int index;

    private void Start()
    {
        width = standard.rect.width;
        index = 1;

        stageSelect1RectTransform.anchoredPosition = Vector2.zero;
        stageSelect2RectTransform.anchoredPosition = new Vector2(width, 0);

        leftBtn.OnClickAsObservable().Subscribe(_ =>
        {
            MoveLeft();
        });

        rightBtn.OnClickAsObservable().Subscribe(_ =>
        {
            MoveRight();
        });
    }

    public void Init(int _stage)
    {
        if (_stage <= 1)
        {
            page = 1;
        }    
        else
        {
            page = (_stage - 1) / 9 + 1;
        }

        stageSelect1.Init();
        stageSelect2.Init();

        stageSelect1.SetPage(page);

        CheckCurrentPage();
    }

    public void ActiveMoveBtn(bool _active)
    {
        leftBtn.gameObject.SetActive(_active);
        rightBtn.gameObject.SetActive(_active);
    }

    void CheckCurrentPage()
    {
        if (page == 0)
        {
            ActiveMoveBtn(false);
            return;
        }

        ActiveMoveBtn(true);

        if (page == 1)
        {
            leftBtn.gameObject.SetActive(false);
        }

        if (page == CHMMain.Json.GetMaxStageGroup())
        {
            rightBtn.gameObject.SetActive(false);
        }
    }

    void MoveLeft()
    {
        if (index == 1)
        {
            if (moving == false && stageSelect2.SetPage(page - 1) == true)
            {
                index = 2;
                stageSelect2RectTransform.anchoredPosition = new Vector2(-width, 0);

                moving = true;
                stageSelect1RectTransform.DOAnchorPos(new Vector2(width, 0), moveSpeed);
                stageSelect2RectTransform.DOAnchorPos(new Vector2(0, 0), moveSpeed).OnComplete(() =>
                {
                    moving = false;
                });

                --page;
            }
        }
        else
        {
            if (moving == false && stageSelect1.SetPage(page - 1) == true)
            {
                index = 1;
                stageSelect1RectTransform.anchoredPosition = new Vector2(-width, 0);

                moving = true;
                stageSelect2RectTransform.DOAnchorPos(new Vector2(width, 0), moveSpeed);
                stageSelect1RectTransform.DOAnchorPos(new Vector2(0, 0), moveSpeed).OnComplete(() =>
                {
                    moving = false;
                });

                --page;
            }
        }

        CheckCurrentPage();
    }

    void MoveRight()
    {
        if (index == 1)
        {
            if (moving == false && stageSelect2.SetPage(page + 1) == true)
            {
                index = 2;
                stageSelect2RectTransform.anchoredPosition = new Vector2(width, 0);

                moving = true;
                stageSelect1RectTransform.DOAnchorPos(new Vector2(-width, 0), moveSpeed);
                stageSelect2RectTransform.DOAnchorPos(new Vector2(0, 0), moveSpeed).OnComplete(() =>
                {
                    moving = false;
                });

                ++page;
            }
        }
        else
        {
            if (moving == false && stageSelect1.SetPage(page + 1) == true)
            {
                index = 1;
                stageSelect1RectTransform.anchoredPosition = new Vector2(width, 0);

                moving = true;
                stageSelect2RectTransform.DOAnchorPos(new Vector2(-width, 0), moveSpeed);
                stageSelect1RectTransform.DOAnchorPos(new Vector2(0, 0), moveSpeed).OnComplete(() =>
                {
                    moving = false;
                });

                ++page;
            }
        }

        CheckCurrentPage();
    }
}
