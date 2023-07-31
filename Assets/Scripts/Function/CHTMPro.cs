using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

[RequireComponent(typeof(TMP_Text))]
public class CHTMPro : MonoBehaviour
{
    [SerializeField] int stringID = -1;
    [SerializeField] public RectTransform rtText;
    [SerializeField] public TMP_Text text;
    [ReadOnly] object[] argArr;

    private void Awake()
    {
        rtText = GetComponent<RectTransform>();
        text = GetComponent<TMP_Text>();

        if (text)
        {
            if (stringID != -1)
            {
                text.text = CHMMain.String.GetString(stringID);
            }
        }
    }

    public void SetText(params object[] _arrArg)
    {
        this.argArr = _arrArg;
        if (text)
        {
            text.text = string.Format(CHMMain.String.GetString(stringID), _arrArg);
        }
    }

    public void SetColor(Color _color)
    {
        if (text)
        {
            text.color = _color;
        }
    }

    public void SetStringID(int _stringID)
    {
        this.argArr = null;
        stringID = _stringID;
        if (text)
        {
            text.text = CHMMain.String.GetString(stringID);
        }
    }

    public void SetPlusString(string _plusString)
    {
        if (text && string.IsNullOrEmpty(_plusString) == false)
        {
            text.text = text.text + " + " + _plusString;
        }
    }

    public string GetString()
    {
        return text.text;
    }
}
