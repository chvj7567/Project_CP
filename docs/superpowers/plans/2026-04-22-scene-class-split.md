# Scene Class Split Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `Game.cs`(~3000줄)와 `First.cs`(~457줄)를 목적별 순수 C# 클래스로 분리하고 배경 루프를 제거한다.

**Architecture:** `GPGameScene`/`LBLobbyScene`이 MonoBehaviour 진입점으로 남고, 각 책임은 순수 C# 클래스(`GPBoard`, `GPMatchChecker`, `GPBombResolver`, `GPTutorial`, `GPBossController`, `LBLoginHandler`, `LBTutorial`)에 위임한다. `Block.cs`는 `Game` → `GPGameScene` 타입 참조만 변경한다.

**Tech Stack:** Unity 6, C#, UniRx, DOTween, async/await

---

## File Map

| 작업 | 경로 |
|------|------|
| 수정 | `Assets/Scripts/Block.cs` |
| 생성 | `Assets/Scripts/GamePlay/GPBoard.cs` |
| 생성 | `Assets/Scripts/GamePlay/GPMatchChecker.cs` |
| 생성 | `Assets/Scripts/GamePlay/GPBombResolver.cs` |
| 생성 | `Assets/Scripts/GamePlay/GPTutorial.cs` |
| 생성 | `Assets/Scripts/GamePlay/GPBossController.cs` |
| 생성→교체 | `Assets/Scripts/Scenes/GPGameScene.cs` (Game.cs 대체) |
| 생성 | `Assets/Scripts/Lobby/LBLoginHandler.cs` |
| 생성 | `Assets/Scripts/Lobby/LBTutorial.cs` |
| 생성→교체 | `Assets/Scripts/Scenes/LBLobbyScene.cs` (First.cs 대체) |
| 삭제 | `Assets/Scripts/Scenes/Game.cs` |
| 삭제 | `Assets/Scripts/Scenes/First.cs` |

---

### Task 1: 폴더 생성 + Block.cs 타입 수정

**Files:**
- Modify: `Assets/Scripts/Block.cs:14`

- [ ] **Step 1: GamePlay, Lobby 폴더 생성**

```bash
mkdir -p "Assets/Scripts/GamePlay"
mkdir -p "Assets/Scripts/Lobby"
```

- [ ] **Step 2: Block.cs의 Game → GPGameScene 타입 변경**

`Assets/Scripts/Block.cs` 14번째 줄:
```csharp
// 변경 전
[SerializeField]
Game game;

// 변경 후
[SerializeField]
GPGameScene game;
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/Block.cs
git commit -m "refactor: Block.cs Game → GPGameScene 타입 참조 변경"
```

---

### Task 2: GPBoard 생성

**Files:**
- Create: `Assets/Scripts/GamePlay/GPBoard.cs`

- [ ] **Step 1: GPBoard.cs 생성**

```csharp
using DG.Tweening;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static Defines;

public class GPBoard
{
    public Block[,] boardArr { get; private set; }
    public int boardSize { get; private set; }

    List<Sprite> _blockSpriteList;
    public float delay;
    public int delayMillisecond;
    CancellationToken _token;

    public void Init(Block[,] arr, int size, List<Sprite> sprites, float delay, int delayMs, CancellationToken token)
    {
        boardArr = arr;
        boardSize = size;
        _blockSpriteList = sprites;
        this.delay = delay;
        delayMillisecond = delayMs;
        _token = token;
    }

    public bool IsValidIndex(int row, int col)
    {
        return row >= 0 && row < boardSize && col >= 0 && col < boardSize;
    }

    public bool IsNormalBlock(int row, int col)
    {
        if (!IsValidIndex(row, col) || boardArr[row, col] == null ||
            boardArr[row, col].IsFixdBlock() || boardArr[row, col].IsBombBlock() ||
            boardArr[row, col].IsSpecialBombBlock() || boardArr[row, col].IsFishBlock() ||
            boardArr[row, col].IsBallBlock())
            return false;
        return true;
    }

    public void ChangeBlock(Block moveBlock, Block targetBlock)
    {
        var tempPos = moveBlock.originPos;
        moveBlock.originPos = targetBlock.originPos;
        targetBlock.originPos = tempPos;

        var tempIndex = moveBlock.index;
        moveBlock.index = targetBlock.index;
        targetBlock.index = tempIndex;

        var tempRow = moveBlock.row;
        moveBlock.row = targetBlock.row;
        targetBlock.row = tempRow;

        var tempCol = moveBlock.col;
        moveBlock.col = targetBlock.col;
        targetBlock.col = tempCol;

        moveBlock.name = $"Block{moveBlock.row}/{moveBlock.col}";
        targetBlock.name = $"Block{targetBlock.row}/{targetBlock.col}";

        boardArr[moveBlock.row, moveBlock.col] = moveBlock;
        boardArr[targetBlock.row, targetBlock.col] = targetBlock;
    }

    public void CreateNewBlock(Block block, ELog log, int key, EBlockState blockState, bool isDelay = true)
    {
        blockState = block.CheckSelectCatShop(blockState);
        block.SetBlockState(log, key, _blockSpriteList[(int)blockState], blockState);
        block.match = false;
        block.boom = false;
        block.squareMatch = false;
        block.remove = false;
        if (isDelay)
            block.rectTransform.DOScale(1f, delay);
        else
            block.rectTransform.localScale = Vector3.one;
    }

    public async Task DownBlock()
    {
        var order = new System.Collections.Generic.List<Block>();
        for (int i = boardSize - 1; i >= 0; --i)
            for (int j = boardSize - 1; j >= 0; --j)
                if (boardArr[i, j] != null)
                    order.Add(boardArr[i, j]);

        foreach (var block in order)
        {
            int row = block.row;
            int col = block.col;

            if (boardArr[row, col].IsMatch() || boardArr[row, col].IsFixdBlock())
                continue;

            Block moveBlock = boardArr[row, col];
            Block targetBlock = null;

            int wallRow = -1;
            for (int i = boardSize - 1; i > row; --i)
                if (boardArr[i, col].IsWallBlock())
                    wallRow = i;

            int searchFrom = wallRow == -1 ? boardSize - 1 : wallRow;
            for (int i = searchFrom; i > row; --i)
            {
                if (boardArr[i, col].IsMatch())
                {
                    targetBlock = boardArr[i, col];
                    break;
                }
            }

            if (targetBlock != null)
                ChangeBlock(moveBlock, targetBlock);
        }

        bool downDelay = false;
        foreach (var block in boardArr)
        {
            if (block == null) continue;
            if (!block.IsFixdBlock())
            {
                downDelay = true;
                block.rectTransform.DOAnchorPos(block.originPos, delay);
            }
        }

        if (downDelay)
            await Task.Delay((int)(delay * delayMillisecond), _token);
    }
}
```

- [ ] **Step 2: Unity Editor에서 컴파일 오류 없음 확인**

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/GamePlay/GPBoard.cs
git commit -m "refactor: GPBoard 추출 - 보드 배열 소유 및 기본 조작"
```

---

### Task 3: GPMatchChecker 생성

**Files:**
- Create: `Assets/Scripts/GamePlay/GPMatchChecker.cs`

- [ ] **Step 1: GPMatchChecker.cs 생성**

```csharp
using System.Collections.Generic;
using UnityEngine;
using static Defines;

public class GPMatchChecker
{
    public bool isMatch;
    public int canMatchRow = -1;
    public int canMatchCol = -1;
    public EDrag canMatchDrag = EDrag.None;

    int _moveIndex1;
    int _moveIndex2;
    int _blockTypeCount;
    GPBoard _board;

    public void Init(GPBoard board, int blockTypeCount)
    {
        _board = board;
        _blockTypeCount = blockTypeCount;
    }

    public void SetBlockTypeCount(int count) => _blockTypeCount = count;

    public void SetMoveIndices(int idx1, int idx2)
    {
        _moveIndex1 = idx1;
        _moveIndex2 = idx2;
    }

    public void CheckMap(bool test = false)
    {
        var arr = _board.boardArr;
        int size = _board.boardSize;

        if (!test)
        {
            for (int i = 0; i < size; ++i)
                for (int j = 0; j < size; ++j)
                    arr[i, j].ResetCheckWallDamage();
        }

        for (int i = 0; i < size; ++i)
            for (int j = 0; j < size; ++j)
                CheckSquareMatch(i, j, test);

        for (int i = 0; i < size; ++i)
        {
            var hList = new List<Block>();
            foreach (var block in arr)
                if (block != null && block.row == i)
                    hList.Add(block);
            Check3Match(hList, EDirection.Horizontal, test);
        }

        for (int i = 0; i < size; ++i)
        {
            var vList = new List<Block>();
            foreach (var block in arr)
                if (block != null && block.col == i)
                    vList.Add(block);
            Check3Match(vList, EDirection.Vertical, test);
        }
    }

    public bool CheckSquareMatch(int row, int col, bool test = false)
    {
        var arr = _board.boardArr;

        if (!_board.IsNormalBlock(row, col) || !_board.IsNormalBlock(row + 1, col) ||
            !_board.IsNormalBlock(row, col + 1) || !_board.IsNormalBlock(row + 1, col + 1))
            return false;

        if (arr[row, col].IsMatch() || arr[row + 1, col].IsMatch() ||
            arr[row, col + 1].IsMatch() || arr[row + 1, col + 1].IsMatch())
            return false;

        EBlockState t = arr[row, col].GetBlockState();
        if (t != arr[row + 1, col].GetBlockState()) return false;
        if (t != arr[row, col + 1].GetBlockState()) return false;
        if (t != arr[row + 1, col + 1].GetBlockState()) return false;

        isMatch = true;

        if (!test)
        {
            arr[row, col].match = true;
            arr[row + 1, col].match = true;
            arr[row, col + 1].match = true;
            arr[row + 1, col + 1].match = true;

            if (_moveIndex1 == arr[row, col].index || _moveIndex2 == arr[row, col].index)
                arr[row, col].squareMatch = true;
            else if (_moveIndex1 == arr[row + 1, col].index || _moveIndex2 == arr[row + 1, col].index)
                arr[row + 1, col].squareMatch = true;
            else if (_moveIndex1 == arr[row, col + 1].index || _moveIndex2 == arr[row, col + 1].index)
                arr[row, col + 1].squareMatch = true;
            else if (_moveIndex1 == arr[row + 1, col + 1].index || _moveIndex2 == arr[row + 1, col + 1].index)
                arr[row + 1, col + 1].squareMatch = true;
            else
                arr[row, col].squareMatch = true;
        }

        return true;
    }

