using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Defines;

public class CHToolCreateMap : EditorWindow
{
    CHTool.StageInfoJson stageInfoJson = new CHTool.StageInfoJson();
    CHTool.StageBlockInfoJson stageBlockInfoJson = new CHTool.StageBlockInfoJson();

    int boardSize = 1;
    Texture emptyTexture = null;
    Texture texture = null;
    Texture[,] textures = new Texture[9, 9];
    Defines.EBlockState blockState = Defines.EBlockState.None;
    Defines.EBlockState[,] blockStates = new Defines.EBlockState[9, 9];
    int[,] hps = new int[9, 9];
    bool[,] tutorialBlocks = new bool[9, 9];

    List<Sprite> blockSpriteList = new List<Sprite>();
    int hp = -1;
    int tutorialID = -1;
    List<Infomation.StageInfo> stageInfoList = new List<Infomation.StageInfo>();
    List<Infomation.StageBlockInfo> stageBlockInfoList = new List<Infomation.StageBlockInfo>();
    bool bossStage = false;
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
        window.minSize = new Vector2(650, 950);
        window.maxSize = new Vector2(650, 950);
    }

    private async void OnGUI()
    {
        if (blockSpriteList != null && blockSpriteList.Count <= 0)
        {
            for (Defines.EBlockState i = 0; i < Defines.EBlockState.Max; ++i)
            {
                CHStatic.LoadAssetOnEditor<Sprite>(Defines.EResourceType.Sprite.ToString(), i.ToString(), (sprite) =>
                {
                    blockSpriteList.Add(sprite);
                });
            }

            Debug.Log(blockSpriteList.Count);
        }

        if (stageInfoList != null && stageInfoList.Count <= 0)
        {
            CHStatic.LoadAssetOnEditor<TextAsset>(Defines.EResourceType.Json.ToString(), Defines.EJsonType.Stage.ToString(), (textAsset) =>
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
            CHStatic.LoadAssetOnEditor<TextAsset>(Defines.EResourceType.Json.ToString(), Defines.EJsonType.StageBlock.ToString(), (textAsset) =>
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
            EditorGUILayout.LabelField("BossStage");
            bossStage = EditorGUILayout.Toggle(bossStage);

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
                    CreateBlockButton(Defines.EBlockState.Cat1);
                    CreateBlockButton(Defines.EBlockState.Cat2);
                    CreateBlockButton(Defines.EBlockState.Cat3);
                    CreateBlockButton(Defines.EBlockState.Cat4);
                    CreateBlockButton(Defines.EBlockState.Cat5);
                    CreateBlockButton(Defines.EBlockState.CatPang);

                    if (GUILayout.Button(emptyTexture, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        texture = null;
                        blockState = Defines.EBlockState.None;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    CreateBlockButton(Defines.EBlockState.Arrow1);
                    CreateBlockButton(Defines.EBlockState.Arrow2);
                    CreateBlockButton(Defines.EBlockState.Arrow3);
                    CreateBlockButton(Defines.EBlockState.Arrow4);
                    CreateBlockButton(Defines.EBlockState.Arrow5);
                    CreateBlockButton(Defines.EBlockState.Arrow6);
                    CreateBlockButton(Defines.EBlockState.Potal);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    CreateBlockButton(Defines.EBlockState.PinkBomb);
                    CreateBlockButton(Defines.EBlockState.YellowBomb);
                    CreateBlockButton(Defines.EBlockState.OrangeBomb);
                    CreateBlockButton(Defines.EBlockState.GreenBomb);
                    CreateBlockButton(Defines.EBlockState.BlueBomb);
                    CreateBlockButton(Defines.EBlockState.Fish);
                    CreateBlockButton(Defines.EBlockState.Wall);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    CreateBlockButton(Defines.EBlockState.CatBox1);
                    CreateBlockButton(Defines.EBlockState.CatBox2);
                    CreateBlockButton(Defines.EBlockState.CatBox3);
                    CreateBlockButton(Defines.EBlockState.CatBox4);
                    CreateBlockButton(Defines.EBlockState.CatBox5);
                    CreateBlockButton(Defines.EBlockState.WallCreator);
                    CreateBlockButton(Defines.EBlockState.PotalCreator);
                    CreateBlockButton(Defines.EBlockState.RainbowPang);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
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
                        blockStates[w, h] = blockState;
                        hps[w, h] = hp;
                        tutorialBlocks[w, h] = tutorialID > 0;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("판 리셋", GUILayout.Width(595), GUILayout.Height(30)))
        {
            ResetBoard();
        }

        if (GUILayout.Button("불러오기(Stage 값 기준)", GUILayout.Width(595), GUILayout.Height(30)))
        {
            ResetBoard();

            int tempGroup = 0;
            int tempStage = 0;
            if (bossStage)
            {
                tempGroup = CHStatic.BossStageStartValue + group;
                tempStage = CHStatic.BossStageStartValue + stage;
            }
            else
            {
                tempGroup = group;
                tempStage = stage;
            }

            var stageInfo = stageInfoList.Find(_ => _.stage == tempStage);
            var stageBlockInfo = stageBlockInfoList.FindAll(_ => _.stage == tempStage);
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
                            blockStates[w, h] = Defines.EBlockState.None;
                            hps[w, h] = -1;
                            tutorialBlocks[w, h] = false;
                        }
                        else
                        {
                            if (blockSpriteList[(int)findBlock.blockState] == null)
                                textures[w, h] = MakeBackgroundTexture(50, 50, Color.white);
                            else
                                textures[w, h] = blockSpriteList[(int)findBlock.blockState].texture;
                            blockStates[w, h] = findBlock.blockState;
                            hps[w, h] = findBlock.hp;
                            tutorialBlocks[w, h] = findBlock.tutorialBlock;
                        }
                    }
                }
            }
        }

        if (GUILayout.Button("저장하기", GUILayout.Width(595), GUILayout.Height(30)))
        {
            int tempGroup = 0;
            int tempStage = 0;
            if (bossStage)
            {
                if (group <= CHStatic.BossStageStartValue)
                {
                    tempGroup = CHStatic.BossStageStartValue + group;
                }
                else
                {
                    tempGroup = group;
                }

                tempStage = CHStatic.BossStageStartValue + stage;
            }
            else
            {
                tempGroup = group;
                tempStage = stage;
            }

            var stageInfo = stageInfoList.Find(_ => _.stage == tempStage);
            if (stageInfo == null)
            {
                stageInfoList.Add(new Infomation.StageInfo
                {
                    group = tempGroup,
                    stage = tempStage,
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
                stageInfo.group = tempGroup;
                stageInfo.stage = tempStage;
                stageInfo.blockTypeCount = blockTypeCount;
                stageInfo.boardSize = boardSize;
                stageInfo.time = time;
                stageInfo.targetScore = targetScore;
                stageInfo.moveCount = moveCount;
                stageInfo.tutorialID = tutorialID;
            }

            stageBlockInfoList.RemoveAll(_ => _.stage == tempStage);

            for (int w = 0; w < boardSize; w++)
            {
                for (int h = 0; h < boardSize; h++)
                {
                    if (textures[w, h] == null || blockStates[w, h] == Defines.EBlockState.None)
                        continue;

                    stageBlockInfoList.Add(new Infomation.StageBlockInfo
                    {
                        stage = tempStage,
                        blockState = blockStates[w, h],
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

    void CreateBlockButton(Defines.EBlockState blockState)
    {
        Texture2D texture;
        if (blockSpriteList[(int)blockState] == null)
            texture = MakeBackgroundTexture(50, 50, Color.white);
        else
            texture = blockSpriteList[(int)blockState].texture;

        if (GUILayout.Button(texture, GUILayout.Width(50), GUILayout.Height(50)))
        {
            this.texture = texture;
            this.blockState = blockState;
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

    void ResetBoard()
    {
        for (int w = 0; w < 9; w++)
        {
            for (int h = 0; h < 9; h++)
            {
                textures[w, h] = null;
                blockStates[w, h] = Defines.EBlockState.None;
                hps[w, h] = -1;
                tutorialBlocks[w, h] = false;
            }
        }
    }
}
