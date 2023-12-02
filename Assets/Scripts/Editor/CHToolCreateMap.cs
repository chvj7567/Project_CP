using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CHToolCreateMap : EditorWindow
{
    CHTool.StageInfoJson stageInfoJson = new CHTool.StageInfoJson();
    CHTool.StageBlockInfoJson stageBlockInfoJson = new CHTool.StageBlockInfoJson();

    int boardSize = 1;
    Texture emptyTexture = null;
    Texture texture = null;
    Texture[,] textures = new Texture[9, 9];
    int[,] hps = new int[9, 9];
    bool[,] tutorialBlocks = new bool[9, 9];

    List<Sprite> blockSpriteList = new List<Sprite>();
    int hp = -1;
    int tutorialID = -1;
    List<Infomation.StageInfo> stageInfoList = new List<Infomation.StageInfo>();
    List<Infomation.StageBlockInfo> stageBlockInfoList = new List<Infomation.StageBlockInfo>();
    int group;
    int stage;
    int blockTypeCount = 1;
    float time = -1f;
    int targetScore = -1;
    int moveCount = -1;

    [MenuItem("CHTools/Create Map")]
    static void ShowWindow()
    {
        var window = GetWindow(typeof(CHToolCreateMap));
        window.titleContent.text = "Single Window";
        window.minSize = new Vector2(650, 800);
        window.maxSize = new Vector2(650, 800);
    }

    private async void OnGUI()
    {
        if (blockSpriteList != null && blockSpriteList.Count <= 0)
        {
            for (Defines.EBlockState i = 0; i < Defines.EBlockState.Max; ++i)
            {
                CHTool.LoadAssetOnEditor<Sprite>(Defines.EResourceType.Sprite.ToString(), i.ToString(), (sprite) =>
                {
                    if (sprite != null)
                        blockSpriteList.Add(sprite);
                });
            }

            Debug.Log(blockSpriteList.Count);
        }

        if (stageInfoList != null && stageInfoList.Count <= 0)
        {
            CHTool.LoadAssetOnEditor<TextAsset>(Defines.EResourceType.Json.ToString(), Defines.EJsonType.Stage.ToString(), (textAsset) =>
            {
                var jsonData = JsonUtility.FromJson<CHMJson.JsonData>("{\"stageInfoArr\":" + textAsset.text + "}");
                foreach (var data in jsonData.stageInfoArr)
                {
                    stageInfoList.Add(data);
                }
            });

            Debug.Log(stageInfoList.Count);
        }

        if (stageBlockInfoList != null && stageBlockInfoList.Count <= 0)
        {
            CHTool.LoadAssetOnEditor<TextAsset>(Defines.EResourceType.Json.ToString(), Defines.EJsonType.StageBlock.ToString(), (textAsset) =>
            {
                var jsonData = JsonUtility.FromJson<CHMJson.JsonData>("{\"stageBlockInfoArr\":" + textAsset.text + "}");
                foreach (var data in jsonData.stageBlockInfoArr)
                {
                    stageBlockInfoList.Add(data);
                }
            });

            Debug.Log(stageBlockInfoList.Count);
        }

        boardSize = EditorGUILayout.IntSlider(boardSize, 2, 9);

        EditorGUIUtility.labelWidth = 30;

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("Group");
            group = EditorGUILayout.IntField(group);

            EditorGUILayout.LabelField("Stage");
            stage = EditorGUILayout.IntField(stage);

        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("BlockTypeCount");
            blockTypeCount = EditorGUILayout.IntField(blockTypeCount);

            EditorGUILayout.LabelField("BoardSize");
            boardSize = EditorGUILayout.IntField(boardSize);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("Time");
            time = EditorGUILayout.FloatField(time);

            EditorGUILayout.LabelField("TargetScore");
            targetScore = EditorGUILayout.IntField(targetScore);

            EditorGUILayout.LabelField("MoveCount");
            moveCount = EditorGUILayout.IntField(moveCount);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("MoveCount");
            moveCount = EditorGUILayout.IntField(moveCount);

            EditorGUILayout.LabelField("HP 지정");
            hp = EditorGUILayout.IntField(hp);

            EditorGUILayout.LabelField("튜토리얼 ID");
            tutorialID = EditorGUILayout.IntField(tutorialID);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUIUtility.labelWidth = 30;

            EditorGUILayout.LabelField("생성할 블럭");

            if (GUILayout.Button(texture, GUILayout.Width(100), GUILayout.Height(100)))
            {
                
            }

            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.Cat1].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.Cat1].texture;
                    }

                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.Cat2].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.Cat2].texture;
                    }

                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.Cat3].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.Cat3].texture;
                    }

                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.Cat4].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.Cat4].texture;
                    }

                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.Cat5].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.Cat5].texture;
                    }

                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.CatPang].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.CatPang].texture;
                    }

                    if (GUILayout.Button(emptyTexture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = null;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.Arrow1].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.Arrow1].texture;
                    }

                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.Arrow2].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.Arrow2].texture;
                    }

                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.Arrow3].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.Arrow3].texture;
                    }

                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.Arrow4].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.Arrow4].texture;
                    }

                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.Arrow5].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.Arrow5].texture;
                    }

                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.Arrow6].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.Arrow6].texture;
                    }

                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.Potal].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.Potal].texture;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.PinkBomb].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.PinkBomb].texture;
                    }

                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.YellowBomb].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.YellowBomb].texture;
                    }

                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.OrangeBomb].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.OrangeBomb].texture;
                    }

                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.GreenBomb].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.GreenBomb].texture;
                    }

                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.BlueBomb].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.BlueBomb].texture;
                    }

                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.Fish].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.Fish].texture;
                    }

                    if (GUILayout.Button(blockSpriteList[(int)Defines.EBlockState.Wall].texture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = blockSpriteList[(int)Defines.EBlockState.Wall].texture;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            /*if (GUILayout.Button(texture, GUILayout.Width(50), GUILayout.Height(50)))
            {
                if (texture == null)
                    texture = blockSpriteList[(int)Defines.EBlockState.Cat1].texture;
                else if (texture == blockSpriteList[(int)Defines.EBlockState.Cat1].texture)
                    texture = blockSpriteList[(int)Defines.EBlockState.Cat2].texture;
                else if (texture == blockSpriteList[(int)Defines.EBlockState.Cat2].texture)
                    texture = blockSpriteList[(int)Defines.EBlockState.Cat3].texture;
                else if (texture == blockSpriteList[(int)Defines.EBlockState.Cat3].texture)
                    texture = blockSpriteList[(int)Defines.EBlockState.Cat4].texture;
                else if (texture == blockSpriteList[(int)Defines.EBlockState.Cat4].texture)
                    texture = blockSpriteList[(int)Defines.EBlockState.Cat5].texture;
                else if (texture == blockSpriteList[(int)Defines.EBlockState.Cat5].texture)
                    texture = blockSpriteList[(int)Defines.EBlockState.CatPang].texture;
                else if (texture == blockSpriteList[(int)Defines.EBlockState.CatPang].texture)
                    texture = blockSpriteList[(int)Defines.EBlockState.PinkBomb].texture;
                else if (texture == blockSpriteList[(int)Defines.EBlockState.PinkBomb].texture)
                    texture = blockSpriteList[(int)Defines.EBlockState.YellowBomb].texture;
                else if (texture == blockSpriteList[(int)Defines.EBlockState.YellowBomb].texture)
                    texture = blockSpriteList[(int)Defines.EBlockState.OrangeBomb].texture;
                else if (texture == blockSpriteList[(int)Defines.EBlockState.OrangeBomb].texture)
                    texture = blockSpriteList[(int)Defines.EBlockState.GreenBomb].texture;
                else if (texture == blockSpriteList[(int)Defines.EBlockState.GreenBomb].texture)
                    texture = blockSpriteList[(int)Defines.EBlockState.BlueBomb].texture;
                else if (texture == blockSpriteList[(int)Defines.EBlockState.BlueBomb].texture)
                    texture = blockSpriteList[(int)Defines.EBlockState.Wall].texture;
                else if (texture == blockSpriteList[(int)Defines.EBlockState.Wall].texture)
                    texture = blockSpriteList[(int)Defines.EBlockState.Potal].texture;
                else if (texture == blockSpriteList[(int)Defines.EBlockState.Potal].texture)
                    texture = blockSpriteList[(int)Defines.EBlockState.Fish].texture;
                else if (texture == blockSpriteList[(int)Defines.EBlockState.Fish].texture)
                    texture = null;
            }*/
        }
        EditorGUILayout.EndHorizontal();

        for (int w = 0; w < boardSize; w++)
        {
            EditorGUILayout.BeginHorizontal();
            {
                for (int h = 0; h < boardSize; h++)
                {
                    if (GUILayout.Button(textures[w, h], GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        textures[w, h] = texture;
                        hps[w, h] = hp;
                        tutorialBlocks[w, h] = tutorialID > 0;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("판 리셋", GUILayout.Width(595), GUILayout.Height(30)))
        {
            for (int w = 0; w < 9; w++)
            {
                for (int h = 0; h < 9; h++)
                {
                    textures[w, h] = null;
                    hps[w, h] = -1;
                    tutorialBlocks[w, h] = false;
                }
            }
        }

        if (GUILayout.Button("불러오기(Stage 값 기준)", GUILayout.Width(595), GUILayout.Height(30)))
        {
            for (int w = 0; w < 9; w++)
            {
                for (int h = 0; h < 9; h++)
                {
                    textures[w, h] = null;
                    hps[w, h] = -1;
                    tutorialBlocks[w, h] = false;
                }
            }

            var stageInfo = stageInfoList.Find(_ => _.stage == stage);
            var stageBlockInfo = stageBlockInfoList.FindAll(_ => _.stage == stage);
            if (stageInfo != null && stageBlockInfo != null)
            {
                group = stageInfo.group;
                blockTypeCount = stageInfo.blockTypeCount;
                boardSize = stageInfo.boardSize;
                time = stageInfo.time;
                targetScore = stageInfo.targetScore;
                moveCount = stageInfo.moveCount;
                tutorialID = stageInfo.tutorialID;

                for (int w = 0; w < boardSize; w++)
                {
                    for (int h = 0; h < boardSize; h++)
                    {
                        var findBlock = stageBlockInfo.Find(_ => _.row == w && _.col == h);
                        if (findBlock == null)
                        {
                            textures[w, h] = null;
                            hps[w, h] = -1;
                            tutorialBlocks[w, h] = false;
                        }
                        else
                        {
                            textures[w, h] = blockSpriteList[(int)findBlock.blockState].texture;
                            hps[w, h] = findBlock.hp;
                            tutorialBlocks[w, h] = findBlock.tutorialBlock;
                        }
                    }
                }
            }
        }

        if (GUILayout.Button("저장하기", GUILayout.Width(595), GUILayout.Height(30)))
        {
            var stageInfo = stageInfoList.Find(_ => _.stage == stage);
            if (stageInfo == null)
            {
                stageInfoList.Add(new Infomation.StageInfo
                {
                    group = group,
                    stage = stage,
                    blockTypeCount = blockTypeCount,
                    boardSize = boardSize,
                    time = time,
                    targetScore = targetScore,
                    moveCount = moveCount,
                    tutorialID = tutorialID,
                });
            }
            else
            {
                stageInfo.group = group;
                stageInfo.stage = stage;
                stageInfo.blockTypeCount = blockTypeCount;
                stageInfo.boardSize = boardSize;
                stageInfo.time = time;
                stageInfo.targetScore = targetScore;
                stageInfo.moveCount = moveCount;
                stageInfo.tutorialID = tutorialID;
            }

            stageBlockInfoList.RemoveAll(_ => _.stage == stage);

            for (int w = 0; w < boardSize; w++)
            {
                for (int h = 0; h < boardSize; h++)
                {
                    if (textures[w, h] == null)
                        continue;

                    stageBlockInfoList.Add(new Infomation.StageBlockInfo
                    {
                        stage = stage,
                        blockState = GetBlockState(textures[w, h]),
                        hp = hps[w, h],
                        row = w,
                        col = h,
                        tutorialBlock = tutorialBlocks[w, h],
                    });
                }
            }

            stageInfoJson.stageList = stageInfoList.OrderBy(_ => _.stage).ToList();
            stageBlockInfoJson.stageBlockList = stageBlockInfoList.OrderBy(_ => _.stage).ToList();

            string jsonData = JsonUtility.ToJson(stageInfoJson, true);

            var splitData = jsonData.Split('[', ']');

            jsonData = "[" + splitData[1] + "]";
            // JSON 파일로 저장 (Assets 폴더 내에 저장됨)
            string filePath = Path.Combine(Application.dataPath + "/AssetBundleResources/json", "Stage.json");
            File.WriteAllText(filePath, jsonData);

            jsonData = JsonUtility.ToJson(stageBlockInfoJson, true);

            splitData = jsonData.Split('[', ']');

            jsonData = "[" + splitData[1] + "]";

            // JSON 파일로 저장 (Assets 폴더 내에 저장됨)
            filePath = Path.Combine(Application.dataPath + "/AssetBundleResources/json", "StageBlock.json");
            File.WriteAllText(filePath, jsonData);
        }
    }

    Texture2D MakeBackgroundTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        Texture2D backgroundTexture = new Texture2D(width, height);

        backgroundTexture.SetPixels(pixels);
        backgroundTexture.Apply();

        return backgroundTexture;
    }

    Defines.EBlockState GetBlockState(Texture texture)
    {
        if (texture == blockSpriteList[(int)Defines.EBlockState.Cat1].texture)
            return Defines.EBlockState.Cat1;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.Cat2].texture)
            return Defines.EBlockState.Cat2;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.Cat3].texture)
            return Defines.EBlockState.Cat3;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.Cat4].texture)
            return Defines.EBlockState.Cat5;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.Cat5].texture)
            return Defines.EBlockState.Cat5;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.CatPang].texture)
            return Defines.EBlockState.CatPang;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.PinkBomb].texture)
            return Defines.EBlockState.PinkBomb;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.YellowBomb].texture)
            return Defines.EBlockState.YellowBomb;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.OrangeBomb].texture)
            return Defines.EBlockState.OrangeBomb;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.GreenBomb].texture)
            return Defines.EBlockState.GreenBomb;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.BlueBomb].texture)
            return Defines.EBlockState.BlueBomb;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.Wall].texture)
            return Defines.EBlockState.Wall;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.Potal].texture)
            return Defines.EBlockState.Potal;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.Fish].texture)
            return Defines.EBlockState.Fish;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.Arrow1].texture)
            return Defines.EBlockState.Arrow1;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.Arrow2].texture)
            return Defines.EBlockState.Arrow2;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.Arrow3].texture)
            return Defines.EBlockState.Arrow3;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.Arrow4].texture)
            return Defines.EBlockState.Arrow4;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.Arrow5].texture)
            return Defines.EBlockState.Arrow5;
        else if (texture == blockSpriteList[(int)Defines.EBlockState.Arrow6].texture)
            return Defines.EBlockState.Arrow6;

        return Defines.EBlockState.None;
    }
}