    public void Check3Match(List<Block> blockList, EDirection direction, bool test = false)
    {
        EBlockState blockState = EBlockState.None;
        int matchCount = 0;

        for (int i = 0; i < blockList.Count; ++i)
        {
            var b = blockList[i];
            if (!b.IsNormalBlock() || b.IsFixdBlock() || b.IsBombBlock() || b.IsFishBlock())
            {
                blockState = EBlockState.None;
                matchCount = 0;
                continue;
            }

            if (blockState == EBlockState.None)
            {
                blockState = b.GetBlockState();
                matchCount = 1;
            }
            else if (blockState == b.GetBlockState())
            {
                ++matchCount;
                if (matchCount >= 3)
                {
                    if (!test)
                    {
                        int temp = i;
                        for (int j = 0; j < matchCount; ++j)
                        {
                            var bl = blockList[temp--];
                            bl.SetScore(matchCount, direction);
                            bl.match = true;
                        }
                    }
                    isMatch = true;
                }
            }
            else
            {
                blockState = b.GetBlockState();
                matchCount = 1;
            }
        }
    }

    public bool CanPlay()
    {
        isMatch = false;
        int max = 9;
        var arr = _board.boardArr;

        for (int i = 0; i < max; ++i)
        {
            for (int j = 0; j < max; ++j)
            {
                if (arr[i, j] == null) continue;
                var cur = arr[i, j];
                if (cur.CanNotDragBlock()) continue;

                var canDrag = CanDragBlock(cur);
                if (cur.GetBlockState() == EBlockState.PinkBomb && canDrag != EDrag.None)
                {
                    canMatchRow = cur.row; canMatchCol = cur.col; canMatchDrag = canDrag;
                    return true;
                }

                if (cur.IsBombBlock() || cur.IsSpecialBombBlock())
                {
                    canMatchRow = cur.row; canMatchCol = cur.col; canMatchDrag = EDrag.Click;
                    return true;
                }

                var up    = _board.IsValidIndex(i - 1, j) ? arr[i - 1, j] : null;
                var down  = _board.IsValidIndex(i + 1, j) ? arr[i + 1, j] : null;
                var left  = _board.IsValidIndex(i, j - 1) ? arr[i, j - 1] : null;
                var right = _board.IsValidIndex(i, j + 1) ? arr[i, j + 1] : null;

                if (up != null && !up.CanNotDragBlock())
                {
                    _board.ChangeBlock(cur, up); CheckMap(true); _board.ChangeBlock(cur, up);
                    if (isMatch) { canMatchRow = cur.row; canMatchCol = cur.col; canMatchDrag = EDrag.Up; return true; }
                }
                if (down != null && !down.CanNotDragBlock())
                {
                    _board.ChangeBlock(cur, down); CheckMap(true); _board.ChangeBlock(cur, down);
                    if (isMatch) { canMatchRow = cur.row; canMatchCol = cur.col; canMatchDrag = EDrag.Down; return true; }
                }
                if (left != null && !left.CanNotDragBlock())
                {
                    _board.ChangeBlock(cur, left); CheckMap(true); _board.ChangeBlock(cur, left);
                    if (isMatch) { canMatchRow = cur.row; canMatchCol = cur.col; canMatchDrag = EDrag.Left; return true; }
                }
                if (right != null && !right.CanNotDragBlock())
                {
                    _board.ChangeBlock(cur, right); CheckMap(true); _board.ChangeBlock(cur, right);
                    if (isMatch) { canMatchRow = cur.row; canMatchCol = cur.col; canMatchDrag = EDrag.Right; return true; }
                }
            }
        }
        return false;
    }

    public EDrag CanDragBlock(Block block)
    {
        int w = block.row, h = block.col;
        if (_board.IsValidIndex(w - 1, h) && !_board.boardArr[w - 1, h].CanNotDragBlock()) return EDrag.Up;
        if (_board.IsValidIndex(w + 1, h) && !_board.boardArr[w + 1, h].CanNotDragBlock()) return EDrag.Down;
        if (_board.IsValidIndex(w, h - 1) && !_board.boardArr[w, h - 1].CanNotDragBlock()) return EDrag.Left;
        if (_board.IsValidIndex(w, h + 1) && !_board.boardArr[w, h + 1].CanNotDragBlock()) return EDrag.Right;
        return EDrag.None;
    }

    public void CheckArround(int row, int col)
    {
        if (!_board.IsValidIndex(row, col) || _board.boardArr[row, col] == null) return;
        DamageBlock(row - 1, col);
        DamageBlock(row, col + 1);
        DamageBlock(row, col - 1);
        DamageBlock(row + 1, col);
    }

    public void DamageBlock(int row, int col)
    {
        if (_board.IsValidIndex(row, col) && _board.boardArr[row, col] != null)
        {
            if (_board.boardArr[row, col].GetBlockState() == EBlockState.RainbowPang)
                _board.boardArr[row, col].Damage(_blockTypeCount, false);
            else
                _board.boardArr[row, col].Damage(_blockTypeCount);
        }
    }

    public bool ChangeMatchState(int row, int col)
    {
        if (!_board.IsValidIndex(row, col) || _board.boardArr[row, col] == null) return false;
        DamageBlock(row, col);

        var b = _board.boardArr[row, col];
        if (b.GetBlockState() == EBlockState.RainbowPang && b.GetHp() > 0) return false;
        if (!b.IsFishBlock() && !b.IsFixdBlock() && b.GetBlockState() != EBlockState.PinkBomb)
        {
            b.match = true;
            return true;
        }
        return false;
    }
}
```

- [ ] **Step 2: Unity Editor 컴파일 확인**

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/GamePlay/GPMatchChecker.cs
git commit -m "refactor: GPMatchChecker 추출 - 매치 감지 및 데미지 로직"
```

---

### Task 4: GPBombResolver 생성

**Files:**
- Create: `Assets/Scripts/GamePlay/GPBombResolver.cs`

- [ ] **Step 1: GPBombResolver.cs 생성**

```csharp
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
        foreach (var go in blueHoleList) CHMMain.Resource.Destroy(go);
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
        int[] dr = { 1, -1, 0, 0 };
        int[] dc = { 0, 0, 1, -1 };
        foreach (var d in new[] { (dr[0], dc[0]), (dr[1], dc[1]), (dr[2], dc[2]), (dr[3], dc[3]) })
        {
            for (int idx = 1; idx < max; ++idx)
            {
                int tr = row + d.Item1 * idx;
                int tc = col + d.Item2 * idx;
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
```

- [ ] **Step 2: Unity Editor 컴파일 확인**

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/GamePlay/GPBombResolver.cs
git commit -m "refactor: GPBombResolver 추출 - 폭탄 패턴 12종 및 특수 폭탄 로직"
```

---

### Task 5: GPTutorial 생성

**Files:**
- Create: `Assets/Scripts/GamePlay/GPTutorial.cs`

- [ ] **Step 1: GPTutorial.cs 생성**

```csharp
using DG.Tweening;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static Defines;
using static Infomation;

public class GPTutorial
{
    GPBoard _board;
    RectTransform _guideFinger;
    RectTransform _guideHole;
    GameObject _guideBackground;
    Button _guideBackgroundBtn;
    List<RectTransform> _normalGuideHoleList;
    List<RectTransform> _bossGuideHoleList;
    CHTMPro _guideDesc;
    bool _guideEnd;

    public void Init(
        GPBoard board,
        RectTransform guideFinger,
        RectTransform guideHole,
        GameObject guideBackground,
        Button guideBackgroundBtn,
        List<RectTransform> normalGuideHoleList,
        List<RectTransform> bossGuideHoleList,
        CHTMPro guideDesc)
    {
        _board = board;
        _guideFinger = guideFinger;
        _guideHole = guideHole;
        _guideBackground = guideBackground;
        _guideBackgroundBtn = guideBackgroundBtn;
        _normalGuideHoleList = normalGuideHoleList;
        _bossGuideHoleList = bossGuideHoleList;
        _guideDesc = guideDesc;
    }

    public async Task StartGuide(ESelectStage selectStage, Data.Login loginData)
    {
        if (selectStage == ESelectStage.Normal &&
            loginData.guideIndex == (int)CHMMain.Json.GetConstValueInfo(EConstValue.NormalStageGuideMaxIndex))
        {
            Time.timeScale = 0;
            _guideBackground.SetActive(true);
            _guideBackground.transform.SetAsLastSibling();
            _guideBackgroundBtn.gameObject.SetActive(true);
            _guideBackgroundBtn.transform.SetAsLastSibling();

            loginData.guideIndex += await NormalStageGuideStart();

            _guideBackground.SetActive(false);
            _guideBackgroundBtn.gameObject.SetActive(false);
            Time.timeScale = 1;
            CHMData.Instance.SaveData(CHMMain.String.CatPang);
        }

        if (selectStage == ESelectStage.Boss &&
            loginData.guideIndex == (int)CHMMain.Json.GetConstValueInfo(EConstValue.BossStageGuideMaxIndex))
        {
            Time.timeScale = 0;
            _guideBackground.SetActive(true);
            _guideBackground.transform.SetAsLastSibling();
            _guideBackgroundBtn.gameObject.SetActive(true);
            _guideBackgroundBtn.transform.SetAsLastSibling();

            loginData.guideIndex += await BossStageGuideStart();

            _guideBackground.SetActive(false);
            _guideBackgroundBtn.gameObject.SetActive(false);
            Time.timeScale = 1;
            CHMData.Instance.SaveData(CHMMain.String.CatPang);
        }
    }

