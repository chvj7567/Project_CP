using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static Defines;

public class GPBombResolver
{
    GPBoard _board;
    GPMatchChecker _matcher;
    Func<Task> _onBoomTrigger;
    Action<int> _addBonusScore;
    Func<ParticleSystem, Vector2, RectTransform> _createEffect;
    Action<ESound> _playSound;
    CancellationToken _token;
    int _arrowPangIndex;
    List<ParticleSystem> _pangEffectList;
    ParticleSystem _bombEffectPS;

    public void Init(
        GPBoard board,
        GPMatchChecker matcher,
        Func<Task> onBoomTrigger,
        Action<int> addBonusScore,
        Func<ParticleSystem, Vector2, RectTransform> createEffect,
        Action<ESound> playSound,
        List<ParticleSystem> pangEffectList,
        ParticleSystem bombEffectPS,
        CancellationToken token)
    {
        _board = board;
        _matcher = matcher;
        _onBoomTrigger = onBoomTrigger;
        _addBonusScore = addBonusScore;
        _createEffect = createEffect;
        _playSound = playSound;
        _pangEffectList = pangEffectList;
        _bombEffectPS = bombEffectPS;
        _token = token;
    }

    public void SetArrowPangIndex(int index) => _arrowPangIndex = index;

    void SaveBombCollectionData(Block block)
    {
        if (!block.IsBombBlock() && !block.IsSpecialBombBlock()) return;
        var collectionData = CHMData.Instance.GetCollectionData(block.GetBlockState().ToString());
        collectionData.value += 1;
    }

    public async Task<bool> CatPang(bool check = false)
    {
        for (int w = 0; w < _board.boardSize; ++w)
        {
            for (int h = 0; h < _board.boardSize; ++h)
            {
                var b = _board.boardArr[w, h];
                if (b.IsBombBlock() &&
                    b.GetBlockState() != EBlockState.PinkBomb &&
                    b.GetBlockState() != EBlockState.RainbowPang)
                {
                    if (check) return true;
                    await b.Bomb();
                    w = -1; break;
                }
            }
        }
        return false;
    }

    public async Task BoomAll(bool ani = true)
    {
        _addBonusScore(5);
        for (int i = 0; i < 9; ++i)
            for (int j = 0; j < 9; ++j)
                _matcher.ChangeMatchState(i, j);
        if (ani) await _onBoomTrigger();
    }

    public async Task Bomb1(Block block, bool ani = true)
    {
        _addBonusScore(10);
        block.match = true; block.boom = true;
        _playSound(ESound.Ching);
        _createEffect(_pangEffectList[(int)EPangEffect.Explosion], block.rectTransform.anchoredPosition);
        _matcher.ChangeMatchState(block.row - 1, block.col - 1);
        _matcher.ChangeMatchState(block.row - 1, block.col);
        _matcher.ChangeMatchState(block.row - 1, block.col + 1);
        _matcher.ChangeMatchState(block.row, block.col - 1);
        _matcher.ChangeMatchState(block.row, block.col + 1);
        _matcher.ChangeMatchState(block.row + 1, block.col - 1);
        _matcher.ChangeMatchState(block.row + 1, block.col);
        _matcher.ChangeMatchState(block.row + 1, block.col + 1);
        SaveBombCollectionData(block);
        if (ani) await _onBoomTrigger();
    }

    public async Task Bomb2(Block block, bool ani = true)
    {
        _addBonusScore(10);
        block.match = true; block.boom = true;
        _playSound(ESound.Pising);
        _createEffect(_pangEffectList[(int)EPangEffect.Move_Line], block.rectTransform.anchoredPosition);
        for (int i = 0; i < 9; ++i) _matcher.ChangeMatchState(i, block.col);
        for (int i = 0; i < 9; ++i) _matcher.ChangeMatchState(block.row, i);
        SaveBombCollectionData(block);
        if (ani) await _onBoomTrigger();
    }

