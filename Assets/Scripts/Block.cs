using DG.Tweening;
using System.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using static Defines;

[RequireComponent(typeof(RectTransform))]
public class Block : MonoBehaviour
{
    [SerializeField]
    Game game;
    [SerializeField]
    Button btn;
    [SerializeField]
    RectTransform backRect;
    [SerializeField]
    CHTMPro hpText;

    [ReadOnly]
    public int index;
    [ReadOnly]
    public int row;
    [ReadOnly]
    public int col;
    [SerializeField, ReadOnly]
    public Defines.EBlockState changeBlockState;
    [SerializeField, ReadOnly]
    Defines.EBlockState blockState = Defines.EBlockState.None;
    [ReadOnly]
    public int hScore;
    [ReadOnly]
    public int vScore;

    public Image img;
    public RectTransform rectTransform;
    public Vector2 originPos;

    public bool match = false;
    public bool boom = false;
    public bool squareMatch = false;

    public bool changeUpScale = false;
    public bool changeDownScale = false;

    // 벽HP
    [SerializeField, ReadOnly] int hp = 0;
    // 벽은 한 턴에 한 번만 데미지를 입음
    [SerializeField, ReadOnly] bool checkDamage = false;

    private void Start()
    {
        changeBlockState = Defines.EBlockState.None;

        originPos = rectTransform.anchoredPosition;

        btn.OnClickAsObservable().Subscribe(async _ =>
        {
            if (IsFixdBlock() == true)
                return;

            if (game.isDrag == false && game.isAni == false)
            {
                await Boom();
            }
        });

        btn.OnBeginDragAsObservable().Subscribe(_ =>
        {
            if (game.isDrag == true || IsFixdBlock() == true)
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

            if (game.isAni == true || IsFixdBlock() == true)
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
                            if (ret.Item2.IsFixdBlock() == true)
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
                            if (ret.Item2.IsFixdBlock() == true)
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
                            if (ret.Item2.IsFixdBlock() == true)
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
                            if (ret.Item2.IsFixdBlock() == true)
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

    public void SetHp(int _hp)
    {
        if (_hp < 0)
        {
            hpText.gameObject.SetActive(false);
        }
        else
        {
            hpText.gameObject.SetActive(true);
        }

        hp = _hp;
        hpText.SetText(hp);
    }

    public int GetHp()
    {
        return hp;
    }

    public void SetBlockState(Defines.ELog _log, int _key, Defines.EBlockState _blockState)
    {
        blockState = _blockState;
        match = false;

        CheckNoneBlockLog(_log, _key);
    }

    public Defines.EBlockState GetBlockState()
    {
        return blockState;
    }

    public void CheckNoneBlockLog(Defines.ELog _log, int _key)
    {
        if (blockState == Defines.EBlockState.None)
        {
            Debug.Log($"Function: {_log.ToString()} {_key} Block{row}/{col} state is None");
        }
    }

    public void Damage()
    {
        if (checkDamage == false && hp >= 1)
        {
            checkDamage = true;
            hp -= 1;

            if (hp > 0)
            {
                hpText.SetText(hp);
            }
            else
            {
                match = true;
            }
        }
    }

    public void ResetCheckWallDamage()
    {
        checkDamage = false;
    }

    public async Task Boom(bool ani = true)
    {
        switch (blockState)
        {
            case Defines.EBlockState.CatPang1:
            case Defines.EBlockState.CatPang2:
            case Defines.EBlockState.CatPang3:
            case Defines.EBlockState.CatPang4:
            case Defines.EBlockState.CatPang5:
                await game.Boom1(this, ani);
                break;
            case Defines.EBlockState.Arrow1:
                await game.Boom4(this, ani);
                break;
            case Defines.EBlockState.Arrow2:
                await game.Boom7(this, ani);
                break;
            case Defines.EBlockState.Arrow3:
                await game.Boom5(this, ani);
                break;
            case Defines.EBlockState.Arrow4:
                await game.Boom8(this, ani);
                break;
            case Defines.EBlockState.Arrow5:
                await game.Boom2(this, ani);
                break;
            case Defines.EBlockState.Arrow6:
                await game.Boom6(this, ani);
                break;
            case Defines.EBlockState.Bomb:
                // 드래그 해야 함
                break;
        }
    }

    public Defines.EBlockState GetPangType()
    {
        switch (blockState)
        {
            case Defines.EBlockState.Cat1:
                return Defines.EBlockState.CatPang1;
            case Defines.EBlockState.Cat2:
                return Defines.EBlockState.CatPang2;
            case Defines.EBlockState.Cat3:
                return Defines.EBlockState.CatPang3;
            case Defines.EBlockState.Cat4:
                return Defines.EBlockState.CatPang4;
            case Defines.EBlockState.Cat5:
                return Defines.EBlockState.CatPang5;
            default:
                return Defines.EBlockState.None;
        }
    }

    public bool IsNormalBlock()
    {
        switch (blockState)
        {
            case Defines.EBlockState.Cat1:
            case Defines.EBlockState.Cat2:
            case Defines.EBlockState.Cat3:
            case Defines.EBlockState.Cat4:
            case Defines.EBlockState.Cat5:
                return true;
            default:
                return false;
        }
    }

    public bool IsBoomBlock()
    {
        switch (blockState)
        {
            case Defines.EBlockState.CatPang1:
            case Defines.EBlockState.CatPang2:
            case Defines.EBlockState.CatPang3:
            case Defines.EBlockState.CatPang4:
            case Defines.EBlockState.CatPang5:
            case Defines.EBlockState.Arrow1:
            case Defines.EBlockState.Arrow2:
            case Defines.EBlockState.Arrow3:
            case Defines.EBlockState.Arrow4:
            case Defines.EBlockState.Arrow5:
            case Defines.EBlockState.Arrow6:
                return true;
            default:
                return false;
        }
    }

    public bool IsFixdBlock()
    {
        switch (blockState)
        {
            case Defines.EBlockState.Wall:
            case Defines.EBlockState.Potal:
                return true;
            default:
                return false;
        }
    }

    public bool IsSpecialBlock()
    {
        switch (blockState)
        {
            case Defines.EBlockState.Bomb:
                return true;
            default:
                return false;
        }
    }

    public bool IsMatch()
    {
        return match == true;
    }

    public bool IsBlock()
    {
        return blockState != Defines.EBlockState.None;
    }
}