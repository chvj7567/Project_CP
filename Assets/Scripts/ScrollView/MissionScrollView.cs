using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionScrollView : CHPoolingScrollView<MissionScrollViewItem, Infomation.MissionInfo>
{
    public override void InitItem(MissionScrollViewItem obj, Infomation.MissionInfo info, int index)
    {
        obj.Init(index, info);
    }

    public override void InitPoolingObject(MissionScrollViewItem obj)
    {
        base.InitPoolingObject(obj);
    }
}
