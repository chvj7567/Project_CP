using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UIAlarmArg : CHUIArg
{
    public float closeTime = 2f;
    public int stringID;
    public int intValue;
}

public class UIAlarm : UIBase
{
    UIAlarmArg arg;

    [SerializeField] CHTMPro alarmText;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIAlarmArg;
    }

    CancellationTokenSource delayTokenSource;

    private async void Start()
    {
        alarmText.SetStringID(arg.stringID);
        alarmText.SetText(arg.intValue);

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
            CHMMain.UI.CloseUI(gameObject);
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
