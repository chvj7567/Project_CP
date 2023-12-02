using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static CHMJson;

public class CHToolString : EditorWindow
{
    CHTool.StringKoreaInfoJson stringKoreaInfoJson = new CHTool.StringKoreaInfoJson();
    CHTool.StringEnglishInfoJson stringEnglishInfoJson = new CHTool.StringEnglishInfoJson();

    int stringID = -1;
    string korea;
    string english;

    List<Infomation.StringInfo> stringKoreaList = new List<Infomation.StringInfo>();
    List<Infomation.StringInfo> stringEnglishList = new List<Infomation.StringInfo>();

    [MenuItem("CHTools/String Tool")]
    static void ShowWindow()
    {
        var window = GetWindow(typeof(CHToolString));
        window.titleContent.text = "Single Window";
        window.minSize = new Vector2(650, 800);
        window.maxSize = new Vector2(650, 800);
    }

    private void OnGUI()
    {
        if (stringKoreaList != null && stringKoreaList.Count <= 0)
        {
            CHTool.LoadAssetOnEditor<TextAsset>(Defines.EResourceType.Json.ToString(), Defines.EJsonType.StringKorea.ToString(), (textAsset) =>
            {
                var jsonData = JsonUtility.FromJson<JsonData>("{\"stringKoreaInfoArr\":" + textAsset.text + "}");
                foreach (var data in jsonData.stringKoreaInfoArr)
                {
                    stringKoreaList.Add(data);
                }
            });

            Debug.Log(stringKoreaList.Count);
        }

        if (stringEnglishList != null && stringEnglishList.Count <= 0)
        {
            CHTool.LoadAssetOnEditor<TextAsset>(Defines.EResourceType.Json.ToString(), Defines.EJsonType.StringEnglish.ToString(), (textAsset) =>
            {
                var jsonData = JsonUtility.FromJson<JsonData>("{\"stringEnglishInfoArr\":" + textAsset.text + "}");
                foreach (var data in jsonData.stringEnglishInfoArr)
                {
                    stringEnglishList.Add(data);
                }
            });

            Debug.Log(stringEnglishList.Count);
        }

        EditorGUIUtility.labelWidth = 10;

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("StringID");
            stringID = EditorGUILayout.IntField(stringID);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Korea");
        korea = EditorGUILayout.TextArea(korea);

        EditorGUILayout.LabelField("English");
        english = EditorGUILayout.TextArea(english);

        if (GUILayout.Button("불러오기(Stage 값 기준)", GUILayout.Width(595), GUILayout.Height(30)))
        {
            var stringInfo = stringKoreaList.Find(_ => _.stringID == stringID);
            if (stringInfo != null)
            {
                korea = stringInfo.value;
            }

            var stringInfo2 = stringEnglishList.Find(_ => _.stringID == stringID);
            if (stringInfo2 != null)
            {
                english = stringInfo2.value;
            }
        }

        if (GUILayout.Button("저장하기", GUILayout.Width(595), GUILayout.Height(30)))
        {
            var stringInfo = stringKoreaList.Find(_ => _.stringID == stringID);
            if (stringInfo != null)
            {
                stringInfo.value = korea;
            }
            else
            {
                stringKoreaList.Add(new Infomation.StringInfo
                {
                    stringID = stringID,
                    value = korea
                });
            }

            var stringInfo2 = stringEnglishList.Find(_ => _.stringID == stringID);
            if (stringInfo2 != null)
            {
                stringInfo2.value = english;
            }
            else
            {
                stringEnglishList.Add(new Infomation.StringInfo
                {
                    stringID = stringID,
                    value = english
                });
            }

            stringKoreaInfoJson.stringList = stringKoreaList.OrderBy(_ => _.stringID).ToList();
            stringEnglishInfoJson.stringList = stringEnglishList.OrderBy(_ => _.stringID).ToList();

            string jsonData = JsonUtility.ToJson(stringKoreaInfoJson, true);

            var splitData = jsonData.Split('[', ']');

            jsonData = "[" + splitData[1] + "]";
            // JSON 파일로 저장 (Assets 폴더 내에 저장됨)
            string filePath = Path.Combine(Application.dataPath + "/AssetBundleResources/json", "StringKorea.json");
            File.WriteAllText(filePath, jsonData);

            stringEnglishList = stringEnglishList.OrderBy(_ => _.stringID).ToList();
            jsonData = JsonUtility.ToJson(stringEnglishInfoJson, true);

            splitData = jsonData.Split('[', ']');

            jsonData = "[" + splitData[1] + "]";

            // JSON 파일로 저장 (Assets 폴더 내에 저장됨)
            filePath = Path.Combine(Application.dataPath + "/AssetBundleResources/json", "StringEnglish.json");
            File.WriteAllText(filePath, jsonData);
        }
    }
}