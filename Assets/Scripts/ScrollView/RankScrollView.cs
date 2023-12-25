using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankScrollView : CHPoolingScrollView<RankScrollViewItem, Infomation.RankInfo>
{
    public override void InitItem(RankScrollViewItem obj, Infomation.RankInfo info, int index)
    {
        obj.Init(index, info);
    }

    public override void InitPoolingObject(RankScrollViewItem obj)
    {
        base.InitPoolingObject(obj);
    }
}
