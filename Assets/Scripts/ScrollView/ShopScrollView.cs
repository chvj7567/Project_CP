using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopScrollView : CHPoolingScrollView<ShopScrollViewItem, Infomation.ShopInfo>
{
    public override void InitItem(ShopScrollViewItem obj, Infomation.ShopInfo info, int index)
    {
        obj.Init(index, info);
    }

    public override void InitPoolingObject(ShopScrollViewItem obj)
    {
        base.InitPoolingObject(obj);
    }
}
