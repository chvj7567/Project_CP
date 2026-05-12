using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class CHTMPro : MonoBehaviour
{
    [SerializeField] int stringID = 1;
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
                text.text = CHMString.Instance.GetString(stringID);

                var loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);

                CHMResource.Instance.LoadFont(loginData.languageType, (font) =>
                {
                    text.font = font;
                });
            }
        }
    }

    public void SetText(params object[] _arrArg)
    {
        this.argArr = _arrArg;
        if (text)
        {
            text.text = string.Format(CHMString.Instance.GetString(stringID), _arrArg);

            if (int.TryParse(text.text, out var intValue))
            {
                // 숫자일 경우 영어 폰트로
                CHMResource.Instance.LoadFont(Defines.ELanguageType.English, (font) =>
                {
                    text.font = font;
                });
            }
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
            text.text = CHMString.Instance.GetString(stringID);
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
