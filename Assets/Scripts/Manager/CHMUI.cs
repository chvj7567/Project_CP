using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHUIArg
{
    public static readonly CHUIArg empty = new CHUIArg();
}

public class CHUIWaitData
{
    public int uid;
    public Defines.EUI uiType;
    public CHUIArg uiArg;
    public bool showCurrentBackground;
}

public class CHMUI
{
    static List<GameObject> activeUI = new List<GameObject>();
    static List<int> activeUID = new List<int>();

    static List<CHUIWaitData> waitActiveUI = new List<CHUIWaitData>();
    static List<GameObject> waitCloseUI = new List<GameObject>();

    static int uid = 0;
    static GameObject uiCamera = null;

    public void CreateEventSystemObject()
    {
        CHMMain.Resource.InstantiateUI(Defines.EUI.EventSystem);
    }

    public void UpdateUI()
    {
        if (waitCloseUI.Count != 0)
        {
            foreach (var waitData in waitCloseUI)
            {
                CloseWaitUI(waitData);
            }

            waitCloseUI.Clear();
        }

        if (waitActiveUI.Count != 0)
        {
            foreach (var waitData in waitActiveUI)
            {
                ShowUI(waitData);
            }

            waitActiveUI.Clear();
        }
    }

    public int ShowUI(Defines.EUI _uiType, CHUIArg _uiArg, bool _bCurrentBackground = true)
    {
        var uiWateData = new CHUIWaitData
        {
            uid = uid,
            uiType = _uiType,
            uiArg = _uiArg,
            showCurrentBackground = _bCurrentBackground,
        };

        activeUID.Add(uid);
        waitActiveUI.Add(uiWateData);

        return uid++;
    }

    void ShowUI(CHUIWaitData _uiWaitData)
    {
        GameObject uiCanvas = null;

        // 현재 화면 위에 UI를 보여줄지 별도의 화면에 보여줄지 확인
        if (_uiWaitData.showCurrentBackground)
        {
            uiCanvas = GameObject.FindGameObjectWithTag("UICanvas");

            if (uiCanvas == null)
            {
                CHMMain.Resource.InstantiateUI(Defines.EUI.UICanvas, (GameObject canvas) =>
                {
                    uiCanvas = canvas;
                });
            }
        }
        else
        {
            uiCanvas = GameObject.FindGameObjectWithTag("UICamera");

            if (uiCamera == null)
            {
                CHMMain.Resource.InstantiateUI(Defines.EUI.UICamera, (GameObject camera) =>
                {
                    uiCamera = camera;
                });
            }

            uiCamera.SetActive(true);

            if (uiCanvas == null)
            {
                CHMMain.Resource.InstantiateUI(Defines.EUI.UICanvas, (GameObject canvas) =>
                {
                    uiCanvas = canvas;
                });
            }
        }

        CHMMain.Resource.InstantiateUI(_uiWaitData.uiType, (GameObject uiObj) =>
        {
            if (uiObj)
            {
                uiObj.transform.SetParent(uiCanvas.transform);
                if (false == activeUID.Contains(_uiWaitData.uid))
                {
                    uiObj.SetActive(false);
                    CHMMain.Resource.Destroy(uiObj);
                    uiObj = null;
                }

                if (uiObj)
                {
                    activeUI.Add(uiObj);
                    var script = uiObj.GetComponent<UIBase>();
                    script.eUIType = _uiWaitData.uiType;
                    script.uid = _uiWaitData.uid;
                    script.InitUI(_uiWaitData.uiArg);
                    uiObj.SetActive(true);

                    var local = uiObj.transform.localPosition;
                    local.z = 0;
                    uiObj.transform.localPosition = local;
                }
            }
        });
    }

    static void CloseWaitUI(GameObject _uiObj)
    {
        if (_uiObj)
        {
            activeUI.Remove(_uiObj);
            _uiObj.SetActive(false);
            CHMMain.Resource.Destroy(_uiObj);
        }

        if (activeUI.IsNullOrEmpty())
        {
            if (uiCamera != null) uiCamera.SetActive(false);
        }
    }

    public void CloseUI(GameObject _uiObj)
    {
        if (_uiObj)
        {
            var popup = _uiObj.GetComponent<UIBase>();
            popup.CloseUI();
            waitCloseUI.Add(_uiObj);
        }
    }

    public void CloseUI(Defines.EUI _uiType)
    {
        activeUI = activeUI.FindAll(_ => _ != null);
        foreach (var obj in activeUI)
        {
            var ui = obj.GetComponent<UIBase>();
            if (ui.eUIType == _uiType)
            {
                ui.CloseUI();
                waitCloseUI.Add(obj);
            }
        }
    }
}