    public async Task Boom3(Block specialBlock, EBlockState blockState, bool ani = true)
    {
        _addBonusScore(10);
        specialBlock.match = true; specialBlock.boom = true;
        var blueHoleList = new List<GameObject>();
        for (int i = 0; i < _board.boardSize; ++i)
        {
            for (int j = 0; j < _board.boardSize; ++j)
            {
                var b = _board.boardArr[i, j];
                if (b == null) continue;
                if (b.GetBlockState() == blockState)
                {
                    b.match = true;
                    var rt = _createEffect(_bombEffectPS, specialBlock.rectTransform.anchoredPosition);
                    rt.DOAnchorPos(b.rectTransform.anchoredPosition, .05f);
                    blueHoleList.Add(rt.gameObject);
                    await Task.Delay(200, _token);
                }
            }
        }
        SaveBombCollectionData(specialBlock);
        if (ani) _onBoomTrigger();
        await Task.Delay(1000, _token);
        foreach (var go in blueHoleList) CHMResource.Instance.Destroy(go);
    }

    public async Task Bomb4(Block block, bool ani = true)
    {
        _addBonusScore(10);
        block.match = true; block.boom = true;
        _playSound(ESound.Pising);
        var rt = _createEffect(_pangEffectList[(int)EPangEffect.Move_Line2], block.rectTransform.anchoredPosition);
        rt.Rotate(new Vector3(0, 0, 0));
        var rt2 = _createEffect(_pangEffectList[(int)EPangEffect.Move_Line2], block.rectTransform.anchoredPosition);
        rt2.Rotate(new Vector3(0, 0, 180));
        for (int i = 0; i < 9; ++i) _matcher.ChangeMatchState(block.row, i);
        SaveBombCollectionData(block);
        if (ani) await _onBoomTrigger();
    }

    public async Task Bomb5(Block block, bool ani = true)
    {
        _addBonusScore(10);
        block.match = true; block.boom = true;
        _playSound(ESound.Pising);
        var rt = _createEffect(_pangEffectList[(int)EPangEffect.Move_Line2], block.rectTransform.anchoredPosition);
        rt.Rotate(new Vector3(0, 0, 90));
        var rt2 = _createEffect(_pangEffectList[(int)EPangEffect.Move_Line2], block.rectTransform.anchoredPosition);
        rt2.Rotate(new Vector3(0, 0, 270));
        for (int i = 0; i < 9; ++i) _matcher.ChangeMatchState(i, block.col);
        SaveBombCollectionData(block);
        if (ani) await _onBoomTrigger();
    }

    public async Task Bomb6(Block block, bool ani = true)
    {
        _addBonusScore(10);
        block.match = true; block.boom = true;
        _playSound(ESound.Pising);
        var rt = _createEffect(_pangEffectList[(int)EPangEffect.Center_Hit], block.rectTransform.anchoredPosition);
        rt.Rotate(new Vector3(0, 0, 45));
        for (int i = 0; i < 9; ++i)
        {
            _matcher.ChangeMatchState(block.row - i, block.col - i);
            _matcher.ChangeMatchState(block.row - i, block.col + i);
            _matcher.ChangeMatchState(block.row + i, block.col - i);
            _matcher.ChangeMatchState(block.row + i, block.col + i);
        }
        SaveBombCollectionData(block);
        if (ani) await _onBoomTrigger();
    }

    public async Task Bomb7(Block block, bool ani = true)
    {
        _addBonusScore(10);
        block.match = true; block.boom = true;
        _playSound(ESound.Pising);
        var rt = _createEffect(_pangEffectList[(int)EPangEffect.Move_Line2], block.rectTransform.anchoredPosition);
        rt.Rotate(new Vector3(0, 0, 45));
        var rt2 = _createEffect(_pangEffectList[(int)EPangEffect.Move_Line2], block.rectTransform.anchoredPosition);
        rt2.Rotate(new Vector3(0, 0, 225));
        for (int i = 0; i < 9; ++i)
        {
            _matcher.ChangeMatchState(block.row - i, block.col + i);
            _matcher.ChangeMatchState(block.row + i, block.col - i);
        }
        SaveBombCollectionData(block);
        if (ani) await _onBoomTrigger();
    }

    public async Task Bomb8(Block block, bool ani = true)
    {
        _addBonusScore(10);
        block.match = true; block.boom = true;
        _playSound(ESound.Pising);
        var rt = _createEffect(_pangEffectList[(int)EPangEffect.Move_Line2], block.rectTransform.anchoredPosition);
        rt.Rotate(new Vector3(0, 0, -45));
        var rt2 = _createEffect(_pangEffectList[(int)EPangEffect.Move_Line2], block.rectTransform.anchoredPosition);
        rt2.Rotate(new Vector3(0, 0, -225));
        for (int i = 0; i < 9; ++i)
        {
            _matcher.ChangeMatchState(block.row - i, block.col - i);
            _matcher.ChangeMatchState(block.row + i, block.col + i);
        }
        SaveBombCollectionData(block);
        if (ani) await _onBoomTrigger();
    }

