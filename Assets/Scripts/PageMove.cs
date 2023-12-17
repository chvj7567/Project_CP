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

    [Header("한 페이지당 스테이지 수")]
    [SerializeField] int stageCount;
    [SerializeField, ReadOnly] int page;
    [SerializeField, ReadOnly] float width;

    bool moving = false;
    // stageSelect의 index;
    int index;

    Defines.ESelectStage curSelect;

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

    public void Init(Defines.ESelectStage select)
    {
        var lastPlayStage = stageSelect1.GetLastPlayStage();

        curSelect = select;

        if (lastPlayStage <= 1)
        {
            page = 1;
        }    
        else
        {
            page = (lastPlayStage - 1) / stageCount + 1;
        }

        stageSelect1.Init(select);
        stageSelect2.Init(select);

        stageSelect1.SetPage(page, select);
        stageSelect2.SetPage(page, select);

        CheckCurrentPage();

        Debug.Log($"PageMove_Init / Page:{page}, Stage:{lastPlayStage}");
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

        int maxGroup = 0;
        if (curSelect == Defines.ESelectStage.Boss)
        {
            maxGroup = CHMMain.Json.GetMaxStageGroup() - CHMData.Instance.BossStageStartValue;
        }
        else
        {
            maxGroup = CHMMain.Json.GetMaxStageGroup(CHMData.Instance.BossStageStartValue);
        }

        if (page == 1)
        {
            leftBtn.gameObject.SetActive(false);
        }

        if (page == maxGroup)
        {
            rightBtn.gameObject.SetActive(false);
        }
    }

    void MoveLeft()
    {
        if (index == 1)
        {
            if (moving == false && stageSelect2.SetPage(page - 1, curSelect) == true)
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
            if (moving == false && stageSelect1.SetPage(page - 1, curSelect) == true)
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
            if (moving == false && stageSelect2.SetPage(page + 1, curSelect) == true)
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
            if (moving == false && stageSelect1.SetPage(page + 1, curSelect) == true)
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