    public void StartTutorial(StageInfo stageInfo, List<StageBlockInfo> stageBlockInfoList, ESelectStage selectStage)
    {
        if (selectStage == ESelectStage.Hard || stageInfo.tutorialID <= 0) return;

        _guideEnd = false;
        _guideFinger.gameObject.SetActive(true);
        Time.timeScale = 0;
        _guideBackground.SetActive(true);
        _guideHole.gameObject.SetActive(true);
        _guideHole.SetAsLastSibling();
        _guideBackground.transform.SetAsLastSibling();
        _guideFinger.transform.SetAsLastSibling();

        var holeValue = GetTutorialStageImgSettingValue(stageBlockInfoList);
        _guideHole.sizeDelta = holeValue.Item1;
        _guideHole.anchoredPosition = holeValue.Item2;

        var tutorialInfo = CHMMain.Json.GetTutorialInfo(stageInfo.tutorialID);
        if (tutorialInfo != null)
            _guideDesc.SetStringID(tutorialInfo.descStringID);
    }

    public void HideGuide()
    {
        _guideBackground.SetActive(false);
        _guideHole.gameObject.SetActive(false);
        _guideFinger.gameObject.SetActive(false);
        _guideEnd = true;
    }

    public bool CheckTutorial()
    {
        for (int w = 0; w < _board.boardSize; w++)
            for (int h = 0; h < _board.boardSize; h++)
                if (_board.boardArr[w, h].tutorialBlock)
                    return true;
        return false;
    }

    public (Vector2, Vector2) TutorialBlockSetting(EBlockState blockState)
    {
        var arr = _board.boardArr;
        for (int w = 0; w < _board.boardSize; w++)
            for (int h = 0; h < _board.boardSize; h++)
                if (arr[w, h].GetBlockState() == blockState)
                {
                    arr[w, h].tutorialBlock = true;
                    return (arr[w, h].rectTransform.sizeDelta, arr[w, h].rectTransform.anchoredPosition);
                }
        return (Vector2.zero, Vector2.zero);
    }

    async Task<int> NormalStageGuideStart()
    {
        _guideBackground.SetActive(true);
        for (int i = 0; i < _normalGuideHoleList.Count; ++i)
        {
            var guideInfo = CHMMain.Json.GetGuideInfo(i + 1 + (int)CHMMain.Json.GetConstValueInfo(EConstValue.NormalStageGuideMaxIndex));
            if (guideInfo == null) break;
            _normalGuideHoleList[i].gameObject.SetActive(true);
            _guideDesc.SetStringID(guideInfo.descStringID);

            var buttonTask = new TaskCompletionSource<bool>();
            var sub = _guideBackgroundBtn.OnClickAsObservable().Subscribe(_ => buttonTask.SetResult(true));
            await buttonTask.Task;
            _normalGuideHoleList[i].gameObject.SetActive(false);
            sub.Dispose();
        }
        return _normalGuideHoleList.Count;
    }

    async Task<int> BossStageGuideStart()
    {
        _guideBackground.SetActive(true);
        for (int i = 0; i < _bossGuideHoleList.Count; ++i)
        {
            var guideInfo = CHMMain.Json.GetGuideInfo(i + 1 + (int)CHMMain.Json.GetConstValueInfo(EConstValue.BossStageGuideMaxIndex));
            if (guideInfo == null) break;
            _bossGuideHoleList[i].gameObject.SetActive(true);
            _guideDesc.SetStringID(guideInfo.descStringID);

            var buttonTask = new TaskCompletionSource<bool>();
            var sub = _guideBackgroundBtn.OnClickAsObservable().Subscribe(_ => buttonTask.SetResult(true));
            await buttonTask.Task;
            _bossGuideHoleList[i].gameObject.SetActive(false);
            sub.Dispose();
        }
        return _bossGuideHoleList.Count;
    }

    (Vector2, Vector2) GetTutorialStageImgSettingValue(List<StageBlockInfo> stageBlockInfoList)
    {
        if (stageBlockInfoList == null) return (Vector2.zero, Vector2.zero);
        var tutorialBlocks = stageBlockInfoList.FindAll(_ => _.tutorialBlock);
        if (tutorialBlocks.Count <= 0) return (Vector2.zero, Vector2.zero);

        var arr = _board.boardArr;
        float sizeX, sizeY, posX, posY;

        if (tutorialBlocks.Count == 1)
        {
            var b = arr[tutorialBlocks[0].row, tutorialBlocks[0].col];
            sizeX = b.rectTransform.sizeDelta.x;
            sizeY = b.rectTransform.sizeDelta.y;
            posX = b.rectTransform.anchoredPosition.x;
            posY = b.rectTransform.anchoredPosition.y;
            _guideFinger.anchoredPosition = new Vector2(posX, posY);
        }
        else
        {
            var b1 = arr[tutorialBlocks[0].row, tutorialBlocks[0].col];
            var b2 = arr[tutorialBlocks[1].row, tutorialBlocks[1].col];
            if (b1.row == b2.row)
            {
                sizeX = b1.rectTransform.sizeDelta.x * 2;
                sizeY = b1.rectTransform.sizeDelta.y;
                posX = (b1.rectTransform.anchoredPosition.x + b2.rectTransform.anchoredPosition.x) / 2f;
                posY = b1.rectTransform.anchoredPosition.y;
            }
            else
            {
                sizeX = b1.rectTransform.sizeDelta.x;
                sizeY = b1.rectTransform.sizeDelta.y * 2;
                posX = b1.rectTransform.anchoredPosition.x;
                posY = (b1.rectTransform.anchoredPosition.y + b2.rectTransform.anchoredPosition.y) / 2f;
            }
            FingerMoveRepeat(b1.rectTransform.anchoredPosition, b2.rectTransform.anchoredPosition);
        }
        return (new Vector2(sizeX, sizeY), new Vector2(posX, posY));
    }

    async Task FingerMoveRepeat(Vector2 startPos, Vector2 endPos)
    {
        float moveTime = 2f;
        while (!_guideEnd)
        {
            _guideFinger.anchoredPosition = startPos;
            float elapsed = 0f;
            while (elapsed < moveTime)
            {
                _guideFinger.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsed / moveTime);
                elapsed += 0.025f;
                await Task.Yield();
            }
            await Task.Delay(1000);
        }
    }
}
```

- [ ] **Step 2: Unity Editor 컴파일 확인**

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/GamePlay/GPTutorial.cs
git commit -m "refactor: GPTutorial 추출 - 튜토리얼 및 가이드 UI 흐름"
```

---

### Task 6: GPBossController 생성

**Files:**
- Create: `Assets/Scripts/GamePlay/GPBossController.cs`

- [ ] **Step 1: GPBossController.cs 생성**

```csharp
using System;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using static Defines;
using static Infomation;

public class GPBossController
{
    public ReactiveProperty<int> hp = new ReactiveProperty<int>();

    GPBoard _board;
    StageInfo _stageInfo;
    Image _bossHpImage;
    CHTMPro _bossHpText;
    CHTMPro _hpText;
    GameObject _normalBossObj;
    GameObject _angryBossObj;
    GameObject _cryBossObj;
    bool _bossSkill;

    public void Init(
        GPBoard board,
        StageInfo stageInfo,
        Data.Login loginData,
        Image bossHpImage,
        CHTMPro bossHpText,
        CHTMPro hpText,
        GameObject normalBossObj,
        GameObject angryBossObj,
        GameObject cryBossObj,
        ReactiveProperty<int> curScore,
        MonoBehaviour owner)
    {
        _board = board;
        _stageInfo = stageInfo;
        _bossHpImage = bossHpImage;
        _bossHpText = bossHpText;
        _hpText = hpText;
        _normalBossObj = normalBossObj;
        _angryBossObj = angryBossObj;
        _cryBossObj = cryBossObj;

        hp.Subscribe(_ => { if (_ >= 0) _hpText.SetText(hp); });
        hp.Value = loginData.hp;

        Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .Subscribe(_ => hp.Value -= 1)
            .AddTo(owner);

        curScore.Subscribe(_ =>
        {
            var fillAmount = (_stageInfo.targetScore - _) / (float)_stageInfo.targetScore;
            _bossHpImage.DOFillAmount(fillAmount, .5f);
            var bossHp = Mathf.Max(0, _stageInfo.targetScore - _);
            _bossHpText.SetText(bossHp);

            if (!_bossSkill && fillAmount <= .5f)
            {
                _bossSkill = true;
                _normalBossObj.SetActive(false);
                _angryBossObj.SetActive(true);

                CHMMain.UI.ShowUI(EUI.UIAlarm, new UIAlarmArg { stringID = 78 });

                int coolTime;
                int mod = _stageInfo.stage % 10;
                if (mod == 0) { coolTime = 10; Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(coolTime)).Subscribe(_ => { BossSkill(1); BossSkill(2); BossSkill(3); }).AddTo(owner); }
                else if (mod >= 6) { coolTime = 10 - mod + 10; Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(coolTime)).Subscribe(_ => { BossSkill(1); BossSkill(2); }).AddTo(owner); }
                else { coolTime = 10 - mod + 10; Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(coolTime)).Subscribe(_ => BossSkill(1)).AddTo(owner); }
            }
        });
    }

    public void OnClear()
    {
        _normalBossObj.SetActive(false);
        _angryBossObj.SetActive(false);
        _cryBossObj.SetActive(true);
    }

    public void BossSkill(int type)
    {
        var blockHp = UnityEngine.Random.Range(0, 10);
        if (blockHp == 0) blockHp = -1;

        int w, h;
        do
        {
            w = UnityEngine.Random.Range(0, _board.boardSize);
            h = UnityEngine.Random.Range(0, _board.boardSize);
        } while (!_board.boardArr[w, h].IsNormalBlock());

        EBlockState block;
        if (type == 1)
            block = (EBlockState)UnityEngine.Random.Range((int)EBlockState.Wall, (int)EBlockState.Potal + 1);
        else if (type == 2)
            block = (EBlockState)UnityEngine.Random.Range((int)EBlockState.WallCreator, (int)EBlockState.PotalCreator + 1);
        else
            block = (EBlockState)UnityEngine.Random.Range((int)EBlockState.CatBox1, (int)EBlockState.CatBox5 + 1);

        _board.boardArr[w, h].changeBlockState = block;
        _board.boardArr[w, h].changeHp = blockHp;
    }
}
```