    public async Task Bomb9(Block block, bool ani = true)
    {
        _addBonusScore(10);
        block.match = true; block.boom = true;
        _playSound(ESound.Pising);
        int random = UnityEngine.Random.Range((int)EPangEffect.Blue, (int)EPangEffect.Green + 1);
        _createEffect(_pangEffectList[random], block.rectTransform.anchoredPosition);
        _matcher.ChangeMatchState(block.row - 2, block.col);
        _matcher.ChangeMatchState(block.row - 1, block.col);
        _matcher.ChangeMatchState(block.row - 1, block.col - 1);
        _matcher.ChangeMatchState(block.row - 1, block.col + 1);
        _matcher.ChangeMatchState(block.row, block.col - 2);
        _matcher.ChangeMatchState(block.row, block.col - 1);
        _matcher.ChangeMatchState(block.row, block.col + 1);
        _matcher.ChangeMatchState(block.row, block.col + 2);
        _matcher.ChangeMatchState(block.row + 1, block.col - 1);
        _matcher.ChangeMatchState(block.row + 1, block.col + 1);
        _matcher.ChangeMatchState(block.row + 1, block.col);
        _matcher.ChangeMatchState(block.row + 2, block.col);
        SaveBombCollectionData(block);
        if (ani) await _onBoomTrigger();
    }

    public async Task Bomb10(Block block, bool ani = true)
    {
        _addBonusScore(10);
        block.match = true; block.boom = true;
        _playSound(ESound.Pising);
        int random = UnityEngine.Random.Range((int)EPangEffect.Blue, (int)EPangEffect.Green + 1);
        _createEffect(_pangEffectList[random], block.rectTransform.anchoredPosition);
        _matcher.ChangeMatchState(block.row - 2, block.col - 2); _matcher.ChangeMatchState(block.row - 2, block.col - 1);
        _matcher.ChangeMatchState(block.row - 2, block.col);     _matcher.ChangeMatchState(block.row - 2, block.col + 1);
        _matcher.ChangeMatchState(block.row - 2, block.col + 2); _matcher.ChangeMatchState(block.row - 1, block.col - 2);
        _matcher.ChangeMatchState(block.row - 1, block.col + 2); _matcher.ChangeMatchState(block.row, block.col - 2);
        _matcher.ChangeMatchState(block.row, block.col + 2);     _matcher.ChangeMatchState(block.row + 1, block.col - 2);
        _matcher.ChangeMatchState(block.row + 1, block.col + 2); _matcher.ChangeMatchState(block.row + 2, block.col - 2);
        _matcher.ChangeMatchState(block.row + 2, block.col - 1); _matcher.ChangeMatchState(block.row + 2, block.col);
        _matcher.ChangeMatchState(block.row + 2, block.col + 1); _matcher.ChangeMatchState(block.row + 2, block.col + 2);
        SaveBombCollectionData(block);
        if (ani) await _onBoomTrigger();
    }

    public async Task Bomb11(Block block, bool ani = true)
    {
        _addBonusScore(10);
        block.match = true; block.boom = true;
        _playSound(ESound.Pising);
        int random = UnityEngine.Random.Range((int)EPangEffect.Blue, (int)EPangEffect.Green + 1);
        _createEffect(_pangEffectList[random], block.rectTransform.anchoredPosition);
        _matcher.ChangeMatchState(block.row - 2, block.col - 1); _matcher.ChangeMatchState(block.row - 1, block.col - 2);
        _matcher.ChangeMatchState(block.row - 1, block.col - 1); _matcher.ChangeMatchState(block.row - 2, block.col + 1);
        _matcher.ChangeMatchState(block.row - 1, block.col + 2); _matcher.ChangeMatchState(block.row - 1, block.col + 1);
        _matcher.ChangeMatchState(block.row + 2, block.col - 1); _matcher.ChangeMatchState(block.row + 1, block.col - 2);
        _matcher.ChangeMatchState(block.row + 1, block.col - 1); _matcher.ChangeMatchState(block.row + 2, block.col + 1);
        _matcher.ChangeMatchState(block.row + 1, block.col + 2); _matcher.ChangeMatchState(block.row + 1, block.col + 1);
        SaveBombCollectionData(block);
        if (ani) await _onBoomTrigger();
    }

