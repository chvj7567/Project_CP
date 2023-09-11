using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHDebugLog : MonoBehaviour
{
    public CHTMPro logText;
    [ReadOnly]
    int logCount = 0;
    Dictionary<string, GUIStyle> dicLogInfo = new Dictionary<string, GUIStyle>();

    private void Awake()
    {
        Application.logMessageReceived -= HandleLog;
        Application.logMessageReceived += HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        switch (type)
        {
            case LogType.Log:
                style.normal.textColor = Color.white;
                return;
            case LogType.Warning:
                return;
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                style.normal.textColor = Color.red;
                break;
        }

        logString = $"<{++logCount}>[{type}] : {logString} {stackTrace}";

        dicLogInfo.Add(logString, style);

        logText.SetPlusString(logString);
        logText.SetPlusString("\n");
    }
}
