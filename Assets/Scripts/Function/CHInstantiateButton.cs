using System.Collections.Generic;
using UnityEngine;

public class CHInstantiateButton : CHSingleton<CHInstantiateButton>
{
    [SerializeField] static GameObject origin;
    [SerializeField] static float margin = 0f;
    [SerializeField] static int horizontalCount = 1;
    [SerializeField] static int verticalCount = 1;
    [SerializeField] static List<string> buttonValue = new List<string>();

    [SerializeField, ReadOnly] static int index = 0;
    [SerializeField, ReadOnly] static Dictionary<RectTransform, Block> blockDict = new Dictionary<RectTransform, Block>();

    static float buttonWidth;
    static float buttonHeight;

    static public float GetHorizontalDistance()
    {
        return buttonWidth + margin;
    }

    static public float GetVerticalDistance()
    {
        return buttonHeight + margin;
    }

    static public Dictionary<RectTransform, Block> GetBlockDict()
    {
        return blockDict;
    }

    static public void ResetBlockDict()
    {
        blockDict.Clear();
    }

    static public (RectTransform, Block) GetBlockInfo(Vector2 pos)
    {
        foreach (var block in blockDict)
        {
            if ((pos - block.Key.anchoredPosition).magnitude <= buttonWidth / 2f)
            {
                return (block.Key, block.Value);
            }
        }

        return (null, null);
    }

    public void InstantiateButton(GameObject _origin, float _margin, int _horizontalCount, int _verticalCount, Transform _parent, Block[,] _boardArr)
    {
        if (_origin == null) return;

        origin = _origin;
        margin = _margin;
        horizontalCount = _horizontalCount;
        verticalCount = _verticalCount;

        RectTransform buttonRectTransform = _origin.GetComponent<RectTransform>();

        float buttonX = buttonRectTransform.anchoredPosition.x;
        float buttonY = buttonRectTransform.anchoredPosition.y;

        buttonWidth = Mathf.Abs(buttonRectTransform.rect.x * 2);
        buttonHeight = Mathf.Abs(buttonRectTransform.rect.y * 2);

        Dictionary<Vector2, Vector2> posDict = new Dictionary<Vector2, Vector2>();

        int row = 0;
        int col = 0;
        for (int i = 0; i < _verticalCount; ++i)
        {
            col = 0;
            for (int j = 0; j < _horizontalCount; ++j)
            {
                posDict.Add(new Vector2(row, col), new Vector2(buttonX + (_margin + buttonWidth) * j, buttonY + -(_margin + buttonHeight) * i));
                ++col;
            }

            ++row;
        }

        foreach (var pos in posDict)
        {
            GameObject addObject = Instantiate(_origin, _parent);
            RectTransform rectTransform = addObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = pos.Value;
            var block = addObject.GetOrAddComponent<Block>();

            block.row = (int)pos.Key.x;
            block.col = (int)pos.Key.y;
            block.index = index++;
            block.originPos = pos.Value;

            _boardArr[block.row, block.col] = block;
            addObject.name = $"Block{block.row}/{block.col}";
            blockDict.Add(rectTransform, block);
        }

        _origin.SetActive(false);
    }
}