    public async Task Bomb12(Block block, bool ani = true)
    {
        _addBonusScore(10);
        block.match = true; block.boom = true;
        _playSound(ESound.Pising);
        int random = UnityEngine.Random.Range((int)EPangEffect.Blue, (int)EPangEffect.Green + 1);
        _createEffect(_pangEffectList[random], block.rectTransform.anchoredPosition);
        _matcher.ChangeMatchState(block.row - 2, block.col - 2); _matcher.ChangeMatchState(block.row - 2, block.col - 1);
        _matcher.ChangeMatchState(block.row - 2, block.col);     _matcher.ChangeMatchState(block.row - 2, block.col + 1);
        _matcher.ChangeMatchState(block.row - 2, block.col + 2); _matcher.ChangeMatchState(block.row - 1, block.col + 1);
        _matcher.ChangeMatchState(block.row + 1, block.col - 1); _matcher.ChangeMatchState(block.row + 2, block.col - 2);
        _matcher.ChangeMatchState(block.row + 2, block.col - 1); _matcher.ChangeMatchState(block.row + 2, block.col);
        _matcher.ChangeMatchState(block.row + 2, block.col + 1); _matcher.ChangeMatchState(block.row + 2, block.col + 2);
        SaveBombCollectionData(block);
        if (ani) await _onBoomTrigger();
    }

    public async Task RainbowPang(Block block, bool ani = true)
    {
        if (block.GetHp() > 0) return;
        _addBonusScore(10);
        block.match = true; block.boom = true;

        for (EBlockState bs = EBlockState.PinkBomb; bs <= EBlockState.BlueBomb; ++bs)
        {
            do
            {
                if (!CheckRainbowTargetBlock()) break;
                int w = UnityEngine.Random.Range(0, _board.boardSize);
                int h = UnityEngine.Random.Range(0, _board.boardSize);
                var b = _board.boardArr[w, h];
                if (!b.remove && !b.IsNormalBlock()) continue;
                if (b.changeBlockState != EBlockState.None) continue;
                b.changeBlockState = bs;
                break;
            } while (true);
        }

        if (ani) await _onBoomTrigger();
    }

    bool CheckRainbowTargetBlock()
    {
        for (int i = 0; i < _board.boardSize; ++i)
            for (int j = 0; j < _board.boardSize; ++j)
                if (_board.boardArr[i, j].remove ||
                    (_board.boardArr[i, j].IsNormalBlock() && _board.boardArr[i, j].changeBlockState == EBlockState.None))
                    return true;
        return false;
    }

