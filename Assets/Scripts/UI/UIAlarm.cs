using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using ChvjUnityInfra;
public class UIAlarmArg : CHUIArg
{
    public bool useStringID = true;
    public string text = "";
    public float closeTime = 2f;
    public int stringID;
    public int intValue;
}

public class UIAlarm : UIBase
{
    UIAlarmArg arg;

    [SerializeField] CHText alarmText;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIAlarmArg;
    }

    CancellationTokenSource delayTokenSource;

    private async void Start()
    {
        // InitUI 없이 Start가 호출되는 경합(씬 전환 직전 ShowUI 등)에서 self-destruct.
        if (arg == null)
        {
            CHMUI.Instance.CloseUI(gameObject);
            return;
        }

        if (arg.useStringID)
        {
            alarmText.SetStringID(arg.stringID);
            alarmText.SetText(arg.intValue);
        }
        else
        {
            alarmText.SetStringID(-1);
            alarmText.SetText(arg.text);
        }

        delayTokenSource = new CancellationTokenSource();

        try
        {
            await Task.Delay((int)(arg.closeTime * 1000), delayTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        if (gameObject != null)
        {
            CHMUI.Instance.CloseUI(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (delayTokenSource != null && !delayTokenSource.IsCancellationRequested)
        {
            delayTokenSource.Cancel();
        }
    }
}