- [ ] **Step 2: Unity Editor 컴파일 확인**

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/GamePlay/GPBossController.cs
git commit -m "refactor: GPBossController 추출 - 보스 HP 및 스킬 로직"
```

---

### Task 7: GPGameScene 생성 (Game.cs 교체)

**Files:**
- Create: `Assets/Scripts/Scenes/GPGameScene.cs`
- Delete: `Assets/Scripts/Scenes/Game.cs`

- [ ] **Step 1: GPGameScene.cs 생성**

```csharp
using DG.Tweening;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Defines;
using static Infomation;

public class GPGameScene : MonoBehaviour
{
    const int MAX = 9;

    [Header("뒤로 가기")]
    [SerializeField] Button backBtn;

    [Header("타이머")]
    [SerializeField] Image timerImg;
    [SerializeField] CHTMPro timerText;
    [SerializeField, ReadOnly] float curTimer;

    [Header("점수 골드 및 이미지")]
    [SerializeField] Image goldImg;
    [SerializeField] List<Image> catFootImgList = new List<Image>();

    [Header("보드")]
    [SerializeField] Transform parent;
    [SerializeField] CHInstantiateButton instBtn;
    [SerializeField] GameObject origin;
    [SerializeField] float margin = 0f;

    [Header("폭탄 이펙트")]
    [SerializeField] ParticleSystem bombEffectPS;
    [SerializeField] List<ParticleSystem> pangEffectList = new List<ParticleSystem>();

    [Header("게임 속도")]
    [SerializeField] public float delay;
    [SerializeField] int delayMillisecond;

    [Header("게임 상태")]
    [SerializeField, ReadOnly] Block[,] boardArr = new Block[MAX, MAX];
    [SerializeField, ReadOnly] public bool isDrag = false;
    [SerializeField, ReadOnly] public bool isLock = false;
    [SerializeField, ReadOnly] bool isMatch = false;
    [SerializeField, ReadOnly] bool oneTimeAlarm = false;
    [SerializeField, ReadOnly] int moveIndex1 = 0;
    [SerializeField, ReadOnly] int moveIndex2 = 0;
    [SerializeField, ReadOnly] int boardSize = 1;
    [SerializeField, ReadOnly] public ReactiveProperty<EGameState> gameResult = new ReactiveProperty<EGameState>();
    [SerializeField, ReadOnly] public bool gameEnd = false;

    [SerializeField] CHTMPro targetScoreText;
    [SerializeField] CHTMPro moveCountText;
    [SerializeField] CHTMPro curScoreText;
    [SerializeField] CHTMPro bonusScoreText;
    [SerializeField, ReadOnly] ReactiveProperty<int> curScore = new ReactiveProperty<int>();
    [SerializeField, ReadOnly] ReactiveProperty<int> bonusScore = new ReactiveProperty<int>();
    [SerializeField, ReadOnly] ReactiveProperty<int> moveCount = new ReactiveProperty<int>();

    [Header("자동 플레이")]
    [SerializeField] bool autoPlay = false;
    [SerializeField, ReadOnly] int updateMapCount = 5;
    [SerializeField, ReadOnly] float teachTime;
    [SerializeField, ReadOnly] float dragTime;

    [Header("보스 스테이지")]
    [SerializeField] GameObject normalBossObj;
    [SerializeField] GameObject angryBossObj;
    [SerializeField] GameObject cryBossObj;
    [SerializeField] Image bossHpImage;
    [SerializeField] CHTMPro bossHpText;
    [SerializeField] CHTMPro hpText;

    [Header("스테이지별 UI 오브젝트")]
    [SerializeField] GameObject onlyNormalStageObject;
    [SerializeField] GameObject onlyBossStageObject;

    [Header("폭탄 선택 UI")]
    [SerializeField, ReadOnly] int arrowPangIndex = 1;
    [SerializeField] CHButton arrowPang1;
    [SerializeField] CHButton arrowPang2;
    [SerializeField] Image banView;

    [Header("가이드")]
    [SerializeField] bool guideEnd = false;
    [SerializeField] RectTransform guideFinger;
    [SerializeField] RectTransform guideHole;
    [SerializeField] GameObject guideBackground;
    [SerializeField] Button guideBackgroundBtn;
    [SerializeField] List<RectTransform> normalStageGuideHoleList = new List<RectTransform>();
    [SerializeField] List<RectTransform> bossStageGuideHoleList = new List<RectTransform>();
    [SerializeField] CHTMPro guideDesc;

    List<Sprite> _blockSpriteList = new List<Sprite>();
    StageInfo _stageInfo;
    List<StageBlockInfo> _stageBlockInfoList = new List<StageBlockInfo>();
    ESelectStage _selectStage = ESelectStage.Hard;
    Data.Login _loginData;

    CancellationTokenSource tokenSource;
    float gameTime = 0;
    int helpTime = 0;
    bool tutorialNextBlock = false;
    bool init = false;

    GPBoard _board;
    GPMatchChecker _matcher;
    GPBombResolver _bombResolver;
    GPTutorial _tutorial;
    GPBossController _boss;

    async void Start()
    {
        bonusScoreText.gameObject.SetActive(false);
        guideFinger.gameObject.SetActive(false);
        guideBackground.SetActive(false);
        guideHole.gameObject.SetActive(false);
        onlyNormalStageObject.SetActive(true);
        onlyBossStageObject.SetActive(false);

        foreach (var h in normalStageGuideHoleList) h.gameObject.SetActive(false);
        foreach (var h in bossStageGuideHoleList) h.gameObject.SetActive(false);
        guideBackgroundBtn.gameObject.SetActive(false);

        if (backBtn)
        {
            backBtn.OnClickAsObservable().Subscribe(_ =>
            {
                tokenSource?.Cancel();
                Time.timeScale = 1;
                CHInstantiateButton.ResetBlockDict();
                CHMMain.UI.CloseUI(EUI.UIAlarm);
                CHMMain.Pool.Clear();
                SceneManager.LoadScene(1);
            });
        }

        if (arrowPang1 && arrowPang2)
        {
            arrowPang1.button.OnClickAsObservable().Subscribe(_ =>
            {
                arrowPangIndex = 1;
                _bombResolver?.SetArrowPangIndex(1);
                banView.rectTransform.DOAnchorPosX(arrowPang2.rectTransform.anchoredPosition.x, .5f);
            });
            arrowPang2.button.OnClickAsObservable().Subscribe(_ =>
            {
                arrowPangIndex = 2;
                _bombResolver?.SetArrowPangIndex(2);
                banView.rectTransform.DOAnchorPosX(arrowPang1.rectTransform.anchoredPosition.x, .5f);
            });
        }

        curScore.Subscribe(_ => curScoreText.SetText(_));
        moveCount.Subscribe(_ => moveCountText.SetText(_));

        this.UpdateAsObservable().Subscribe(_ =>
        {
            if (gameResult.Value == EGameState.GameClearWait) { GameEnd(true); return; }
            if (gameResult.Value == EGameState.GameOverWait) { GameEnd(false); return; }

            if (gameResult.Value == EGameState.NormalOrHardStagePlay)
            {
                bool clear = true;
                bool useTime = _stageInfo.time > 0;
                bool useTargetScore = _stageInfo.targetScore > 0;
                bool useMoveCount = _stageInfo.moveCount > 0;

                for (int i = 0; i < boardSize; ++i)
                {
                    for (int j = 0; j < boardSize; ++j)
                    {
                        if (boardArr[i, j].GetBlockState() == EBlockState.RainbowPang) continue;
                        if (!boardArr[i, j].checkHp) continue;
                        if (boardArr[i, j].GetHp() > 0 || boardArr[i, j].IsFishBlock() || boardArr[i, j].IsBallBlock())
                        { clear = false; break; }
                    }
                    if (!clear) break;
                }

                if (useTime && timerImg.fillAmount >= 1)
                {
                    if (useTargetScore && curScore.Value < _stageInfo.targetScore) clear = false;
                    GameEnd(clear);
                }
                else
                {
                    if (useTargetScore && curScore.Value < _stageInfo.targetScore) clear = false;
                    if (useMoveCount && moveCount.Value <= 0) GameEnd(clear);
                    else if (clear) GameEnd(clear);
                }
            }
            else if (gameResult.Value == EGameState.BossStagePlay)
            {
                if (_boss.hp.Value <= 0) { GameEnd(bossHpImage.fillAmount <= 0); return; }
                if (bossHpImage.fillAmount <= 0) { GameEnd(true); return; }
            }
        });

        await LoadImage();
        InitData();
        await CreateMap();
        await _tutorial.StartGuide(_selectStage, _loginData);
        _tutorial.StartTutorial(_stageInfo, _stageBlockInfoList, _selectStage);
    }

    private async void Update()
    {
        gameTime += Time.deltaTime;

        if (!isLock)
        {
            curTimer += Time.deltaTime;
            timerImg.fillAmount = curTimer / _stageInfo.time;

            if (curTimer >= helpTime)
            {
                if (_stageInfo.time >= helpTime)
                {
                    timerText.gameObject.SetActive(true);
                    timerText.SetText(_stageInfo.time - helpTime);
                    ++helpTime;
                }
                else timerText.gameObject.SetActive(false);
            }
        }

        if (isLock) { teachTime = gameTime; dragTime = gameTime; }
        else
        {
            if (autoPlay && dragTime + .5f < gameTime)
                boardArr[_matcher.canMatchRow, _matcher.canMatchCol].Drag(_matcher.canMatchDrag);

            if (teachTime + 3 < gameTime && !oneTimeAlarm && _matcher.canMatchRow >= 0 && _matcher.canMatchCol >= 0)
            {
                oneTimeAlarm = true;
                try
                {
                    var block = boardArr[_matcher.canMatchRow, _matcher.canMatchCol];
                    block.transform.DOScale(1.5f, 0.25f).OnComplete(() => block.transform.DOScale(1f, 0.25f));
                    await Task.Delay(3000, tokenSource.Token);
                }
                catch (TaskCanceledException) { }
                oneTimeAlarm = false;
            }
        }
    }

    void OnDestroy() => tokenSource?.Cancel();

    private void OnApplicationQuit() => CHMData.Instance.SaveData(CHMMain.String.CatPang);

