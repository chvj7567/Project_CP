using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
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
            var info = CHMJson.Instance.GetGuideInfo(i + 1);
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
