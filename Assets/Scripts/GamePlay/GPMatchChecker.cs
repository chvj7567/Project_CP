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