    async Task LoadImage()
    {
        for (EBlockState i = 0; i < EBlockState.Max; ++i)
        {
            var t = new TaskCompletionSource<Sprite>();
            CHMMain.Resource.LoadSprite(i, sprite =>
            {
                if (sprite != null) _blockSpriteList.Add(sprite);
                t.SetResult(sprite);
            });
            await t.Task;
        }
    }

    void InitData()
    {
        if (init) return;
        init = true;

        tokenSource = new CancellationTokenSource();
        _loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        _selectStage = (ESelectStage)PlayerPrefs.GetInt(CHMMain.String.SelectStage);

        int stage = 0;
        switch (_selectStage)
        {
            case ESelectStage.Hard:   stage = PlayerPrefs.GetInt(CHMMain.String.HardStage); break;
            case ESelectStage.Boss:   stage = PlayerPrefs.GetInt(CHMMain.String.BossStage); break;
            case ESelectStage.Normal: stage = PlayerPrefs.GetInt(CHMMain.String.NormalStage); break;
        }

        _stageInfo = CHMMain.Json.GetStageInfo(stage);
        _stageBlockInfoList = CHMMain.Json.GetStageBlockInfoList(stage);
        boardSize = _stageInfo.boardSize;

        switch (_selectStage)
        {
            case ESelectStage.Hard:
                _stageInfo.tutorialID = -1;
                foreach (var b in _stageBlockInfoList) b.tutorialBlock = false;
                break;
            case ESelectStage.Normal:
                _stageInfo.time = -1;
                if (_stageInfo.targetScore > 0) _stageInfo.targetScore /= 2;
                else if (_stageInfo.moveCount > 0) _stageInfo.moveCount *= 2;
                break;
        }

        targetScoreText.SetText(_stageInfo.targetScore);
        if (_stageInfo.targetScore < 0) targetScoreText.gameObject.SetActive(false);

        if (_stageInfo.moveCount > 0) moveCount.Value = _stageInfo.moveCount + _loginData.useMoveItemCount;
        else
        {
            moveCount.Value = 99;
            if (_loginData.useMoveItemCount > 0) _loginData.addMoveItemCount += _loginData.useMoveItemCount;
        }

        if (_stageInfo.time > 0) _stageInfo.time += _loginData.useTimeItemCount * 10;
        else if (_loginData.useTimeItemCount > 0) _loginData.addTimeItemCount += _loginData.useTimeItemCount;

        gameResult.Value = _selectStage == ESelectStage.Boss ? EGameState.BossStagePlay : EGameState.NormalOrHardStagePlay;

        // GP 클래스 초기화
        _board = new GPBoard();
        _board.Init(boardArr, boardSize, _blockSpriteList, delay, delayMillisecond, tokenSource.Token);

        _matcher = new GPMatchChecker();
        _matcher.Init(_board, _stageInfo.blockTypeCount);

        _bombResolver = new GPBombResolver();
        _bombResolver.Init(
            _board, _matcher,
            () => AfterDrag(null, null, true),
            score => bonusScore.Value += score,
            CreateEffect,
            sound => CHMMain.Sound.Play(sound),
            pangEffectList, bombEffectPS,
            tokenSource.Token);
        _bombResolver.SetArrowPangIndex(arrowPangIndex);

        _tutorial = new GPTutorial();
        _tutorial.Init(_board, guideFinger, guideHole, guideBackground, guideBackgroundBtn,
            normalStageGuideHoleList, bossStageGuideHoleList, guideDesc);

        if (_selectStage == ESelectStage.Boss)
        {
            onlyBossStageObject.SetActive(true);
            onlyNormalStageObject.SetActive(false);
            normalBossObj.SetActive(true);
            angryBossObj.SetActive(false);
            cryBossObj.SetActive(false);

            _boss = new GPBossController();
            _boss.Init(_board, _stageInfo, _loginData, bossHpImage, bossHpText, hpText,
                normalBossObj, angryBossObj, cryBossObj, curScore, this);
        }
    }