    public async Task CreateBombBlock(int moveIndex1, int moveIndex2)
    {
        bool createDelay = false;
        var arr = _board.boardArr;
        int size = _board.boardSize;

        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                var block = arr[i, j];
                if (block == null) continue;
                int row = i, col = j;

                if (block.squareMatch)
                {
                    createDelay = true;
                    _board.CreateNewBlock(block, ELog.CreateBoomBlock, 1, block.GetPangType());
                    block.ResetScore(); block.SetOriginPos();
                }

                if (block.hScore >= 3 && block.vScore >= 3)
                {
                    createDelay = true;
                    if (_arrowPangIndex == 1) _board.CreateNewBlock(block, ELog.CreateBoomBlock, 2, EBlockState.Arrow5);
                    else _board.CreateNewBlock(block, ELog.CreateBoomBlock, 3, EBlockState.Arrow6);
                    block.ResetScore(); block.SetOriginPos();
                    ClearScoreNeighbors(row, col);
                }
            }
        }

        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                var block = arr[i, j];
                if (block == null) continue;

                if (block.hScore > 3)
                {
                    createDelay = true;
                    bool checkMove = false;
                    int tempScore = block.hScore;
                    for (int idx = 0; idx < tempScore; ++idx)
                    {
                        int tc = j + idx;
                        if (!_board.IsValidIndex(i, tc) || arr[i, tc] == null) continue;
                        var tb = arr[i, tc];
                        if (tb.index == moveIndex1 || tb.index == moveIndex2)
                        {
                            checkMove = true;
                            if (_arrowPangIndex == 1) _board.CreateNewBlock(tb, ELog.CreateBoomBlock, 4, EBlockState.Arrow1);
                            else _board.CreateNewBlock(tb, ELog.CreateBoomBlock, 5, EBlockState.Arrow4);
                        }
                        arr[i, tc].ResetScore();
                    }
                    if (!checkMove)
                    {
                        if (_arrowPangIndex == 1) _board.CreateNewBlock(block, ELog.CreateBoomBlock, 6, EBlockState.Arrow1);
                        else _board.CreateNewBlock(block, ELog.CreateBoomBlock, 7, EBlockState.Arrow4);
                    }
                }
                else if (block.vScore > 3)
                {
                    createDelay = true;
                    bool checkMove = false;
                    int tempScore = block.vScore;
                    for (int idx = 0; idx < tempScore; ++idx)
                    {
                        int tr = i + idx;
                        if (!_board.IsValidIndex(tr, j) || arr[tr, j] == null) continue;
                        var tb = arr[tr, j];
                        if (tb.index == moveIndex1 || tb.index == moveIndex2)
                        {
                            checkMove = true;
                            if (_arrowPangIndex == 1) _board.CreateNewBlock(tb, ELog.CreateBoomBlock, 9, EBlockState.Arrow3);
                            else _board.CreateNewBlock(tb, ELog.CreateBoomBlock, 8, EBlockState.Arrow2);
                        }
                        arr[tr, j].ResetScore();
                    }
                    if (!checkMove)
                    {
                        if (_arrowPangIndex == 1) _board.CreateNewBlock(block, ELog.CreateBoomBlock, 11, EBlockState.Arrow3);
                        else _board.CreateNewBlock(block, ELog.CreateBoomBlock, 10, EBlockState.Arrow2);
                    }
                }
            }
        }

        if (createDelay)
            await Task.Delay((int)(_board.delay * _board.delayMillisecond), _token);
    }

    void ClearScoreNeighbors(int row, int col)
    {
        var arr = _board.boardArr;
        int max = 9;
        var dirs = new (int dr, int dc)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
        foreach (var d in dirs)
        {
            for (int idx = 1; idx < max; ++idx)
            {
                int tr = row + d.dr * idx;
                int tc = col + d.dc * idx;
                if (!_board.IsValidIndex(tr, tc) || arr[tr, tc] == null) break;
                if (arr[tr, tc].hScore > 0 || arr[tr, tc].vScore > 0)
                    arr[tr, tc].ResetScore();
                else break;
            }
        }
    }

    public void BlockCreatorBlock(EBlockState creatorBlock, EBlockState changeBlock)
    {
        var arr = _board.boardArr;
        int size = _board.boardSize;
        for (int w = 0; w < size; w++)
        {
            for (int h = 0; h < size; h++)
            {
                var block = arr[w, h];
                if (block == null) continue;
                if (block.GetBlockState() != creatorBlock) continue;
                if (block.GetHp() == 0) continue;

                bool change = false;
                int random = UnityEngine.Random.Range(0, 4);
                int tempW = w, tempH = h;

                do
                {
                    switch (random)
                    {
                        case 0: { tempW -= 1; var b = _board.IsValidIndex(tempW, tempH) ? arr[tempW, tempH] : null; if (b != null && b.IsNormalBlock()) { block.Damage(); change = true; b.changeHp = 1; b.changeBlockState = changeBlock; b.checkHp = false; } } break;
                        case 1: { tempW += 1; var b = _board.IsValidIndex(tempW, tempH) ? arr[tempW, tempH] : null; if (b != null && b.IsNormalBlock()) { block.Damage(); change = true; b.changeHp = 1; b.changeBlockState = changeBlock; b.checkHp = false; } } break;
                        case 2: { tempH -= 1; var b = _board.IsValidIndex(tempW, tempH) ? arr[tempW, tempH] : null; if (b != null && b.IsNormalBlock()) { block.Damage(); change = true; b.changeHp = 1; b.changeBlockState = changeBlock; b.checkHp = false; } } break;
                        case 3: { tempH += 1; var b = _board.IsValidIndex(tempW, tempH) ? arr[tempW, tempH] : null; if (b != null && b.IsNormalBlock()) { block.Damage(); change = true; b.changeHp = 1; b.changeBlockState = changeBlock; b.checkHp = false; } } break;
                    }
                    if (tempW < 0 || tempW > size || tempH < 0 || tempH > size) break;
                } while (!change);
            }
        }
    }
}
