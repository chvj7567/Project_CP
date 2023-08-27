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
    [SerializeField, ReadOnly]
    Defines.ENormalBlockType normalType;
    [SerializeField, ReadOnly]
    Defines.ESpecailBlockType specailType;
    [ReadOnly]
    public Defines.EState state = Defines.EState.None;
    [ReadOnly]
    public int hScore;
    [ReadOnly]
    public int vScore;

    public Image img;
    public RectTransform rectTransform;
    public Vector2 originPos;

    // 벽HP
    [SerializeField, ReadOnly] int hp = 0;
    // 벽은 한 턴에 한 번만 데미지를 입음
    [SerializeField, ReadOnly] bool checkWallDamage = false;

    private void Start()
    {
        originPos = rectTransform.anchoredPosition;

        btn.OnClickAsObservable().Subscribe(async _ =>
        {
            if (CheckMoveBlock(state) == false)
                return;

            if (game.isDrag == false && game.isAni == false)
            {
                switch (specailType)
                {
                    case Defines.ESpecailBlockType.CatPang1:
                        await game.Boom1(this);
                        break;
                    case Defines.ESpecailBlockType.CatPang2:
                        await game.Boom2(this);
                        break;
                    case Defines.ESpecailBlockType.CatPang3:
                        await game.BoomAll(this);
                        break;
                }
            }
        });

        btn.OnBeginDragAsObservable().Subscribe(_ =>
        {
            if (game.isDrag == true || CheckMoveBlock(state) == false)
                return;

            game.isDrag = true;
        });

        btn.OnDragAsObservable().Subscribe(_ =>
        {
            // 있어야 BeginDrag, EndDrag 작동함
        });

        btn.OnEndDragAsObservable().Subscribe(async _ =>
        {
            game.isDrag = false;

            if (game.isAni == true || CheckMoveBlock(state) == false)
                return;

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
                            if (CheckMoveBlock(ret.Item2.state) == false)
                                break;

                            rectTransform.DOAnchorPosX(movePos.x, game.delay);
                            ret.Item1.DOAnchorPosX(originPos.x, game.delay);

                            ChangeBlock(ret.Item2);
                            await game.AfterDrag(this, ret.Item2);
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
                            if (CheckMoveBlock(ret.Item2.state) == false)
                                break;

                            rectTransform.DOAnchorPosY(movePos.y, game.delay);
                            ret.Item1.DOAnchorPosY(originPos.y, game.delay);

                            ChangeBlock(ret.Item2);
                            await game.AfterDrag(this, ret.Item2);
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
                            if (CheckMoveBlock(ret.Item2.state) == false)
                                break;

                            rectTransform.DOAnchorPosX(movePos.x, game.delay);
                            ret.Item1.DOAnchorPosX(originPos.x, game.delay);

                            ChangeBlock(ret.Item2);
                            await game.AfterDrag(this, ret.Item2);
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
                            if (CheckMoveBlock(ret.Item2.state) == false)
                                break;

                            rectTransform.DOAnchorPosY(movePos.y, game.delay);
                            ret.Item1.DOAnchorPosY(originPos.y, game.delay);

                            ChangeBlock(ret.Item2);
                            await game.AfterDrag(this, ret.Item2);
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
    }

    public void SetScore(int score, Defines.EDirection direction)
    {
        switch (direction)
        {
            case Defines.EDirection.Horizontal:
                hScore = score;
                break;
            case Defines.EDirection.Vertical:
                vScore = score;
                break;
        }
    }

    public void ResetScore()
    {
        hScore = 0;
        vScore = 0;
    }

    public Defines.ENormalBlockType GetNormalType()
    {
        return normalType;
    }

    public Defines.ESpecailBlockType GetSpecailType()
    {
        return specailType;
    }

    public void SetNormalType(Defines.ENormalBlockType normalType)
    {
        this.specailType = Defines.ESpecailBlockType.None;
        this.normalType = normalType;
    }

    public void SetSpecailType(Defines.ESpecailBlockType specailType)
    {
        this.normalType = Defines.ENormalBlockType.None;
        this.specailType = specailType;
    }

    public void SetWallHp(int _hp)
    {
        hp = _hp;
    }

    public void DamageWall()
    {
        if (checkWallDamage == false && hp >= 1)
        {
            checkWallDamage = true;
            hp -= 1;

            if (hp >= 3)
            {
                img.sprite = game.wallBlockSpriteList[(int)Defines.EWallBlockType.Wall3];
            }
            else if (hp == 2)
            {
                img.sprite = game.wallBlockSpriteList[(int)Defines.EWallBlockType.Wall2];
            }
            else if (hp == 1)
            {
                img.sprite = game.wallBlockSpriteList[(int)Defines.EWallBlockType.Wall1];
            }
            else
            {
                state = Defines.EState.Match;
            }
        }
    }

    public void DamagePotal()
    {
        if (checkWallDamage == false && hp >= 1)
        {
            checkWallDamage = true;
            hp -= 1;

            if (hp >= 3)
            {
                img.sprite = game.wallBlockSpriteList[(int)Defines.EWallBlockType.Wall3];
            }
            else if (hp == 2)
            {
                img.sprite = game.wallBlockSpriteList[(int)Defines.EWallBlockType.Wall2];
            }
            else if (hp == 1)
            {
                img.sprite = game.wallBlockSpriteList[(int)Defines.EWallBlockType.Wall1];
            }
            else
            {
                state = Defines.EState.Match;
            }
        }
    }

    public void ResetCheckWallDamage()
    {
        checkWallDamage = false;
    }

    bool CheckMoveBlock(Defines.EState _state)
    {
        switch (_state)
        {
            case Defines.EState.Potal:
            case Defines.EState.Wall:
                return false;
            default:
                return true;
        }
    }
}