    async Task CreateMap()
    {
        instBtn.InstantiateButton(origin, margin, boardSize, boardSize, parent, boardArr);

        foreach (var block in boardArr)
        {
            if (block == null) continue;
            float moveDis = CHInstantiateButton.GetHorizontalDistance() * (boardSize - 1) / 2;
            block.originPos.x -= moveDis;
            block.SetOriginPos();
            block.rectTransform.DOScale(1f, delay);

            var info = _stageBlockInfoList.Find(_ => _.row == block.row && _.col == block.col);
            if (info == null)
            {
                var random = (EBlockState)UnityEngine.Random.Range(0, _stageInfo.blockTypeCount);
                random = block.CheckSelectCatShop(random);
                block.SetBlockState(ELog.CreateMap, 1, _blockSpriteList[(int)random], random);
                block.checkHp = block.CheckHpBlock();
                block.SetHp(-1);
            }
            else
            {
                var blockState = block.CheckSelectCatShop(info.blockState);
                block.SetBlockState(ELog.CreateMap, 2, _blockSpriteList[(int)blockState], blockState);
                block.checkHp = block.CheckHpBlock();
                block.tutorialBlock = info.tutorialBlock;
                block.SetHp(block.IsNormalBlock() ? -1 : info.hp);
            }
        }

        isMatch = false;
        _matcher.CheckMap();

        bool canMatch = true;
        do
        {
            if (!canMatch)
            {
                foreach (var block in boardArr)
                {
                    if (block == null) continue;
                    var info = _stageBlockInfoList.Find(_ => _.row == block.row && _.col == block.col);
                    if (info == null)
                    {
                        var random = (EBlockState)UnityEngine.Random.Range(0, _stageInfo.blockTypeCount);
                        random = block.CheckSelectCatShop(random);
                        block.SetBlockState(ELog.CreateMap, 3, _blockSpriteList[(int)random], random);
                        block.SetHp(-1);
                    }
                    else
                    {
                        var blockState = block.CheckSelectCatShop(info.blockState);
                        block.SetBlockState(ELog.CreateMap, 4, _blockSpriteList[(int)blockState], blockState);
                        block.tutorialBlock = info.tutorialBlock;
                        block.SetHp(block.IsNormalBlock() ? -1 : info.hp);
                    }
                }
            }

            for (int i = 0; i < boardSize; ++i)
            {
                for (int j = 0; j < boardSize; ++j)
                {
                    var block = boardArr[i, j];
                    if (block == null) continue;
                    if (block.squareMatch || block.IsMatch())
                    {
                        var random = (EBlockState)UnityEngine.Random.Range(0, _stageInfo.blockTypeCount);
                        random = block.CheckSelectCatShop(random);
                        block.SetBlockState(ELog.CreateMap, 5, _blockSpriteList[(int)random], random);
                        block.SetHp(-1); block.ResetScore(); block.match = false; block.squareMatch = false;
                    }
                }
            }

            isMatch = false;
            _matcher.CheckMap();
            if (!_matcher.isMatch) canMatch = _matcher.CanPlay();
            _matcher.isMatch = false;

        } while (_matcher.isMatch || !canMatch);

        Debug.Log("Create Map End");
        await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token);
    }

    async Task UpdateMap()
    {
        try
        {
            int count = 0;
            bool reUpdate = false;
            bool createDelay = false;
            int firstRow = 0, firstCol = 0;

            do
            {
                foreach (var block in boardArr)
                {
                    if (block == null) continue;
                    if (block.changeBlockState != EBlockState.None)
                    {
                        createDelay = true;
                        _board.CreateNewBlock(block, ELog.UpdateMap, 1, block.changeBlockState);
                        block.SetHp(block.changeHp); block.ResetScore(); block.SetOriginPos();
                        block.changeBlockState = EBlockState.None;
                    }
                    else if (reUpdate || block.IsMatch())
                    {
                        if (block.IsFixdBlock() || block.IsFishBlock()) continue;
                        if (reUpdate && (block.GetBlockState() == EBlockState.RainbowPang || block.IsBallBlock())) continue;
                        firstRow = block.row; firstCol = block.col;
                        var random = UnityEngine.Random.Range(0, _stageInfo.blockTypeCount);
                        createDelay = true;
                        _board.CreateNewBlock(block, ELog.UpdateMap, 2, (EBlockState)random);
                        block.SetHp(-1); block.ResetScore(); block.SetOriginPos();
                    }
                }

                reUpdate = !_matcher.CanPlay();
                if (reUpdate) CHMMain.UI.ShowUI(EUI.UIAlarm, new UIAlarmArg { stringID = 56 });
                if (createDelay) await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token);

                if (count++ > updateMapCount)
                {
                    CHMMain.UI.ShowUI(EUI.UIAlarm, new UIAlarmArg { stringID = 80 });
                    _board.CreateNewBlock(boardArr[firstRow, firstCol], ELog.UpdateMap, 3, EBlockState.YellowBomb);
                    break;
                }
            } while (reUpdate);

            _matcher.isMatch = false;
        }
        catch (TaskCanceledException) { Debug.Log("Cancel Update Map"); }
    }

    async Task RemoveMatchBlock()
    {
        bool removeDelay = false;
        for (int i = 0; i < boardSize; ++i)
        {
            for (int j = 0; j < boardSize; ++j)
            {
                var block = boardArr[i, j];
                if (block == null) continue;
                if (block.IsMatch() && !block.remove)
                {
                    _matcher.CheckArround(block.row, block.col);
                    curScore.Value += 1;
                    removeDelay = true;
                    block.remove = true;
                    block.rectTransform.DOScale(0f, delay);

                    Image img = _selectStage == ESelectStage.Boss
                        ? catFootImgList[UnityEngine.Random.Range(0, catFootImgList.Count)]
                        : goldImg;

                    var gold = CHMMain.Resource.Instantiate(img.gameObject, transform.parent);
                    if (gold != null)
                    {
                        var rect = gold.GetComponent<RectTransform>();
                        if (rect != null)
                        {
                            rect.anchoredPosition = block.rectTransform.anchoredPosition;
                            rect.DOAnchorPosY(rect.anchoredPosition.y + UnityEngine.Random.Range(30f, 50f), .5f).OnComplete(() =>
                                rect.DOAnchorPos(img.rectTransform.anchoredPosition, UnityEngine.Random.Range(.2f, 1f)).OnComplete(() =>
                                    CHMMain.Resource.Destroy(gold)));
                        }
                    }

                    if (block.IsBombBlock() && !block.boom)
                    {
                        bonusScore.Value += 20;
                        await block.Bomb(false);
                        i = -1; break;
                    }
                }
            }
        }

        if (removeDelay)
        {
            CHMMain.Sound.Play(ESound.Ppauk);
            await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token);
        }
    }

    void SetDissapearBlock()
    {
        int row = boardSize - 1;
        for (int i = 0; i < boardSize; ++i)
        {
            var block = boardArr[row, i];
            if (block.IsFishBlock())
            {
                block.tutorialBlock = false;
                block.changeBlockState = (EBlockState)UnityEngine.Random.Range((int)EBlockState.PinkBomb, (int)EBlockState.BlueBomb + 1);
            }
            else if (block.IsBallBlock())
            {
                block.tutorialBlock = false;
                block.changeBlockState = EBlockState.Potal;
                block.changeHp = 5;

                int ballHp = 4;
                for (int k = i + 1; k < boardSize; ++k)
                {
                    var cb = boardArr[row, k];
                    if (ballHp <= 0) break;
                    if (cb.IsNormalBlock() || cb.remove) { cb.changeBlockState = EBlockState.Potal; cb.changeHp = ballHp--; cb.checkHp = true; }
                    else break;
                }
                ballHp = 4;
                for (int k = i - 1; k >= 0; --k)
                {
                    var cb = boardArr[row, k];
                    if (cb.IsNormalBlock() || cb.remove) { cb.changeBlockState = EBlockState.Potal; cb.changeHp = ballHp--; cb.checkHp = true; }
                    else break;
                }
            }
        }
    }

    public async Task<bool> CatInTheBox()
    {
        bool inDelay = false;
        for (int w = 0; w < boardSize; w++)
        {
            for (int h = 0; h < boardSize; h++)
            {
                var block = boardArr[w, h];
                if (block == null || !block.IsBoxBlock()) continue;
                var upBlock = _board.IsValidIndex(w - 1, h) ? boardArr[w - 1, h] : null;
                if (upBlock == null) continue;
                if (block.CatInTheBox(upBlock.GetBlockState()))
                {
                    inDelay = true;
                    upBlock.remove = true; upBlock.match = true;
                    upBlock.rectTransform.DOAnchorPosY(block.rectTransform.anchoredPosition.y, delay);
                    upBlock.rectTransform.DOScale(0f, delay);
                }
            }
        }
        if (inDelay) { await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token); return true; }
        return false;
    }

    RectTransform CreateEffect(ParticleSystem effect, Vector2 movePos)
    {
        var copyObj = CHMMain.Resource.Instantiate(effect.gameObject, transform.parent);
        copyObj.SetActive(true);
        var rt = copyObj.GetComponent<RectTransform>();
        rt.anchoredPosition = movePos;
        return rt;
    }

    public async Task AfterDrag(Block block1, Block block2, bool isBoom = false)
    {
        bool checkCreateBlock = false;

        if (moveCount.Value == 0 && gameResult.Value != EGameState.CatPang) return;

        Time.timeScale = 1;
        _tutorial.HideGuide();
        isLock = true;

        await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token);

        if (block1 && block2 && block1.IsBlock() && block2.IsBlock())
        {
            if (block1.tutorialBlock) block1.tutorialBlock = false;
            if (block2.tutorialBlock) block2.tutorialBlock = false;

            moveIndex1 = block1.index;
            moveIndex2 = block2.index;
            _matcher.SetMoveIndices(moveIndex1, moveIndex2);

            if (block1.IsSpecialBombBlock() && block2.IsSpecialBombBlock())
            { block1.match = true; block2.match = true; await _bombResolver.BoomAll(); isLock = false; return; }
            else if (block1.GetBlockState() == EBlockState.PinkBomb)
            { await _bombResolver.Boom3(block1, block2.GetBlockState()); isLock = false; return; }
            else if (block2.GetBlockState() == EBlockState.PinkBomb)
            { await _bombResolver.Boom3(block2, block1.GetBlockState()); isLock = false; return; }
            else if (block1.IsSpecialBombBlock())
            { await block1.Bomb(); isLock = false; return; }
            else if (block2.IsSpecialBombBlock())
            { await block2.Bomb(); isLock = false; return; }
            else if (block1.IsBombBlock() && block2.IsBombBlock())
            {
                moveCount.Value -= 1;
                bonusScore.Value += 30;
                block2.match = true;
                block1.changeBlockState = (EBlockState)UnityEngine.Random.Range((int)EBlockState.PinkBomb, (int)EBlockState.BlueBomb + 1);
            }
            else if (block1.IsBombBlock()) { await block1.Bomb(); isLock = false; return; }
            else if (block2.IsBombBlock()) { await block2.Bomb(); isLock = false; return; }
        }

        bool back = false;
        _matcher.isMatch = false;
        _matcher.CheckMap();

        if (block1 != null && block2 != null && !_matcher.isMatch)
        {
            _board.ChangeBlock(block1, block2);
            block1.rectTransform.DOAnchorPos(block1.originPos, delay);
            block2.rectTransform.DOAnchorPos(block2.originPos, delay);
            await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token);
            back = true;
        }

        do
        {
            await RemoveMatchBlock();
            await _bombResolver.CreateBombBlock(moveIndex1, moveIndex2);
            await _board.DownBlock();

            if (_selectStage == ESelectStage.Boss) bonusScore.Value += _loginData.attack;

            curScore.Value += bonusScore.Value;
            if (bonusScore.Value > 0)
            {
                bonusScoreText.gameObject.SetActive(true);
                bonusScoreText.SetText(bonusScore.Value);
                await Task.Delay((int)(delay * delayMillisecond));
                bonusScoreText.gameObject.SetActive(false);
            }
            bonusScore.Value = 0;

            if (!checkCreateBlock)
            {
                checkCreateBlock = true;
                _bombResolver.BlockCreatorBlock(EBlockState.WallCreator, EBlockState.Wall);
                _bombResolver.BlockCreatorBlock(EBlockState.PotalCreator, EBlockState.Potal);
            }

            SetDissapearBlock();
            await UpdateMap();
            _matcher.isMatch = false;
            _matcher.CheckMap();

        } while (_matcher.isMatch || await CatInTheBox());

        bool validDrag = block1 != null && block2 != null && !back;
        if (validDrag || (isBoom && gameResult.Value != EGameState.CatPang))
            moveCount.Value -= 1;

        curScore.Value += bonusScore.Value;
        if (bonusScore.Value > 0)
        {
            bonusScoreText.gameObject.SetActive(true);
            bonusScoreText.SetText(bonusScore.Value);
            await Task.Delay((int)(delay * delayMillisecond));
            bonusScoreText.gameObject.SetActive(false);
        }
        bonusScore.Value = 0;

        do
        {
            if (!tutorialNextBlock)
            {
                tutorialNextBlock = true;
                if (_selectStage != ESelectStage.Hard && _stageInfo.tutorialID > 0)
                {
                    var tutInfo = CHMMain.Json.GetTutorialInfo(_stageInfo.tutorialID);
                    if (tutInfo == null || tutInfo.connectNextBlock == EBlockState.None) break;
                    guideBackground.SetActive(true);
                    guideHole.gameObject.SetActive(true);
                    var sv = _tutorial.TutorialBlockSetting(tutInfo.connectNextBlock);
                    guideHole.sizeDelta = sv.Item1;
                    guideHole.anchoredPosition = sv.Item2;
                    guideFinger.gameObject.SetActive(true);
                    guideFinger.anchoredPosition = sv.Item2;
                    guideDesc.SetStringID(tutInfo.descNextBlockStringID);
                }
            }
        } while (false);

        isLock = false;
    }

    async void GameEnd(bool clear)
    {
        if (isLock) { gameResult.Value = clear ? EGameState.GameClearWait : EGameState.GameOverWait; return; }
        if (gameEnd) return;
        gameEnd = true;

        gameResult.Value = clear ? EGameState.GameClear : EGameState.GameOver;

        if (clear && _selectStage == ESelectStage.Boss) _boss.OnClear();

        if (await _bombResolver.CatPang(true))
        {
            gameResult.Value = EGameState.CatPang;
            CHMMain.UI.ShowUI(EUI.UIAlarm, new UIAlarmArg { stringID = 55, closeTime = 1 });
            await Task.Delay(1000);
            await _bombResolver.CatPang();
            gameEnd = false;
            gameResult.Value = _selectStage == ESelectStage.Boss ? EGameState.BossStagePlay : EGameState.NormalOrHardStagePlay;
            return;
        }

        CHMMain.UI.ShowUI(EUI.UIGameEnd, new UIGameEndArg
        {
            clearState = GetClearState(),
            result = gameResult.Value,
            gold = curScore.Value
        });

        if (clear) SaveClearData();
    }

    void SaveClearData()
    {
        switch (_selectStage)
        {
            case ESelectStage.Hard:
                if (CHMData.Instance.GetLoginData(CHMMain.String.CatPang).hardStage < PlayerPrefs.GetInt(CHMMain.String.HardStage))
                {
                    CHMData.Instance.GetLoginData(CHMMain.String.CatPang).hardStage = PlayerPrefs.GetInt(CHMMain.String.HardStage);
#if UNITY_ANDROID
                    CHMGPGS.Instance.ReportLeaderboard(GPGSIds.leaderboard_hard_stage_rank, PlayerPrefs.GetInt(CHMMain.String.HardStage));
#endif
                }
                break;
            case ESelectStage.Boss:
                if (CHMData.Instance.GetLoginData(CHMMain.String.CatPang).bossStage < PlayerPrefs.GetInt(CHMMain.String.BossStage))
                {
                    CHMData.Instance.GetLoginData(CHMMain.String.CatPang).bossStage = PlayerPrefs.GetInt(CHMMain.String.BossStage);
#if UNITY_ANDROID
                    CHMGPGS.Instance.ReportLeaderboard(GPGSIds.leaderboard_boss_stage_rank, PlayerPrefs.GetInt(CHMMain.String.BossStage));
#endif
                }
                break;
            case ESelectStage.Normal:
                if (CHMData.Instance.GetLoginData(CHMMain.String.CatPang).normalStage < PlayerPrefs.GetInt(CHMMain.String.NormalStage))
                {
                    CHMData.Instance.GetLoginData(CHMMain.String.CatPang).normalStage = PlayerPrefs.GetInt(CHMMain.String.NormalStage);
#if UNITY_ANDROID
                    CHMGPGS.Instance.ReportLeaderboard(GPGSIds.leaderboard_normal_stage_rank, PlayerPrefs.GetInt(CHMMain.String.NormalStage));
#endif
                }
                break;
        }
    }

    EClearState GetClearState()
    {
        switch (_selectStage)
        {
            case ESelectStage.Hard:   if (CHMData.Instance.GetLoginData(CHMMain.String.CatPang).hardStage >= PlayerPrefs.GetInt(CHMMain.String.HardStage)) return EClearState.Clear; break;
            case ESelectStage.Boss:   if (CHMData.Instance.GetLoginData(CHMMain.String.CatPang).bossStage >= PlayerPrefs.GetInt(CHMMain.String.BossStage)) return EClearState.Clear; break;
            case ESelectStage.Normal: if (CHMData.Instance.GetLoginData(CHMMain.String.CatPang).normalStage >= PlayerPrefs.GetInt(CHMMain.String.NormalStage)) return EClearState.Clear; break;
        }
        return EClearState.Doing;
    }

    // Block.cs 에서 호출하는 public 파사드
    public void ChangeBlock(Block a, Block b) => _board.ChangeBlock(a, b);
    public bool CheckTutorial() => _tutorial.CheckTutorial();
    public async Task Bomb1(Block b, bool ani = true) => await _bombResolver.Bomb1(b, ani);
    public async Task Bomb2(Block b, bool ani = true) => await _bombResolver.Bomb2(b, ani);
    public async Task Boom3(Block b, EBlockState s, bool ani = true) => await _bombResolver.Boom3(b, s, ani);
    public async Task Bomb4(Block b, bool ani = true) => await _bombResolver.Bomb4(b, ani);
    public async Task Bomb5(Block b, bool ani = true) => await _bombResolver.Bomb5(b, ani);
    public async Task Bomb6(Block b, bool ani = true) => await _bombResolver.Bomb6(b, ani);
    public async Task Bomb7(Block b, bool ani = true) => await _bombResolver.Bomb7(b, ani);
    public async Task Bomb8(Block b, bool ani = true) => await _bombResolver.Bomb8(b, ani);
    public async Task Bomb9(Block b, bool ani = true) => await _bombResolver.Bomb9(b, ani);
    public async Task Bomb10(Block b, bool ani = true) => await _bombResolver.Bomb10(b, ani);
    public async Task Bomb11(Block b, bool ani = true) => await _bombResolver.Bomb11(b, ani);
    public async Task Bomb12(Block b, bool ani = true) => await _bombResolver.Bomb12(b, ani);
    public async Task RainbowPang(Block b, bool ani = true) => await _bombResolver.RainbowPang(b, ani);
    public async Task BoomAll(bool ani = true) => await _bombResolver.BoomAll(ani);
}
```

- [ ] **Step 2: Unity Editor 컴파일 확인**

- [ ] **Step 3: Game.cs 삭제**

Unity Editor에서 `Assets/Scripts/Scenes/Game.cs` 파일을 삭제하면 .meta 파일도 함께 삭제된다. 이후 씬에서 GPGameScene 컴포넌트를 수동으로 재연결해야 한다.

```bash
git rm "Assets/Scripts/Scenes/Game.cs"
git rm "Assets/Scripts/Scenes/Game.cs.meta"
git add Assets/Scripts/Scenes/GPGameScene.cs
git commit -m "refactor: GPGameScene 생성, Game.cs 제거, 배경 루프 제거"
```

---

### Task 8: LBLoginHandler + LBTutorial 생성

**Files:**
- Create: `Assets/Scripts/Lobby/LBLoginHandler.cs`
- Create: `Assets/Scripts/Lobby/LBTutorial.cs`

- [ ] **Step 1: LBLoginHandler.cs 생성**

```csharp
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LBLoginHandler
{
    CHTMPro _userID;
    Button _connectGPGSBtn;
    Button _logoutBtn;
    GameObject _objWait;
    System.Action _onLoginComplete;

    public void Init(CHTMPro userID, Button connectGPGSBtn, Button logoutBtn, GameObject objWait, System.Action onLoginComplete)
    {
        _userID = userID;
        _connectGPGSBtn = connectGPGSBtn;
        _logoutBtn = logoutBtn;
        _objWait = objWait;
        _onLoginComplete = onLoginComplete;
    }

    public bool GetGPGSLogin()
    {
        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        Debug.Log($"GetLoginState : {loginData.connectGPGS}");
        return loginData.connectGPGS;
    }

    public bool GetPhoneLoginState() => PlayerPrefs.GetInt(CHMMain.String.Login) == 1;

    public async Task<bool> SetGPGSLogin(bool success, string gpgsUserName)
    {
        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        loginData.connectGPGS = success;
        Debug.Log($"SetLoginState : {loginData.connectGPGS}");

        if (success)
        {
            Debug.Log($"GPGS Login Success : {gpgsUserName}");
#if UNITY_ANDROID
            await CHMData.Instance.LoadCloudData(CHMMain.String.CatPang);
#endif
            _userID.gameObject.SetActive(true);
            _userID.SetText(gpgsUserName);
        }
        else
        {
            _userID.gameObject.SetActive(false);
            Debug.Log($"GPGS Login Failed");
            await CHMData.Instance.LoadLocalData(CHMMain.String.CatPang);
        }

        CHMData.Instance.GetShopData("1").buy = true;
        PlayerPrefs.SetInt(CHMMain.String.Login, success ? 1 : 0);
        _connectGPGSBtn.gameObject.SetActive(!success);
        _logoutBtn.gameObject.SetActive(success);

        CHMData.Instance.SaveData(CHMMain.String.CatPang);
        _onLoginComplete?.Invoke();
        return true;
    }
}
```

- [ ] **Step 2: LBTutorial.cs 생성**

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LBTutorial
{
    GameObject _guideBackground;
    Button _guideBackgroundBtn;
    List<RectTransform> _guideHoleList;
    CHTMPro _guideDesc;

    public void Init(GameObject guideBackground, Button guideBackgroundBtn,
        List<RectTransform> guideHoleList, CHTMPro guideDesc)
    {
        _guideBackground = guideBackground;
        _guideBackgroundBtn = guideBackgroundBtn;
        _guideHoleList = guideHoleList;
        _guideDesc = guideDesc;
    }

    public async Task<int> TutorialStart()
    {
        _guideBackground.SetActive(true);
        for (int i = 0; i < _guideHoleList.Count; ++i)
        {
            var info = CHMMain.Json.GetGuideInfo(i + 1);
            if (info == null) break;
            _guideHoleList[i].gameObject.SetActive(true);
            _guideDesc.SetStringID(info.descStringID);

            var clickTask = new TaskCompletionSource<bool>();
            var sub = _guideBackgroundBtn.OnClickAsObservable().Subscribe(_ => clickTask.SetResult(true));
            await clickTask.Task;
            _guideHoleList[i].gameObject.SetActive(false);
            sub.Dispose();
        }
        return _guideHoleList.Count;
    }
}
```

