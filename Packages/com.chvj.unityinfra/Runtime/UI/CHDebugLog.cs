using System.Collections.Generic;
using UnityEngine;

namespace ChvjUnityInfra
{
    public class CHDebugLog : MonoBehaviour
    {
        public CHText logText;

        [ReadOnly]
        private int _logCount = 0;
        private Dictionary<string, GUIStyle> _dicLogInfo = new Dictionary<string, GUIStyle>();

        private void Awake()
        {
            Application.logMessageReceived -= HandleLog;
            Application.logMessageReceived += HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            switch (type)
            {
                case LogType.Log:
                    style.normal.textColor = Color.white;
                    break;
                case LogType.Warning:
                    return;
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    style.normal.textColor = Color.red;
                    break;
            }

            logString = $"<{++_logCount}>[{type}] : {logString}";

            _dicLogInfo.Add(logString, style);

            if (logText != null)
            {
                logText.SetPlusString(logString);
                logText.SetPlusString("\n");
            }
        }
    }
}
