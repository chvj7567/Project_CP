using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ChvjUnityInfra
{

public enum PoolingScrollViewDirection
{
    Vertical,
    Horizontal,
}

public enum PoolingScrollViewAlign
{
    LeftOrTop,
    Center,
    RightOrBottom,
}

public enum PoolingScrollViewScrollingDirection
{
    UpOrLeft,
    DownOrRight,
}

public class PoolingScrollViewItem<TItem>
{
    public int index;
    public TItem item;
}

[RequireComponent(typeof(ScrollRect))]
public abstract class CHPoolingScrollView<TItem, TData> : MonoBehaviour where TItem : MonoBehaviour
{
    protected LinkedList<PoolingScrollViewItem<TItem>> liPoolItem = new LinkedList<PoolingScrollViewItem<TItem>>();

    public List<TItem> ItemList
    {
        get
        {
            return liPoolItem.Select(_ => _.item).ToList();
        }
    }

    protected List<TData> liData = new List<TData>();

    public List<TData> DataList
    {
        get
        {
            return liData.ToList();
        }
    }

    [SerializeField, Header("������ ������ ������Ʈ")]
    private GameObject _origin;

    [SerializeField, Header("������ ũ��(Zero�̸� Origin ������� ����")]
    private Vector2 _itemSize = Vector2.zero;

    [SerializeField, Header("������ ���� ����")]
    private Vector2 _itemGap = Vector2.zero;

    [SerializeField, Header("�е� ����")]
    private RectOffset _padding = new RectOffset();

    [SerializeField, Header("��ũ�� ���� ����")]
    private PoolingScrollViewDirection _scrollDirection = PoolingScrollViewDirection.Vertical;

    [SerializeField, Header("�� ����, 0�����̸� �ڵ� ���")]
    private int _rowCount = 0;

    [SerializeField, Header("�� ����, 0�����̸� �ڵ� ���")]
    private int _columnCount = 0;

    [SerializeField, Header("������ƮǮ ���� ����, 0 �����̸� �ڵ� �Ҵ�")]
    private int _poolItemCount = 0;

    [SerializeField, Header("������ ���� ����")]
    private PoolingScrollViewAlign _align = PoolingScrollViewAlign.Center;

    [SerializeField, Header("��ũ�Ѻ� ������ ����� ���ΰ�ħ ����")]
    private bool _refresh = false;

    [Space]

    private Vector2 _prevScrollPosition = Vector2.zero;
    private GameObject _contentObject;
    private RectTransform _contentRectTransform;
    private RectTransform _viewPortRectTransform;
    private CanvasGroup _canvasGroupContent;
    private ScrollRect _scrollRect;

    private int _calcRowCount = 0;
    private int _calcColumnCount = 0;

    public int LineCount
    {
        get
        {
            int line = 0;

            if (liData.Count == 0)
                return line;

            int count = 1;

            switch (_scrollDirection)
            {
                case PoolingScrollViewDirection.Vertical:
                    {
                        count = _calcColumnCount;
                    }
                    break;
                case PoolingScrollViewDirection.Horizontal:
                    {
                        count = _calcRowCount;
                    }
                    break;
            }

            line = liData.Count / count;
            if (liData.Count % count != 0)
            {
                line += 1;
            }

            return line;
        }
    }

    public float ItemsWidth
    {
        get
        {
            return _calcColumnCount * _itemSize.x + (_calcColumnCount - 1) * _itemGap.x;
        }
    }

    public float ItemsHeight
    {
        get
        {
            return _calcRowCount * _itemSize.y + (_calcRowCount - 1) * _itemGap.y;
        }
    }

    public float ContentWidth
    {
        get
        {
            return _contentRectTransform.rect.width;
        }
    }

    public float ContentHeight
    {
        get
        {
            return _contentRectTransform.rect.height;
        }
    }

    public float ScrollViewWidth
    {
        get
        {
            int line = LineCount;
            float width = line * _itemSize.x;
            width += (line - 1) * _itemGap.x;
            width += _padding.left + _padding.right;
            return width;
        }
    }

    public float ScrollViewHeight
    {
        get
        {
            int line = LineCount;
            float height = line * _itemSize.y;
            height += (line - 1) * _itemGap.y;
            height += _padding.top + _padding.bottom;
            return height;
        }
    }

    private void Awake()
    {
        if (_scrollRect == null)
            _scrollRect = GetComponent<ScrollRect>();

        if (_viewPortRectTransform == null)
            _viewPortRectTransform = _scrollRect.viewport;

        if (_contentObject == null)
            _contentObject = _scrollRect.content.gameObject;

        if (_contentRectTransform == null)
            _contentRectTransform = _scrollRect.content.GetComponent<RectTransform>();

        if (_canvasGroupContent = null)
            _canvasGroupContent = _scrollRect.content.GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        _origin.SetActive(false);

        var scrollRect = GetComponent<ScrollRect>();
        scrollRect.onValueChanged.AddListener(OnScroll);
    }

    /// <summary>
    /// RectTransform 크기 변경 시 Unity가 자동 호출. _refresh가 true면 재구성.
    /// </summary>
    private void OnRectTransformDimensionsChange()
    {
        if (_refresh)
        {
            Refresh();
        }
    }

    /// <summary>
    /// ������ �ʱ�ȭ ��ũ���Ͽ� �ε����� �ٲ𶧸��� ȣ��
    /// </summary>
    /// <param name="item"></param>
    /// <param name="data"></param>
    /// <param name="index"></param>
    public abstract void InitItem(TItem item, TData data, int index);

    /// <summary>
    /// Ǯ�� ������Ʈ ���� �� ȣ��(���� 1ȸ)
    /// </summary>
    /// <param name="item"></param>
    public abstract void InitPoolingObject(TItem item);

    /// <summary>
    /// ��ũ�� �� ������ ����
    /// </summary>
    /// <param name="dataList"></param>
    public void SetItemList(List<TData> dataList)
    {
        liData.Clear();
        liData.AddRange(dataList);

        switch (_scrollDirection)
        {
            case PoolingScrollViewDirection.Vertical:
                {
                    _scrollRect.horizontal = false;
                    _scrollRect.vertical = true;
                }
                break;
            case PoolingScrollViewDirection.Horizontal:
                {
                    _scrollRect.horizontal = true;
                    _scrollRect.vertical = false;
                }
                break;
        }

        //# ������ ũ�� ����
        SetItemSize();

        //# �� ���� ����
        SetRowCount();

        //# �� ���� ����
        SetColumnCount();

        //# ������ Ǯ�� ���� ����
        SetPoolItemCount();

        //# Content ������Ʈ ����
        SetContentTransform();

        //# Ǯ�� ������Ʈ ����
        CreatePoolingObject();

        //# ������ �ʱ�ȭ
        InitItem();
    }

    private bool ExistCurrentIndex(int index)
    {
        foreach (var item in liPoolItem)
        {
            if (item.index == index)
                return true;
        }

        return false;
    }

    private void SetScrollPosition(int index, int destIndex, Action completeCallback = null, float duration = .1f)
    {
        var pos = GetItemPosition(index);

        float x = _contentRectTransform.anchoredPosition.x;
        float y = _contentRectTransform.anchoredPosition.y;

        //# �ִ� ��ũ�� ��ġ �� ���
        switch (_scrollDirection)
        {
            case PoolingScrollViewDirection.Vertical:
                {
                    float tempY = Mathf.Min(ContentHeight - _viewPortRectTransform.rect.height, -pos.y);
                    tempY = tempY < 0 ? 0 : tempY;
                    y = tempY;
                }
                break;
            case PoolingScrollViewDirection.Horizontal:
                {
                    float tempX = Mathf.Min(ContentHeight - _viewPortRectTransform.rect.width, -pos.x);
                    tempX = tempX < 0 ? 0 : tempX;
                    x = tempX;
                }
                break;
        }

        StartCoroutine(LerpAnchorPos(_contentRectTransform, new Vector2(x, y), duration, () =>
        {
            if (index == destIndex)
            {
                completeCallback?.Invoke();
            }
            else
            {
                SetScrollPosition(destIndex, completeCallback);
            }
        }));
    }

    private IEnumerator LerpAnchorPos(RectTransform rt, Vector2 target, float duration, Action onComplete)
    {
        Vector2 start = rt.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rt.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }
        rt.anchoredPosition = target;
        onComplete?.Invoke();
    }

    public void SetScrollPosition(int index, Action completeCallback = null, float duration = .1f)
    {
        if (ExistCurrentIndex(index))
        {
            SetScrollPosition(index, index, completeCallback, duration);
        }
        else
        {
            int lastIndex = liPoolItem.Last().index;
            SetScrollPosition(lastIndex, index, completeCallback, duration);
        }
    }

    public void Refresh()
    {
        SetItemList(liData.ToList());
    }

    public void Clear()
    {
        liData.Clear();
        liPoolItem.Clear();
        if (_contentObject)
        {
            int childCount = _contentObject.transform.childCount;
            GameObject[] children = new GameObject[childCount];

            for (int i = 0; i < childCount; i++)
            {
                children[i] = _contentObject.transform.GetChild(i).gameObject;
            }

            foreach (var child in children)
            {
                child.SetActive(false);
            }
        }
    }

    private void SetItemSize()
    {
        RectTransform rectTransform = _origin.GetComponent<RectTransform>();
        if (rectTransform == null)
            throw new NullReferenceException();

        _itemSize.x = rectTransform.rect.width * rectTransform.localScale.x;
        _itemSize.y = rectTransform.rect.height * rectTransform.localScale.y;
    }

    private void SetColumnCount()
    {
        if (_columnCount <= 0)
        {
            float width = _viewPortRectTransform.rect.width - (_padding.left + _padding.right);
            _calcColumnCount = Mathf.Max(1, Mathf.FloorToInt(width / (_itemSize.x + _itemGap.x)));
        }
        else
        {
            _calcColumnCount = _columnCount;
        }
    }

    private void SetRowCount()
    {
        if (_rowCount <= 0)
        {
            float width = _viewPortRectTransform.rect.height - (_padding.top + _padding.bottom);
            _calcRowCount = Mathf.Max(1, Mathf.FloorToInt(width / (_itemSize.y + _itemGap.y)));
        }
        else
        {
            _calcRowCount = _rowCount;
        }
    }

    private void SetPoolItemCount()
    {
        if (_poolItemCount <= 0)
        {
            switch (_scrollDirection)
            {
                case PoolingScrollViewDirection.Vertical:
                    {
                        int line = Mathf.RoundToInt(_viewPortRectTransform.rect.size.y / _itemSize.y);
                        line += 2;
                        _poolItemCount = line * _calcColumnCount;
                    }
                    break;
                case PoolingScrollViewDirection.Horizontal:
                    {
                        int line = Mathf.RoundToInt(_viewPortRectTransform.rect.size.x / _itemSize.x);
                        line += 2;
                        _poolItemCount = line * _calcRowCount;
                    }
                    break;
            }
        }
    }

    private void SetContentTransform()
    {
        switch (_scrollDirection)
        {
            case PoolingScrollViewDirection.Vertical:
                {
                    //# ��Ʈ��ġ ��Ŀ�� ����
                    _contentRectTransform.anchorMax = new Vector2(.5f, 1f);
                    _contentRectTransform.anchorMin = new Vector2(.5f, 1f);
                    _contentRectTransform.pivot = new Vector2(.5f, 1f);

                    //# ������ �缳��
                    _contentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _viewPortRectTransform.rect.width);
                    _contentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ScrollViewHeight);

                    //# ������ �ֻ������ �̵�
                    _contentRectTransform.anchoredPosition = Vector2.zero;
                }
                break;
            case PoolingScrollViewDirection.Horizontal:
                {
                    //# ��Ʈ��ġ ��Ŀ�� ����
                    _contentRectTransform.anchorMax = new Vector2(0f, .5f);
                    _contentRectTransform.anchorMin = new Vector2(0f, .5f);
                    _contentRectTransform.pivot = new Vector2(0f, .5f);

                    //# ������ �缳��
                    _contentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ScrollViewWidth);
                    _contentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _viewPortRectTransform.rect.height);

                    //# ������ �ֻ������ �̵�
                    _contentRectTransform.anchoredPosition = Vector2.zero;
                }
                break;
        }
    }

    private Vector2 GetItemPosition(int index)
    {
        Vector2 pos = Vector2.zero;

        switch (_scrollDirection)
        {
            case PoolingScrollViewDirection.Horizontal:
                pos = GetItemHorizontalPosition(index);
                break;
            case PoolingScrollViewDirection.Vertical:
                pos = GetItemVerticalPosition(index);
                break;
        }

        return pos;
    }

    private Vector2 GetItemHorizontalPosition(int index)
    {
        int rowIndex = index % _calcRowCount;

        float y = rowIndex * _itemSize.y;
        y += rowIndex * _itemGap.y;

        float diff = ContentHeight - ItemsHeight;

        switch (_align)
        {
            case PoolingScrollViewAlign.LeftOrTop:
                {
                    y += 0;
                    y += _padding.top;
                }
                break;
            case PoolingScrollViewAlign.Center:
                {
                    y += (diff) * 0.5f;
                    y += _padding.top;
                }
                break;
            case PoolingScrollViewAlign.RightOrBottom:
                {
                    y += diff;
                    y -= _padding.bottom;
                }
                break;
        }

        y *= -1;

        int colIndex = index / _calcRowCount;

        float x = colIndex * _itemSize.x;
        x += colIndex * _itemGap.x;
        x += _padding.left;

        return new Vector2(x, y);
    }

    private Vector2 GetItemVerticalPosition(int index)
    {
        int colIndex = index % _calcColumnCount;

        float x = colIndex * _itemSize.x;
        x += colIndex * _itemGap.x;

        float diff = ContentWidth - ItemsWidth;

        switch (_align)
        {
            case PoolingScrollViewAlign.LeftOrTop:
                {
                    x += 0;
                    x += _padding.left;
                }
                break;
            case PoolingScrollViewAlign.Center:
                {
                    x += (diff) * 0.5f;
                    x += _padding.left;
                }
                break;
            case PoolingScrollViewAlign.RightOrBottom:
                {
                    x += diff;
                    x -= _padding.right;
                }
                break;
        }

        int rowIndex = index / _calcColumnCount;

        float y = rowIndex * _itemSize.y;
        y += rowIndex * _itemGap.y;
        y += _padding.top;
        y *= -1;

        return new Vector2(x, y);
    }

    public Rect GetViewConentRect()
    {
        Rect rect = new Rect();

        switch (_scrollDirection)
        {
            case PoolingScrollViewDirection.Vertical:
                {
                    float scrollRectY = _viewPortRectTransform.rect.size.y;
                    rect.Set(_contentRectTransform.anchoredPosition.x, -1 * (_contentRectTransform.anchoredPosition.y + scrollRectY), ContentWidth, scrollRectY + _itemSize.y);
                }
                break;
            case PoolingScrollViewDirection.Horizontal:
                {
                    float scrollRectX = _viewPortRectTransform.rect.size.x;
                    float scrollRectY = _viewPortRectTransform.rect.size.y;
                    rect.Set(-1 * _contentRectTransform.anchoredPosition.x, -1 * (_contentRectTransform.anchoredPosition.y + scrollRectY), scrollRectX + _itemSize.x, ContentHeight);
                }
                break;
        }

        return rect;
    }

    private void OnScroll(Vector2 scrollPosition)
    {
        if (liPoolItem.Count <= 0)
            return;

        Vector2 delta = scrollPosition - _prevScrollPosition;
        _prevScrollPosition = scrollPosition;

        switch (_scrollDirection)
        {
            case PoolingScrollViewDirection.Vertical:
                {
                    //# ���� ��ġ�� �޶����ٸ�
                    if (delta.y != 0)
                    {
                        UpdateContent(delta.y > 0 ? PoolingScrollViewScrollingDirection.UpOrLeft : PoolingScrollViewScrollingDirection.DownOrRight);
                    }
                }
                break;
            case PoolingScrollViewDirection.Horizontal:
                {
                    //# ���� ��ġ�� �޶����ٸ�
                    if (delta.x != 0)
                    {
                        UpdateContent(delta.x < 0 ? PoolingScrollViewScrollingDirection.UpOrLeft : PoolingScrollViewScrollingDirection.DownOrRight);
                    }
                }
                break;
        }
    }

    private void UpdateContent(PoolingScrollViewScrollingDirection direction)
    {
        if (liPoolItem.Count <= 0)
            return;

        Rect contentRect = GetViewConentRect();
        Rect itemRect = new Rect();

        switch (direction)
        {
            case PoolingScrollViewScrollingDirection.UpOrLeft:
                {
                    int firstIndex = liPoolItem.First.Value.index;
                    bool wasOverlapping = false;
                    for (int i = firstIndex - 1; i >= 0; --i)
                    {
                        Vector2 itemPosition = GetItemPosition(i);
                        itemRect.Set(itemPosition.x, itemPosition.y, _itemSize.x, _itemSize.y);

                        if (contentRect.Overlaps(itemRect))
                        {
                            wasOverlapping = true;
                            var node = liPoolItem.Last;
                            liPoolItem.Remove(node);

                            InitItem(node.Value.item, i);

                            node.Value.index = i;
                            liPoolItem.AddFirst(node);
                        }
                        else if (wasOverlapping)
                        {
                            // 한 번 viewport와 겹친 후 다시 벗어났음 — 그 너머 인덱스는 모두 viewport 밖
                            break;
                        }
                    }
                }
                break;
            case PoolingScrollViewScrollingDirection.DownOrRight:
                {
                    int lastIndex = liPoolItem.Last.Value.index;
                    bool wasOverlapping = false;
                    for (int i = lastIndex + 1; i < liData.Count; ++i)
                    {
                        Vector2 itemPosition = GetItemPosition(i);
                        itemRect.Set(itemPosition.x, itemPosition.y, _itemSize.x, _itemSize.y);

                        if (contentRect.Overlaps(itemRect))
                        {
                            wasOverlapping = true;
                            var node = liPoolItem.First;
                            liPoolItem.Remove(node);

                            InitItem(node.Value.item, i);

                            node.Value.index = i;
                            liPoolItem.AddLast(node);
                        }
                        else if (wasOverlapping)
                        {
                            // 한 번 viewport와 겹친 후 다시 벗어났음 — 그 너머 인덱스는 모두 viewport 밖
                            break;
                        }
                    }
                }
                break;
        }
    }

    private void InitItem()
    {
        liPoolItem.Clear();

        //# ������ Ǯ�� ������Ʈ ������
        int childCount = _contentObject.transform.childCount;
        GameObject[] arrChildObj = new GameObject[childCount];
        for (int i = 0; i < childCount; ++i)
        {
            arrChildObj[i] = _contentObject.transform.GetChild(i).gameObject;
        }

        Rect contentRect = GetViewConentRect();
        Rect itemRect = new Rect();
        int firstIndex = Enumerable.Range(0, liData.Count).FirstOrDefault(_ =>
        {
            Vector2 itemPosition = GetItemPosition(_);
            itemRect.Set(itemPosition.x, itemPosition.y, _itemSize.x, _itemSize.y);

            //# Content�� ��ġ�� ���� ��ȯ
            return contentRect.Overlaps(itemRect);
        });

        for (int i = 0; i < childCount; ++i)
        {
            arrChildObj[i].SetActive(true);
        }

        for (int i = 0; i < arrChildObj.Length; ++i)
        {
            int index = i + firstIndex;
            TItem item = arrChildObj[i].GetComponent<TItem>();

            InitItem(item, index);
            PoolingScrollViewItem<TItem> poolItem = new PoolingScrollViewItem<TItem>() { index = index, item = item };
            liPoolItem.AddLast(poolItem);
        }
    }

    private void InitItem(TItem item, int index)
    {
        TData info = liData.ElementAtOrDefault(index);

        bool isNotNull = info != null;
        item.gameObject.SetActive(isNotNull);

        if (isNotNull)
        {
            InitItem(item, info, index);
        }

        InitItemTransform(item.gameObject, index);
        item.gameObject.name = $"{_origin.name} {index}";
    }

    private void InitItemTransform(GameObject item, int index)
    {
        var rectTransform = item.GetComponent<RectTransform>();

        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);

        rectTransform.anchoredPosition = GetItemPosition(index);
        rectTransform.SetSiblingIndex(index);
    }

    private void CreatePoolingObject()
    {
        int diff = _poolItemCount - _contentObject.transform.childCount;
        diff = Mathf.Min(diff, liData.Count); // �ʿ��� ��ŭ�� Ǯ�� ������
        if (diff > 0)
        {
            for (int i = 0; i < diff; ++i)
            {
                GameObject obj = Instantiate(_origin, _contentObject.transform);
                InitPoolingObject(obj.GetComponent<TItem>());
            }
        }
    }
}

}