- [ ] **Step 3: Unity Editor 컴파일 확인**

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/Lobby/LBLoginHandler.cs Assets/Scripts/Lobby/LBTutorial.cs
git commit -m "refactor: LBLoginHandler, LBTutorial 추출"
```

---

### Task 9: LBLobbyScene 생성 (First.cs 교체)

**Files:**
- Create: `Assets/Scripts/Scenes/LBLobbyScene.cs`
- Delete: `Assets/Scripts/Scenes/First.cs`

- [ ] **Step 1: LBLobbyScene.cs 생성**

```csharp
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class LBLobbyScene : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] GameObject stageSelect1;
    [SerializeField] GameObject stageSelect2;
    [SerializeField] PageMove pageMove;
    [SerializeField] Button missionBtn;
    [SerializeField] Button startBtn;
    [SerializeField] Button connectGPGSBtn;
    [SerializeField] Button logoutBtn;
    [SerializeField] Button shopBtn;
    [SerializeField] Button bombBtn;
    [SerializeField] Button menuBtn;
    [SerializeField] Button rankingBtn;
    [SerializeField] CHAdvertise adScript;
    [SerializeField] ReactiveProperty<bool> dataDownload = new ReactiveProperty<bool>();
    [SerializeField] ReactiveProperty<bool> bundleDownload = new ReactiveProperty<bool>();
    [SerializeField] GameObject guideBackground;
    [SerializeField] Button guideBackgroundBtn;
    [SerializeField] List<RectTransform> guideHoleList = new List<RectTransform>();
    [SerializeField] CHTMPro guideDesc;
    [SerializeField] CHTMPro userID;
    [SerializeField] GameObject objWait;

    CancellationTokenSource tokenSource;
    bool initButton = false;
    bool firstStartBtnClick = false;

    LBLoginHandler _loginHandler;
    LBTutorial _tutorial;

    void InitButton()
    {
        if (initButton) return;
        initButton = true;

        startBtn.OnClickAsObservable().Subscribe(async _ =>
        {
            if (!bundleDownload.Value || !dataDownload.Value) return;
            firstStartBtnClick = true;
            var arg = new UIStageSelectArg();
            arg.stageSelect += async (select) => await StageSelect(select);
            CHMMain.UI.ShowUI(Defines.EUI.UIStageSelect, arg);
        });

        missionBtn.OnClickAsObservable().Subscribe(_ => CHMMain.UI.ShowUI(Defines.EUI.UIMission, new CHUIArg()));

        logoutBtn.OnClickAsObservable().Subscribe(async _ =>
        {
            if (!_loginHandler.GetGPGSLogin()) return;
            await _loginHandler.SetGPGSLogin(false, "");
#if UNITY_ANDROID
            CHMGPGS.Instance.Logout();
#endif
        });

        shopBtn.OnClickAsObservable().Subscribe(_ => CHMMain.UI.ShowUI(Defines.EUI.UIShop, new CHUIArg()));
        bombBtn.OnClickAsObservable().Subscribe(_ => CHMMain.UI.ShowUI(Defines.EUI.UISetting, new CHUIArg()));

        rankingBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (!CHMData.Instance.GetLoginData(CHMMain.String.CatPang).connectGPGS)
                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg { stringID = 107 });
            else
                CHMMain.UI.ShowUI(Defines.EUI.UIRank, new CHUIArg());
        });

        connectGPGSBtn.OnPointerClickAsObservable().Subscribe(_ =>
        {
#if UNITY_ANDROID
            if (!CHMData.Instance.GetLoginData(CHMMain.String.CatPang).connectGPGS)
            {
                objWait.SetActive(true);
                CHMGPGS.Instance.Login(async (success, localUser) =>
                {
                    await _loginHandler.SetGPGSLogin(success, localUser.userName);
                    objWait.SetActive(false);
                });
            }
#endif
        });

        menuBtn.OnClickAsObservable().Subscribe(_ =>
        {
            var arg = new UIStageSelectArg();
            arg.stageSelect += async (select) => await StageSelect(select);
            CHMMain.UI.ShowUI(Defines.EUI.UIStageSelect, arg);
        });
    }

    async void Start()
    {
        tokenSource = new CancellationTokenSource();

        _loginHandler = new LBLoginHandler();
        _loginHandler.Init(userID, connectGPGSBtn, logoutBtn, objWait, () => dataDownload.Value = true);

        _tutorial = new LBTutorial();
        _tutorial.Init(guideBackground, guideBackgroundBtn, guideHoleList, guideDesc);

        menuBtn.gameObject.SetActive(false);
        stageSelect1.SetActive(false);
        stageSelect2.SetActive(false);
        startBtn.gameObject.SetActive(false);
        missionBtn.gameObject.SetActive(false);
        connectGPGSBtn.gameObject.SetActive(false);
        logoutBtn.gameObject.SetActive(false);
        shopBtn.gameObject.SetActive(false);
        bombBtn.gameObject.SetActive(false);
        rankingBtn.gameObject.SetActive(false);
        objWait.SetActive(false);
        userID.gameObject.SetActive(false);
        foreach (var h in guideHoleList) h.gameObject.SetActive(false);
        guideBackground.SetActive(false);
        guideBackgroundBtn.gameObject.SetActive(false);

        CHMIAP.Instance.Init();
        CHMAdmob.Instance.Init();

        dataDownload.Subscribe(async dl =>
        {
            if (CHMAssetBundle.Instance.firstDownload && dl && bundleDownload.Value)
            {
                CHMAssetBundle.Instance.firstDownload = false;
                startBtn.gameObject.SetActive(true);
            }
        });

        bundleDownload.Subscribe(async bl =>
        {
            if (bl && !dataDownload.Value)
            {
                if (_loginHandler.GetPhoneLoginState())
                {
                    objWait.SetActive(true);
#if UNITY_ANDROID
                    CHMGPGS.Instance.Login(async (success, localUser) =>
                    {
                        await _loginHandler.SetGPGSLogin(success, localUser.userName);
                        objWait.SetActive(false);
                    });
#endif
                }
                else await _loginHandler.SetGPGSLogin(false, "");
            }
            else if (CHMAssetBundle.Instance.firstDownload && bl && dataDownload.Value)
            {
                CHMAssetBundle.Instance.firstDownload = false;
                startBtn.gameObject.SetActive(true);
            }
        });

        if (CHMAssetBundle.Instance.firstDownload)
        {
            pageMove.ActiveMoveBtn(false);
        }
        else
        {
            bundleDownload.Value = true;
            dataDownload.Value = true;

            pageMove.Init((Defines.ESelectStage)PlayerPrefs.GetInt(CHMMain.String.SelectStage));
            stageSelect1.SetActive(true);
            stageSelect2.SetActive(true);
            missionBtn.gameObject.SetActive(true);
            shopBtn.gameObject.SetActive(true);
            bombBtn.gameObject.SetActive(true);
            menuBtn.gameObject.SetActive(true);
            rankingBtn.gameObject.SetActive(true);

            var login = _loginHandler.GetGPGSLogin();
            connectGPGSBtn.gameObject.SetActive(!login);
            logoutBtn.gameObject.SetActive(login);

            adScript.GetAdvertise();

            var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
            if (loginData.guideIndex == 0)
            {
                Time.timeScale = 0;
                guideBackground.SetActive(true);
                guideBackground.transform.SetAsLastSibling();
                guideBackgroundBtn.gameObject.SetActive(true);
                guideBackgroundBtn.transform.SetAsLastSibling();

                loginData.guideIndex = await _tutorial.TutorialStart();

                guideBackground.SetActive(false);
                guideBackgroundBtn.gameObject.SetActive(false);
                Time.timeScale = 1;
                CHMData.Instance.SaveData(CHMMain.String.CatPang);
            }
        }

        bundleDownload.Value = true;
        InitButton();
        CHMData.Instance.GetShopData("1").buy = true;
    }

    private void OnApplicationQuit() => CHMData.Instance.SaveData(CHMMain.String.CatPang);

    async Task StageSelect(int select)
    {
        PlayerPrefs.SetInt(CHMMain.String.SelectStage, select);
        pageMove.Init((Defines.ESelectStage)select);
        startBtn.gameObject.SetActive(false);
        missionBtn.gameObject.SetActive(true);
        shopBtn.gameObject.SetActive(true);
        bombBtn.gameObject.SetActive(true);
        stageSelect1.SetActive(true);
        stageSelect2.SetActive(true);
        menuBtn.gameObject.SetActive(true);
        rankingBtn.gameObject.SetActive(true);

        CHMMain.Sound.Play(Defines.ESound.Bgm);

        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        if (loginData.guideIndex == 0)
        {
            Time.timeScale = 0;
            guideBackground.SetActive(true);
            guideBackground.transform.SetAsLastSibling();
            guideBackgroundBtn.gameObject.SetActive(true);
            guideBackgroundBtn.transform.SetAsLastSibling();

            loginData.guideIndex = await _tutorial.TutorialStart();

            guideBackground.SetActive(false);
            guideBackgroundBtn.gameObject.SetActive(false);
            Time.timeScale = 1;
            CHMData.Instance.SaveData(CHMMain.String.CatPang);
        }
    }
}
```

- [ ] **Step 2: Unity Editor 컴파일 확인**

- [ ] **Step 3: First.cs 삭제 및 씬 재연결**

Unity Editor에서 `Assets/Scripts/Scenes/First.cs` 삭제 후, FirstScene의 GameObject에서 `First` 컴포넌트를 `LBLobbyScene`으로 재연결한다.

```bash
git rm "Assets/Scripts/Scenes/First.cs"
git rm "Assets/Scripts/Scenes/First.cs.meta"
git add Assets/Scripts/Scenes/LBLobbyScene.cs
git commit -m "refactor: LBLobbyScene 생성, First.cs 제거, 배경 루프 제거"
```

---

### Task 10: Unity 씬 컴포넌트 재연결 + 최종 커밋

- [ ] **Step 1: GameScene 재연결**

Unity Editor에서 GameScene을 열어 보드 GameObject의 `Game` 컴포넌트를 제거하고 `GPGameScene`을 추가. Inspector에서 모든 SerializeField를 이전과 동일하게 연결한다.

- [ ] **Step 2: FirstScene(LobbyScene) 재연결**

Unity Editor에서 FirstScene을 열어 `First` 컴포넌트를 제거하고 `LBLobbyScene`을 추가. Inspector에서 SerializeField 재연결.

- [ ] **Step 3: Block prefab 재연결**

Block prefab에서 `game` 필드(이제 `GPGameScene` 타입)를 씬의 `GPGameScene` GameObject로 연결.

- [ ] **Step 4: Play Mode 테스트**

Unity Editor에서 GameScene 실행, 블록 드래그/매치/폭탄/보스 스테이지 정상 동작 확인.

- [ ] **Step 5: 씬 파일 저장 및 최종 커밋**

```bash
git add Assets/Scenes/
git commit -m "refactor: Unity 씬 컴포넌트 재연결 완료"
```

---

## 자기 검토 수정사항

### 수정 1: LBTutorial.cs — UniRx using 누락

`LBTutorial.cs` 파일 상단에 UniRx import 추가:

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;           // 추가
using UniRx.Triggers;  // 추가
using UnityEngine;
using UnityEngine.UI;
```

