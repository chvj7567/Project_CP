using DG.Tweening;
using System.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class Block : MonoBehaviour
{
    [SerializeField]
    Game game;
    [SerializeField]
    Button btn;
    [SerializeField]
    RectTransform backRect;

    [ReadOnly]
    public int index;
    [ReadOnly]
    public int row;
    [ReadOnly]
    public int col;
    [ReadOnly]
    public Defines.ESpriteType type;
    [ReadOnly]
    public Defines.EState state = Defines.EState.None;
    [ReadOnly]
    public int horizontalScore;
    [ReadOnly]
    public int verticalScore;

    public RectTransform rectTransform;
    public Vector2 originPos;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originPos = rectTransform.anchoredPosition;

        btn.OnBeginDragAsObservable().Subscribe(_ =>
        {
            if (game.isDrag == true) return;

            game.isDrag = true;
        });

        btn.OnDragAsObservable().Subscribe(_ =>
        {
            // 있어야 BeginDrag, EndDrag 작동함
        });

        btn.OnEndDragAsObservable().Subscribe(_ =>
        {
            if (game.isAni == true) return;

            game.isDrag = false;

            Vector2 rectPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(backRect, _.position, _.pressEventCamera, out rectPosition);

            var angle = EvalDragAngle(originPos, rectPosition);

            int swipe = (((int)angle + 45) % 360) / 90;

            switch (swipe)
            {
                // Right
                case 0:
                    {
                        var addDis = CHInstantiateButton.GetHorizontalDistance();
                        var movePos = new Vector2(originPos.x + addDis, originPos.y);
                        var ret = CHInstantiateButton.GetBlockInfo(movePos);
                        if (ret.Item1 != null)
                        {
                            rectTransform.DOAnchorPosX(movePos.x, .5f);
                            ret.Item1.DOAnchorPosX(originPos.x, .5f);

                            ChangeBlock(ret.Item2);
                            game.AfterDrag(this, ret.Item2);
                        }
                    }
                    break;
                // Up
                case 1:
                    {
                        var addDis = CHInstantiateButton.GetVerticalDistance();
                        var movePos = new Vector2(originPos.x, originPos.y + addDis);
                        var ret = CHInstantiateButton.GetBlockInfo(movePos);
                        if (ret.Item1 != null)
                        {
                            rectTransform.DOAnchorPosY(movePos.y, .5f);
                            ret.Item1.DOAnchorPosY(originPos.y, .5f);

                            ChangeBlock(ret.Item2);
                            game.AfterDrag(this, ret.Item2);
                        }
                    }
                    break;
                // Left
                case 2:
                    {
                        var addDis = CHInstantiateButton.GetHorizontalDistance();
                        var movePos = new Vector2(originPos.x - addDis, originPos.y);
                        var ret = CHInstantiateButton.GetBlockInfo(movePos);
                        if (ret.Item1 != null)
                        {
                            rectTransform.DOAnchorPosX(movePos.x, .5f);
                            ret.Item1.DOAnchorPosX(originPos.x, .5f);

                            ChangeBlock(ret.Item2);
                            game.AfterDrag(this, ret.Item2);
                        }
                    }
                    break;
                // Down
                case 3:
                    {
                        var addDis = CHInstantiateButton.GetVerticalDistance();
                        var movePos = new Vector2(originPos.x, originPos.y - addDis);
                        var ret = CHInstantiateButton.GetBlockInfo(movePos);
                        if (ret.Item1 != null)
                        {
                            rectTransform.DOAnchorPosY(movePos.y, .5f);
                            ret.Item1.DOAnchorPosY(originPos.y, .5f);

                            ChangeBlock(ret.Item2);
                            game.AfterDrag(this, ret.Item2);
                        }
                    }
                    break;
            }
        });
    }

    float EvalDragAngle(Vector2 vtStart, Vector2 vtEnd)
    {
        Vector2 dragDirection = vtEnd - vtStart;
        if (dragDirection.magnitude <= 0.2f)
            return -1f;

        float aimAngle = Mathf.Atan2(dragDirection.y, dragDirection.x);
        if (aimAngle < 0f)
        {
            aimAngle = Mathf.PI * 2 + aimAngle;
        }

        return aimAngle * Mathf.Rad2Deg;
    }

    void ChangeBlock(Block block)
    {
        game.ChangeBlock(this, block);
    }

    public void SetOriginPos()
    {
        rectTransform.anchoredPosition = originPos;
        transform.DOScale(1f, .5f);
    }

    public void MoveOriginPos()
    {
        rectTransform.DOAnchorPos(originPos, .5f);
    }

    public void SetScore(int score, Defines.EDirection direction)
    {
        switch (direction)
        {
            case Defines.EDirection.None:
                break;
            case Defines.EDirection.Horizontal:
                horizontalScore = score;
                break;
            case Defines.EDirection.Vertical:
                verticalScore = score;
                break;
        }
    }

    public void ResetScore()
    {
        horizontalScore = 0;
        verticalScore = 0;
    }
}