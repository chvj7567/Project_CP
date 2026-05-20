using DG.Tweening;
using System.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using ChvjUnityInfra;
using UnityEngine.UI;
using static Defines;
using static Infomation;

[RequireComponent(typeof(RectTransform))]
public class Block : MonoBehaviour
{
    [SerializeField]
    GPGameScene game;
    [SerializeField]
    Button btn;
    [SerializeField]
    RectTransform backRect;
    [SerializeField]
    CHText hpText;

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

    // 매치 상태 (매치되었는지 확인)
    public bool match = false;
    // 폭탄 생성 혹은 연출용 매치 상태인지 확인
    public bool boom = false;
    // 사각형으로 매치되었는지 확인 (폭탄 생성 조건일 때 true)
    public bool squareMatch = false;
    // 블럭 삭제 대상 여부 확인
    public bool remove = false;
    // 튜토리얼 대상 블럭인지 여부
    public bool tutorialBlock = false;
    // 체력(HP)을 체크해야 하는 블럭인지 여부
    public bool checkHp = true;

    // 현재 HP
    [SerializeField, ReadOnly] int hp = 0;
    public int changeHp = -1;

    // 중복 데미지 처리 방지 플래그
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
            // 자체 드래그 이벤트 대신 BeginDrag, EndDrag 핸들러를 사용함
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
        background.color = new Color(PlayerPrefs.GetFloat(CHMString.Instance.Red), PlayerPrefs.GetFloat(CHMString.Instance.Green), PlayerPrefs.GetFloat(CHMString.Instance.Blue), PlayerPrefs.GetFloat(CHMString.Instance.Alpha));

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
            case EBlockState.CatBox1:
            case EBlockState.CatBox2:
            case EBlockState.CatBox3:
            case EBlockState.CatBox4:
            case EBlockState.CatBox5:
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

    // 스킨 적용된 고양이 블록을 원본 Cat1~7 타입으로 환산. 고양이가 아니면 None
    // 전제: Cat1~7(0~6)과 신규 스킨(CatCrown1~CatStrawberry5, 54~83)이 각각 연속 배치되어 있음
    public static EBlockState GetBaseCat(EBlockState _state)
    {
        int v = (int)_state;

        if (v >= (int)EBlockState.Cat1 && v <= (int)EBlockState.Cat7)
            return _state;

        if (v >= (int)EBlockState.CatCrown1 && v <= (int)EBlockState.CatStrawberry5)
            return EBlockState.Cat1 + ((v - (int)EBlockState.CatCrown1) % 5);

        return EBlockState.None;
    }

    public EBlockState CheckSelectCatShop(EBlockState _blockState)
    {
        var data = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (data == null)
            return _blockState;

        // selectCatShop: 0=없음, 1~6=스킨 테마. Cat1~5만 스킨 적용 대상
        int catIndex;
        switch (_blockState)
        {
            case EBlockState.Cat1: catIndex = 0; break;
            case EBlockState.Cat2: catIndex = 1; break;
            case EBlockState.Cat3: catIndex = 2; break;
            case EBlockState.Cat4: catIndex = 3; break;
            case EBlockState.Cat5: catIndex = 4; break;
            default: return _blockState;
        }

        switch (data.selectCatShop)
        {
            case 1: return EBlockState.CatCrown1 + catIndex;
            case 2: return EBlockState.CatFlowers1 + catIndex;
            case 3: return EBlockState.CatMushroom1 + catIndex;
            case 4: return EBlockState.CatParty1 + catIndex;
            case 5: return EBlockState.CatSanta1 + catIndex;
            case 6: return EBlockState.CatStrawberry1 + catIndex;
            default: return _blockState;
        }
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
                // 추후 특수 기능 구현 혹은 처리 생략
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
        // Cat1~7 및 스킨 적용된 고양이 블록은 모두 일반 블록
        return GetBaseCat(blockState) != Defines.EBlockState.None;
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
                return true;
            default:
                return false;
        }
    }

    bool CheckInBoxBlock(Defines.EBlockState upBlockState)
    {
        // 자기 자신의 히트박스가 아니면 무시함
        if (IsBoxBlock() == false)
            return false;

        // 위 블록을 원본 고양이 타입으로 환산해 박스가 받는 고양이인지 판정
        var baseCat = GetBaseCat(upBlockState);

        switch (blockState)
        {
            case EBlockState.CatBox1:
                return baseCat == EBlockState.Cat1 || baseCat == EBlockState.Cat6;
            case EBlockState.CatBox2:
                return baseCat == EBlockState.Cat2 || baseCat == EBlockState.Cat7;
            case EBlockState.CatBox3:
                return baseCat == EBlockState.Cat3;
            case EBlockState.CatBox4:
                return baseCat == EBlockState.Cat4;
            case EBlockState.CatBox5:
                return baseCat == EBlockState.Cat5;
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