### 수정 2: LBLoginHandler — 로그인 성공 후 StageSelect 콜백 누락

원본 `First.cs`에는 GPGS 로그인 성공 시 `firstStartBtnClick == true`이면 `StageSelect`를 호출하는 로직이 있었음. `LBLoginHandler`로 분리하면서 이 로직이 유실됨.

**수정: `LBLoginHandler.Init` 시그니처 변경** — `onLoginComplete`를 `Func<bool, Task>`로 변경해 success 여부를 전달:

```csharp
// LBLoginHandler.cs — Init 및 SetGPGSLogin 수정
Func<bool, Task> _onLoginComplete;

public void Init(CHTMPro userID, Button connectGPGSBtn, Button logoutBtn,
    GameObject objWait, Func<bool, Task> onLoginComplete)
{
    // ... 동일
    _onLoginComplete = onLoginComplete;
}

public async Task<bool> SetGPGSLogin(bool success, string gpgsUserName)
{
    // ... 기존 로직 동일 ...

    CHMData.Instance.SaveData(CHMMain.String.CatPang);
    await _onLoginComplete?.Invoke(success);  // bool 전달로 변경
    return true;
}
```

**수정: `LBLobbyScene.Start`에서 Init 호출부 변경** — success 여부를 받아 StageSelect 처리:

```csharp
_loginHandler.Init(userID, connectGPGSBtn, logoutBtn, objWait, async (success) =>
{
    dataDownload.Value = true;
    if (success && firstStartBtnClick && CHMData.Instance.newUser == false)
        await StageSelect(PlayerPrefs.GetInt(CHMMain.String.SelectStage));
});
```

> Task 8, Task 9 진행 시 위 수정된 시그니처로 작성할 것.
