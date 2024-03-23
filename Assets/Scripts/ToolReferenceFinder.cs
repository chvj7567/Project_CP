using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class ToolReferenceFinder : EditorWindow
{
    enum Form
    {
        Object,
        Component
    }

    class ReferenceInfo
    {
        public Form form;
        public string strObject;
        public string strComponent;
    }

    static Object findObject;

    static string inputScenePath = "";

    
    static List<bool> liSceneExpand = new List<bool>();
    static List<bool> liObjectExpand = new List<bool>();
    static List<bool> liComponentExpand = new List<bool>();

    static List<string> liSceneName = new List<string>();
    static List<string> liScenePath = new List<string>();

    static List<string> liObjectName = new List<string>();
    static List<string> liObjectPath = new List<string>();

    static Dictionary<string, Dictionary<Form, List<ReferenceInfo>>> dicSceneReferenceInfo = new Dictionary<string, Dictionary<Form, List<ReferenceInfo>>>();
    static Dictionary<string, int> dicObjectInfo = new Dictionary<string, int>();

    static Vector2 scrollPosition = Vector2.zero;

    static bool IsEffect { get; set; }
    static bool IsPrefab { get; set; }
    static string SceneName { get; set; }

    [MenuItem("CHTool/Reference Object Finder")]
    public static void ShowSingleWindow()
    {
        // GetWindow
        var window = EditorWindow.GetWindow(typeof(ToolReferenceFinder));

        // 윈도우 타이틀 지정
        window.titleContent.text = "Tool Reference Object Finder";

        // 윈도우 최소, 최대 사이즈 지정
        window.minSize = new Vector2(300f, 300f);
        window.maxSize = new Vector2(500f, 1000f);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("찾을 씬 경로 Assets 포함하여 작성하며 2개 이상은 Enter로 구분하여 작성");
        EditorGUILayout.LabelField("(빈 칸이면 모든 경로 확인하며 시간 오래 걸림)");
        inputScenePath = EditorGUILayout.TextArea(inputScenePath);

        EditorGUILayout.LabelField("찾을 오브젝트");
        findObject = EditorGUILayout.ObjectField("Find Object", findObject, typeof(Object), true);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("이펙트이면 체크");
        IsEffect = EditorGUILayout.Toggle(IsEffect);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("모든 씬의 참조 찾기", GUILayout.ExpandWidth(true), GUILayout.Height(50)))
        {
            if (findObject == null)
                return;

            if (IsEffect && findObject is GameObject)
            {
                GameObject gameObject = findObject as GameObject;
                ParticleSystem effect = gameObject.GetComponent<ParticleSystem>();
                findObject = effect == null ? findObject : effect;
            }

            Init();
            FindScene();
        }

        if (GUILayout.Button("모든 오브젝트의 참조 찾기", GUILayout.ExpandWidth(true), GUILayout.Height(50)))
        {
            if (findObject == null)
                return;

            if (IsEffect && findObject is GameObject)
            {
                GameObject gameObject = findObject as GameObject;
                ParticleSystem effect = gameObject.GetComponent<ParticleSystem>();
                findObject = effect == null ? findObject : effect;
            }

            Init();
            FindObject();
        }

        int index = 0;

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach (var info in dicSceneReferenceInfo)
        {
            int totalSceneReferenceCount = info.Value[Form.Object].Count + info.Value[Form.Component].Count;
            if (totalSceneReferenceCount <= 0)
                continue;
            EditorGUILayout.BeginHorizontal();
            liSceneExpand[index] = EditorGUILayout.Foldout(liSceneExpand[index], $"{info.Key} ({totalSceneReferenceCount})", true);
            if (GUILayout.Button("Copy"))
            {
                Copy(info.Key);
            }
            EditorGUILayout.EndHorizontal();
            if (liSceneExpand[index])
            {
                EditorGUI.indentLevel++;

                liObjectExpand[index] = EditorGUILayout.Foldout(liObjectExpand[index], $"{Form.Object} ({info.Value[Form.Object].Count})", true);
                if (liObjectExpand[index])
                {
                    EditorGUI.indentLevel++;
                    foreach (var obj in info.Value[Form.Object])
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"{obj.strObject}");
                        if (GUILayout.Button("Copy"))
                        {
                            Copy(obj.strObject);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                }

                liComponentExpand[index] = EditorGUILayout.Foldout(liComponentExpand[index], $"{Form.Component} ({info.Value[Form.Component].Count})", true);
                if (liComponentExpand[index])
                {
                    EditorGUI.indentLevel++;
                    foreach (var comp in info.Value[Form.Component])
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"{comp.strObject} / {comp.strComponent}");
                        if (GUILayout.Button("Copy"))
                        {
                            Copy(comp.strObject);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }

            ++index;
        }

        foreach (var info in dicObjectInfo)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{info.Key} => {info.Value}개의 참조");
            if (GUILayout.Button("Copy"))
            {
                Copy(info.Key);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    static void Init()
    {
        IsPrefab = PrefabUtility.IsPartOfAnyPrefab(findObject);

        liSceneName.Clear();
        liScenePath.Clear();
        dicSceneReferenceInfo.Clear();

        liObjectName.Clear();
        liObjectPath.Clear();
        dicObjectInfo.Clear();

        FindScenePath();

        foreach (var sceneName in liSceneName)
        {
            var dict = new Dictionary<Form, List<ReferenceInfo>>
            {
                {
                    Form.Object,
                    new List<ReferenceInfo>()
                },
                {
                    Form.Component,
                    new List<ReferenceInfo>()
                }
            };

            if (dicSceneReferenceInfo.ContainsKey(sceneName) == false)
            {
                dicSceneReferenceInfo.Add(sceneName, dict);
            }
        }

        foreach (var info in dicSceneReferenceInfo)
        {
            liSceneExpand.Add(false);
            liObjectExpand.Add(false);
            liComponentExpand.Add(false);
        }
    }

    static void FindScene()
    {
        string pathOriginScene = SceneManager.GetActiveScene().path;

        for (int i = 0; i < liScenePath.Count; i++)
        {
            SceneName = liSceneName[i];

            Debug.Log($"@@{liSceneName[i]}/{liScenePath[i]}");
            EditorSceneManager.OpenScene(liScenePath[i]);

            var scene = SceneManager.GetActiveScene();
            GameObject[] arrGameObject = scene.GetRootGameObjects();

            List<Transform> addObject = new List<Transform>();

            foreach (GameObject obj in arrGameObject)
            {
                var allChild = obj.transform.GetComponentsInChildren<Transform>();
                addObject.AddRange(allChild);
            }

            foreach (Transform obj in addObject)
            {
                if (IsPrefab && PrefabUtility.IsPartOfAnyPrefab(obj) && obj.name.Contains(findObject.name))
                {
                    AddReferenceInfo(new ReferenceInfo
                    {
                        form = Form.Object,
                        strObject = obj.name
                    });
                }
            }

            FindReferences(findObject, addObject);
        }

        EditorSceneManager.OpenScene(pathOriginScene);
    }

    static void FindObject()
    {
        for (int i = 0; i < liObjectPath.Count; i++)
        {
            string assetPath = liObjectPath[i];
            string[] arrDependency = AssetDatabase.GetDependencies(assetPath, false);

            foreach (string depend in arrDependency)
            {
                string dependName = depend.Split('/').Last().Split('.')[0];
                if (dependName == findObject.name)
                {
                    if (dicObjectInfo.TryGetValue(liObjectName[i], out var value))
                    {
                        value += 1;
                    }
                    else
                    {
                        dicObjectInfo.Add(liObjectName[i], 1);
                    }
                }
            }
        }
        
    }

    static void FindReferences(Object to, List<Transform> allObjects)
    {
        var referencedBy = new List<Object>();

        for (int i = 0; i < allObjects.Count; i++)
        {
            var go = allObjects[i];

            var components = go.GetComponents<Component>();
            for (int j = 0; j < components.Length; j++)
            {
                var c = components[j];
                if (c == null)
                    continue;

                var so = new SerializedObject(c);
                var sp = so.GetIterator();

                while (sp.NextVisible(true))
                {
                    if (sp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (sp.objectReferenceValue == to)
                        {
                            AddReferenceInfo(new ReferenceInfo
                            {
                                form = Form.Component,
                                strObject = c.name,
                                strComponent = c.GetType().ToString(),
                            });

                            referencedBy.Add(c.gameObject);
                        }
                    }
                }
            }
        }
    }

    static void AddReferenceInfo(ReferenceInfo referenceInfo)
    {
        if (dicSceneReferenceInfo.TryGetValue(SceneName, out var value))
        {
            if (value.TryGetValue(referenceInfo.form, out var value2))
            {
                value2.Add(referenceInfo);
            }
            else
            {
                var list = new List<ReferenceInfo>
                {
                    referenceInfo
                };

                value.Add(referenceInfo.form, list);
            }
        }
        else
        {
            var dict = new Dictionary<Form, List<ReferenceInfo>>
            {
                {
                    Form.Object,
                    new List<ReferenceInfo>()
                },
                {
                    Form.Component,
                    new List<ReferenceInfo>()
                }
            };

            if (dict.TryGetValue(referenceInfo.form, out var value3))
            {
                value3.Add(referenceInfo);
            }

            dicSceneReferenceInfo.Add(SceneName, dict);
        }
    }

    public static void FindScenePath()
    {
        inputScenePath = inputScenePath.Trim();
        var arrPath = inputScenePath.Split('\n');

        string[] guids;

        if (inputScenePath == "" || arrPath.Length == 0)
        {
            guids = AssetDatabase.FindAssets("", new[] { "Assets" });
        }
        else
        {
            guids = AssetDatabase.FindAssets("", arrPath);
        }

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            if (assetPath.EndsWith(".unity"))
            {
                liScenePath.Add(assetPath);

                var split = assetPath.Split('/');
                liSceneName.Add(split[split.Length - 1].Split('.')[0]);
            }

            if (assetPath.EndsWith(".asset") || assetPath.EndsWith(".prefab"))
            {
                liObjectPath.Add(assetPath);

                var split = assetPath.Split('/');
                liObjectName.Add(split[split.Length - 1].Split('.')[0]);
            }
        }
    }

    public static void FindReference2()
    {
        var referenceCache = new Dictionary<string, List<string>>();

        string[] guids = AssetDatabase.FindAssets("", new[] { "Assets" });
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string[] dependencies = AssetDatabase.GetDependencies(assetPath, false);

            foreach (var dependency in dependencies)
            {
                if (referenceCache.ContainsKey(dependency))
                {
                    if (!referenceCache[dependency].Contains(assetPath))
                    {
                        referenceCache[dependency].Add(assetPath);
                    }
                }
                else
                {
                    referenceCache[dependency] = new List<string>() { assetPath };
                }
            }
        }

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        Debug.Log("===== Find: " + path, Selection.activeObject);
        if (referenceCache.ContainsKey(path))
        {
            foreach (var reference in referenceCache[path])
            {
                Debug.Log(reference, AssetDatabase.LoadMainAssetAtPath(reference));
            }
        }
        else
        {
            Debug.LogWarning("No References");
        }
        referenceCache.Clear();
    }

    static void Copy(string copyText)
    {
        TextEditor editor = new TextEditor
        {
            text = copyText
        };

        editor.SelectAll();
        editor.Copy();
    }
}
