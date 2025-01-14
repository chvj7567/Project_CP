using DG.Tweening;
using System.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using static Defines;
using static Infomation;

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

    public Image background;
    public Image img;
    public RectTransform rectTransform;
    public Vector2 originPos;

    [ReadOnly]
    public int index;
    [ReadOnly]
    public int row;
    [ReadOnly]
    public int col;
    [SerializeField, ReadOnly]
    public Defines.EBlockState changeBlockState = Defines.EBlockState.None;
    [SerializeField, ReadOnly]
    Defines.EBlockState blockState = Defines.EBlockState.None;
    [ReadOnly]
    public int hScore;
    [ReadOnly]
    public int vScore;

    // 매치되었는지 확인
    public bool match = false;
    // 폭탄 블럭일 경우 터졌는지 확인
    public bool boom = false;
    // 사각형 매치되었는지 확인(폭탄 블럭 생성되는 곳에만 true)
    public bool squareMatch = false;
    // 없어진 블럭인지 확인
    public bool remove = false;
    // 튜토리얼 블럭 유무
    public bool tutorialBlock = false;
    // hp가 게임 클리어에 영향 가는지
    public bool checkHp = true;

    // 벽HP
    [SerializeField, ReadOnly] int hp = 0;
    public int changeHp = -1;

    // 벽은 한 턴에 한 번만 데미지를 입음
    [SerializeField, ReadOnly] bool checkDamage = false;

    private void Start()
    {
        changeBlockState = EBlockState.None;

        originPos = rectTransform.anchoredPosition;

        btn.OnClickAsObservable().Subscribe(async _ =>
        {
            if (IsFixdBlock())
                return;

            if (game.isDrag == false && game.isLock == false &&
            blockState != Defines.EBlockState.RainbowPang)
            {
                tutorialBlock = false;
                await Bomb();
            }
        });

        btn.OnBeginDragAsObservable().Subscribe(_ =>
        {
            if (game.isDrag || IsFixdBlock() || CanNotDragBlock())
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

            if (game.gameEnd || game.isLock || IsFixdBlock() || CanNotDragBlock())
                return;

            Vector2 rectPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(backRect, _.position, _.pressEventCamera, out rectPosition);

            var angle = EvalDragAngle(originPos, rectPosition);

            int swipe = (((int)angle + 45) % 360) / 90;

            switch (swipe)
            {
                // Right
                case 0:
                    await Drag(EDrag.Right);
                    break;
                // Up
                case 1:
                    await Drag(EDrag.Up);
                    break;
                // Left
                case 2:
                    await Drag(EDrag.Left);
                    break;
                // Down
                case 3:
                    await Drag(EDrag.Down);
                    break;
            }
        });
    }

    public async Task Drag(Defines.EDrag drag)
    {
        switch (drag)
        {
            // Right
            case Defines.EDrag.Right:
                {
                    var addDis = CHInstantiateButton.GetHorizontalDistance();
                    var movePos = new Vector2(originPos.x + addDis, originPos.y);
                    var ret = CHInstantiateButton.GetBlockInfo(movePos);

                    if (ret.Item1 != null)
                    {
                        if (ret.Item2.IsFixdBlock() || ret.Item2.CanNotDragBlock())
                            break;

                        if (game.CheckTutorial() &&
                        (tutorialBlock == false || ret.Item2.tutorialBlock == false))
                            break;

                        rectTransform.DOAnchorPosX(movePos.x, game.delay);
                        ret.Item1.DOAnchorPosX(originPos.x, game.delay);

                        ChangeBlock(ret.Item2);
                        await game.AfterDrag(this, ret.Item2);
                    }
                }
                break;
            // Up
            case Defines.EDrag.Up:
                {
                    var addDis = CHInstantiateButton.GetVerticalDistance();
                    var movePos = new Vector2(originPos.x, originPos.y + addDis);
                    var ret = CHInstantiateButton.GetBlockInfo(movePos);
                    if (ret.Item1 != null)
                    {
                        if (ret.Item2.IsFixdBlock() || ret.Item2.CanNotDragBlock())
                            break;

                        if (game.CheckTutorial() &&
                        (tutorialBlock == false || ret.Item2.tutorialBlock == false))
                            break;

                        rectTransform.DOAnchorPosY(movePos.y, game.delay);
                        ret.Item1.DOAnchorPosY(originPos.y, game.delay);

                        ChangeBlock(ret.Item2);
                        await game.AfterDrag(this, ret.Item2);
                    }
                }
                break;
            // Left
            case Defines.EDrag.Left:
                {
                    var addDis = CHInstantiateButton.GetHorizontalDistance();
                    var movePos = new Vector2(originPos.x - addDis, originPos.y);
                    var ret = CHInstantiateButton.GetBlockInfo(movePos);
                    if (ret.Item1 != null)
                    {
                        if (ret.Item2.IsFixdBlock() || ret.Item2.CanNotDragBlock())
                            break;

                        if (game.CheckTutorial() &&
                        (tutorialBlock == false || ret.Item2.tutorialBlock == false))
                            break;

                        rectTransform.DOAnchorPosX(movePos.x, game.delay);
                        ret.Item1.DOAnchorPosX(originPos.x, game.delay);

                        ChangeBlock(ret.Item2);
                        await game.AfterDrag(this, ret.Item2);
                    }
                }
                break;
            // Down
            case Defines.EDrag.Down:
                {
                    var addDis = CHInstantiateButton.GetVerticalDistance();
                    var movePos = new Vector2(originPos.x, originPos.y - addDis);
                    var ret = CHInstantiateButton.GetBlockInfo(movePos);
                    if (ret.Item1 != null)
                    {
                        if (ret.Item2.IsFixdBlock() || ret.Item2.CanNotDragBlock())
                            break;

                        if (game.CheckTutorial() &&
                        (tutorialBlock == false || ret.Item2.tutorialBlock == false))
                            break;

                        rectTransform.DOAnchorPosY(movePos.y, game.delay);
                        ret.Item1.DOAnchorPosY(originPos.y, game.delay);

                        ChangeBlock(ret.Item2);
                        await game.AfterDrag(this, ret.Item2);
                    }
                }
                break;
            case Defines.EDrag.Click:
                {
                    await Bomb();
                }
                break;
        }
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

        changeHp = -1;
    }

    public int GetHp()
    {
        return hp;
    }

    public void SetBlockState(Defines.ELog _log, int _key, Sprite _sprite, Defines.EBlockState _blockState)
    {
        blockState = _blockState;
        match = false;
        img.sprite = _sprite;
        img.color = new Color(1, 1, 1, 1);
        background.color = new Color(PlayerPrefs.GetFloat(CHMMain.String.Red), PlayerPrefs.GetFloat(CHMMain.String.Green), PlayerPrefs.GetFloat(CHMMain.String.Blue), PlayerPrefs.GetFloat(CHMMain.String.Alpha));

        switch (blockState)
        {
            case EBlockState.Wall:
                img.rectTransform.sizeDelta = new Vector2(30, 30);
                img.color = new Color(.5f, .5f, .5f);
                background.color = new Color(0, 0, 0, .5f);
                break;
            case EBlockState.Potal:
            case EBlockState.PotalCreator:
            case EBlockState.WallCreator:
                img.rectTransform.sizeDelta = new Vector2(-20, -20);
                background.color = new Color(0, 0, 0, .2f);
                break;
            case EBlockState.Fish:
                background.color = new Color(0, 0, 0, .35f);
                break;
            case EBlockState.Locker1:
            case EBlockState.Locker2:
            case EBlockState.Locker3:
            case EBlockState.Locker4:
            case EBlockState.Locker5:
                img.rectTransform.sizeDelta = new Vector2(70, 70);
                break;
            case EBlockState.CatBox1:
            case EBlockState.CatBox2:
            case EBlockState.CatBox3:
            case EBlockState.CatBox4:
            case EBlockState.CatBox5:
            case EBlockState.LockerBox1:
            case EBlockState.LockerBox2:
            case EBlockState.LockerBox3:
            case EBlockState.LockerBox4:
            case EBlockState.LockerBox5:
                hpText.GetComponent<RectTransform>().DOAnchorPosY(30, .1f);
                img.rectTransform.sizeDelta = new Vector2(10, 10);
                background.color = new Color(0, 0, 0, 0);
                break;
            case EBlockState.Ball:
                img.rectTransform.sizeDelta = new Vector2(-10, -10);
                break;
            default:
                img.rectTransform.sizeDelta = new Vector2(30, 30);
                break;
        }

        CheckNoneBlockLog(_log, _key);
    }

    public EBlockState CheckSelectCatShop(EBlockState _blockState)
    {
        var data = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        if (data == null)
            return _blockState;

        if (data.selectCatShop == 0)
            return _blockState;
        else if (data.selectCatShop == 1)
        {
            switch (_blockState)
            {
                case EBlockState.Cat1:
                    return EBlockState.CatHat1;
                case EBlockState.Cat2:
                    return EBlockState.CatHat2;
                case EBlockState.Cat3:
                    return EBlockState.CatHat3;
                case EBlockState.Cat4:
                    return EBlockState.CatHat4;
                case EBlockState.Cat5:
                    return EBlockState.CatHat5;
            }
        }
        else if (data.selectCatShop == 2)
        {
            switch (_blockState)
            {
                case EBlockState.Cat1:
                    return EBlockState.CatSkin1;
                case EBlockState.Cat2:
                    return EBlockState.CatSkin2;
                case EBlockState.Cat3:
                    return EBlockState.CatSkin3;
                case EBlockState.Cat4:
                    return EBlockState.CatSkin4;
                case EBlockState.Cat5:
                    return EBlockState.CatSkin5;
            }
        }
        else if (data.selectCatShop == 3)
        {
            switch (_blockState)
            {
                case EBlockState.Cat1:
                    return EBlockState.Locker1;
                case EBlockState.Cat2:
                    return EBlockState.Locker2;
                case EBlockState.Cat3:
                    return EBlockState.Locker3;
                case EBlockState.Cat4:
                    return EBlockState.Locker4;
                case EBlockState.Cat5:
                    return EBlockState.Locker5;
                case EBlockState.CatBox1:
                    return EBlockState.LockerBox1;
                case EBlockState.CatBox2:
                    return EBlockState.LockerBox2;
                case EBlockState.CatBox3:
                    return EBlockState.LockerBox3;
                case EBlockState.CatBox4:
                    return EBlockState.LockerBox4;
                case EBlockState.CatBox5:
                    return EBlockState.LockerBox5;
            }
        }

        return _blockState;
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

    public void Damage(int _blockMaxIndex = 0, bool changeNormalBlock = true)
    {
        if (checkDamage == false && hp >= 1 && IsBoxBlock() == false)
        {
            checkDamage = true;
            hp -= 1;

            if (hp > 0)
            {
                hpText.SetText(hp);
            }
            else
            {
                if (changeNormalBlock)
                {
                    var random = UnityEngine.Random.Range(0, _blockMaxIndex);
                    changeBlockState = (Defines.EBlockState)random;
                }
            }
        }
    }

    public void ResetCheckWallDamage()
    {
        checkDamage = false;
    }

    public async Task Bomb(bool ani = true)
    {
        switch (blockState)
        {
            case Defines.EBlockState.CatPang:
                await game.Bomb1(this, ani);
                break;
            case Defines.EBlockState.Arrow1:
                await game.Bomb4(this, ani);
                break;
            case Defines.EBlockState.Arrow2:
                await game.Bomb7(this, ani);
                break;
            case Defines.EBlockState.Arrow3:
                await game.Bomb5(this, ani);
                break;
            case Defines.EBlockState.Arrow4:
                await game.Bomb8(this, ani);
                break;
            case Defines.EBlockState.Arrow5:
                await game.Bomb2(this, ani);
                break;
            case Defines.EBlockState.Arrow6:
                await game.Bomb6(this, ani);
                break;
            case Defines.EBlockState.PinkBomb:
                // 드래그 해야 함
                break;
            case Defines.EBlockState.YellowBomb:
                await game.Bomb9(this, ani);
                break;
            case Defines.EBlockState.OrangeBomb:
                await game.Bomb10(this, ani);
                break;
            case Defines.EBlockState.BlueBomb:
                await game.Bomb11(this, ani);
                break;
            case Defines.EBlockState.GreenBomb:
                await game.Bomb12(this, ani);
                break;
            case Defines.EBlockState.RainbowPang:
                await game.RainbowPang(this, ani);
                break;
        }
    }

    public Defines.EBlockState GetPangType()
    {
        return Defines.EBlockState.CatPang;
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
            case Defines.EBlockState.Cat6:
            case Defines.EBlockState.Cat7:
            case Defines.EBlockState.Cat8:
            case Defines.EBlockState.Cat9:
            case Defines.EBlockState.Cat10:
            case Defines.EBlockState.CatHat1:
            case Defines.EBlockState.CatHat2:
            case Defines.EBlockState.CatHat3:
            case Defines.EBlockState.CatHat4:
            case Defines.EBlockState.CatHat5:
            case Defines.EBlockState.CatSkin1:
            case Defines.EBlockState.CatSkin2:
            case Defines.EBlockState.CatSkin3:
            case Defines.EBlockState.CatSkin4:
            case Defines.EBlockState.CatSkin5:
            case Defines.EBlockState.Locker1:
            case Defines.EBlockState.Locker2:
            case Defines.EBlockState.Locker3:
            case Defines.EBlockState.Locker4:
            case Defines.EBlockState.Locker5:
                return true;
            default:
                return false;
        }
    }

    public bool IsBombBlock()
    {
        switch (blockState)
        {
            case Defines.EBlockState.CatPang:
            case Defines.EBlockState.Arrow1:
            case Defines.EBlockState.Arrow2:
            case Defines.EBlockState.Arrow3:
            case Defines.EBlockState.Arrow4:
            case Defines.EBlockState.Arrow5:
            case Defines.EBlockState.Arrow6:
            case Defines.EBlockState.GreenBomb:
            case Defines.EBlockState.OrangeBomb:
            case Defines.EBlockState.BlueBomb:
            case Defines.EBlockState.YellowBomb:
            case Defines.EBlockState.PinkBomb:
            case Defines.EBlockState.RainbowPang:
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
            case Defines.EBlockState.CatBox1:
            case Defines.EBlockState.CatBox2:
            case Defines.EBlockState.CatBox3:
            case Defines.EBlockState.CatBox4:
            case Defines.EBlockState.CatBox5:
            case Defines.EBlockState.LockerBox1:
            case Defines.EBlockState.LockerBox2:
            case Defines.EBlockState.LockerBox3:
            case Defines.EBlockState.LockerBox4:
            case Defines.EBlockState.LockerBox5:
            case Defines.EBlockState.WallCreator:
            case Defines.EBlockState.PotalCreator:
                return true;
            default:
                return false;
        }
    }

    public bool IsWallBlock()
    {
        switch (blockState)
        {
            case Defines.EBlockState.Wall:
            case Defines.EBlockState.CatBox1:
            case Defines.EBlockState.CatBox2:
            case Defines.EBlockState.CatBox3:
            case Defines.EBlockState.CatBox4:
            case Defines.EBlockState.CatBox5:
            case Defines.EBlockState.LockerBox1:
            case Defines.EBlockState.LockerBox2:
            case Defines.EBlockState.LockerBox3:
            case Defines.EBlockState.LockerBox4:
            case Defines.EBlockState.LockerBox5:
            case Defines.EBlockState.WallCreator:
            case Defines.EBlockState.PotalCreator:
                return true;
            default:
                return false;
        }
    }

    public bool CanNotDragBlock()
    {
        switch (blockState)
        {
            case Defines.EBlockState.Wall:
            case Defines.EBlockState.Potal:
            case Defines.EBlockState.Fish:
            case Defines.EBlockState.CatBox1:
            case Defines.EBlockState.CatBox2:
            case Defines.EBlockState.CatBox3:
            case Defines.EBlockState.CatBox4:
            case Defines.EBlockState.CatBox5:
            case Defines.EBlockState.LockerBox1:
            case Defines.EBlockState.LockerBox2:
            case Defines.EBlockState.LockerBox3:
            case Defines.EBlockState.LockerBox4:
            case Defines.EBlockState.LockerBox5:
            case Defines.EBlockState.WallCreator:
            case Defines.EBlockState.PotalCreator:
            case Defines.EBlockState.RainbowPang:
                return true;
            default:
                return false;
        }
    }

    public bool IsSpecialBombBlock()
    {
        switch (blockState)
        {
            case Defines.EBlockState.PinkBomb:
            case Defines.EBlockState.GreenBomb:
            case Defines.EBlockState.OrangeBomb:
            case Defines.EBlockState.BlueBomb:
            case Defines.EBlockState.YellowBomb:
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

    public bool IsFishBlock()
    {
        switch (blockState)
        {
            case Defines.EBlockState.Fish:
                return true;
            default:
                return false;
        }
    }

    public bool IsBallBlock()
    {
        switch (blockState)
        {
            case Defines.EBlockState.Ball:
                return true;
            default:
                return false;
        }
    }

    public bool IsBoxBlock()
    {
        switch (blockState)
        {
            case Defines.EBlockState.CatBox1:
            case Defines.EBlockState.CatBox2:
            case Defines.EBlockState.CatBox3:
            case Defines.EBlockState.CatBox4:
            case Defines.EBlockState.CatBox5:
            case Defines.EBlockState.LockerBox1:
            case Defines.EBlockState.LockerBox2:
            case Defines.EBlockState.LockerBox3:
            case Defines.EBlockState.LockerBox4:
            case Defines.EBlockState.LockerBox5:
                return true;
            default:
                return false;
        }
    }

    bool CheckInBoxBlock(Defines.EBlockState upBlockState)
    {
        // 자기 자신이 박스 블럭이 아니면 확인 X
        if (IsBoxBlock() == false)
            return false;

        switch (blockState)
        {
            case EBlockState.CatBox1:
            case EBlockState.LockerBox1:
                {
                    switch (upBlockState)
                    {
                        case Defines.EBlockState.Cat1:
                        case Defines.EBlockState.Cat6:
                        case Defines.EBlockState.CatHat1:
                        case Defines.EBlockState.CatSkin1:
                        case Defines.EBlockState.Locker1:
                            return true;
                        default:
                            return false;
                    }
                }
            case EBlockState.CatBox2:
            case EBlockState.LockerBox2:
                {
                    switch (upBlockState)
                    {
                        case Defines.EBlockState.Cat2:
                        case Defines.EBlockState.Cat7:
                        case Defines.EBlockState.CatHat2:
                        case Defines.EBlockState.CatSkin2:
                        case Defines.EBlockState.Locker2:
                            return true;
                        default:
                            return false;
                    }
                }
            case EBlockState.CatBox3:
            case EBlockState.LockerBox3:
                {
                    switch (upBlockState)
                    {
                        case Defines.EBlockState.Cat3:
                        case Defines.EBlockState.Cat8:
                        case Defines.EBlockState.CatHat3:
                        case Defines.EBlockState.CatSkin3:
                        case Defines.EBlockState.Locker3:
                            return true;
                        default:
                            return false;
                    }
                }
            case EBlockState.CatBox4:
            case EBlockState.LockerBox4:
                {
                    switch (upBlockState)
                    {
                        case Defines.EBlockState.Cat4:
                        case Defines.EBlockState.Cat9:
                        case Defines.EBlockState.CatHat4:
                        case Defines.EBlockState.CatSkin4:
                        case Defines.EBlockState.Locker4:
                            return true;
                        default:
                            return false;
                    }
                }
            case EBlockState.CatBox5:
            case EBlockState.LockerBox5:
                {
                    switch (upBlockState)
                    {
                        case Defines.EBlockState.Cat5:
                        case Defines.EBlockState.Cat10:
                        case Defines.EBlockState.CatHat5:
                        case Defines.EBlockState.CatSkin5:
                        case Defines.EBlockState.Locker5:
                            return true;
                        default:
                            return false;
                    }
                }
            default:
                return false;
        }
    }

    public bool CatInTheBox(Defines.EBlockState upBlockState)
    {
        if (CheckInBoxBlock(upBlockState) == false)
            return false;

        if (hp <= 0)
            return true;

        hp -= 1;

        if (hp > 0)
        {
            hpText.SetText(hp);
        }
        else
        {
            hpText.gameObject.SetActive(false);
        }

        return true;
    }

    public bool CheckHpBlock()
    {
        switch (blockState)
        {
            case Defines.EBlockState.WallCreator:
            case Defines.EBlockState.PotalCreator:
                return false;
            default:
                return true;
        }
    }
}