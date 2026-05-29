using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ChvjUnityInfra;

namespace CatPang.Tests.EditMode
{
    //# Cat1~5 수집 미션 데이터 정합 테스트 (Mission.json ↔ String JSON ↔ EBlockState/EReward).
    //# 데이터 소스: Application.dataPath 기준 AssetBundleResources/json/*.json 원본 파일 직접 읽기
    //#             (Addressables 빌드 상태와 무관하게 소스 진실을 검증).
    //# JsonArrayUtility 는 패키지(com.chvj.unityinfra) → asmdef 가 직접 참조.
    //# 순수 JSON 데이터 정합만 검증한다 (Defines enum 값 대조는 인프라 부담(리플렉션) 대비 가치가 낮아 제외).
    //#
    //# 주의 — Mission.json 의 missionID/tapIndex 는 JSON 상 "문자열"("18")이라 JsonUtility 가 int 로
    //#        코어션하지 않는다(기본값 -1). DTO 파싱은 실수형 필드(collectionType/descStringID/clearValue
    //#        등)만 신뢰하고, missionID 18~22 / tapIndex 매핑은 원본 텍스트로 검증한다.
    public class CatMissionDataIntegrityTests
    {
        //# Mission.json 파싱용 DTO — JSON 상 숫자 필드만 (missionID/tapIndex 는 문자열이라 제외).
        [Serializable]
        private class MissionRow
        {
            public int descStringID = -1;
            public int collectionType = -99;
            public int clearValue = -1;
            public int addValue = -1;
            public int reward = -99;
            public int rewardCount = -1;
        }

        [Serializable]
        private class StringRow
        {
            public int stringID = -1;
            public string value = "";
        }

        private const string JsonDir = "AssetBundleResources/json/";

        private static string ReadJson(string fileName)
        {
            string path = Path.Combine(Application.dataPath, JsonDir + fileName);
            Assert.IsTrue(File.Exists(path), $"JSON 파일이 존재해야 한다: {path}");
            //# 한국어 포함 — 명시적 UTF-8 디코딩 (시스템 코드페이지로 읽으면 모지바케)
            return File.ReadAllText(path, new System.Text.UTF8Encoding(false));
        }

        //# collectionType(0~4) 으로 Cat 미션 행만 추린다 — Cat1~5 = EBlockState 0~4.
        private static Dictionary<int, MissionRow> LoadCatMissionRowsByCollectionType()
        {
            MissionRow[] rows = JsonArrayUtility.FromJsonArray<MissionRow>(ReadJson("Mission.json"));
            Dictionary<int, MissionRow> catRows = new Dictionary<int, MissionRow>();
            foreach (MissionRow row in rows)
            {
                if (row.collectionType >= 0 && row.collectionType <= 4)
                    catRows[row.collectionType] = row;
            }
            return catRows;
        }

        private static Dictionary<int, string> LoadStringMap(string fileName)
        {
            StringRow[] rows = JsonArrayUtility.FromJsonArray<StringRow>(ReadJson(fileName));
            Dictionary<int, string> map = new Dictionary<int, string>();
            foreach (StringRow row in rows)
                map[row.stringID] = row.value;
            return map;
        }

        //# ── 정상: Cat1~5 미션 5개가 collectionType 0~4 로 모두 존재 ────────────────
        [Test]
        public void Mission_json에_collectionType_0부터4_Cat미션이_모두_존재한다()
        {
            Dictionary<int, MissionRow> catRows = LoadCatMissionRowsByCollectionType();
            for (int ct = 0; ct <= 4; ct++)
            {
                Assert.IsTrue(catRows.ContainsKey(ct), $"collectionType {ct} (Cat{ct + 1}) 미션이 Mission.json 에 있어야 한다");
            }
            Assert.AreEqual(5, catRows.Count, "Cat 미션은 정확히 5개 (collectionType 0~4)");
        }

        //# ── 정합: 각 Cat 미션의 descStringID 가 180~184 와 1:1 대응 ─────────────────
        [Test]
        public void Cat미션_collectionType_0부터4는_descStringID_180부터184를_가리킨다()
        {
            Dictionary<int, MissionRow> catRows = LoadCatMissionRowsByCollectionType();
            Assert.AreEqual(180, catRows[0].descStringID, "Cat1 미션 descStringID");
            Assert.AreEqual(181, catRows[1].descStringID, "Cat2 미션 descStringID");
            Assert.AreEqual(182, catRows[2].descStringID, "Cat3 미션 descStringID");
            Assert.AreEqual(183, catRows[3].descStringID, "Cat4 미션 descStringID");
            Assert.AreEqual(184, catRows[4].descStringID, "Cat5 미션 descStringID");
        }

        //# ── 밸런스 결정 락: clearValue=100, addValue=100, reward=0(Gold), rewardCount=100 ──
        [Test]
        public void Cat미션_5개의_밸런스값이_결정락_100과_Gold100과_일치한다()
        {
            Dictionary<int, MissionRow> catRows = LoadCatMissionRowsByCollectionType();
            for (int ct = 0; ct <= 4; ct++)
            {
                MissionRow row = catRows[ct];
                Assert.AreEqual(100, row.clearValue, $"Cat{ct + 1} clearValue 는 100");
                Assert.AreEqual(100, row.addValue, $"Cat{ct + 1} addValue 는 100");
                Assert.AreEqual(0, row.reward, $"Cat{ct + 1} reward 는 0(Gold)");
                Assert.AreEqual(100, row.rewardCount, $"Cat{ct + 1} rewardCount 는 100");
            }
        }

        //# ── 문자열 존재: 두 String 파일에 180~184 모두 존재하고 비어있지 않다 ─────────
        [Test]
        public void StringKorea에_180부터184가_모두_존재하고_비어있지_않다()
        {
            Dictionary<int, string> map = LoadStringMap("StringKorea.json");
            for (int id = 180; id <= 184; id++)
            {
                Assert.IsTrue(map.ContainsKey(id), $"StringKorea.json 에 stringID {id} 가 있어야 한다");
                Assert.IsFalse(string.IsNullOrWhiteSpace(map[id]), $"StringKorea.json {id} 값이 비어있으면 안 됨");
            }
        }

        [Test]
        public void StringEnglish에_180부터184가_모두_존재하고_비어있지_않다()
        {
            Dictionary<int, string> map = LoadStringMap("StringEnglish.json");
            for (int id = 180; id <= 184; id++)
            {
                Assert.IsTrue(map.ContainsKey(id), $"StringEnglish.json 에 stringID {id} 가 있어야 한다");
                Assert.IsFalse(string.IsNullOrWhiteSpace(map[id]), $"StringEnglish.json {id} 값이 비어있으면 안 됨");
            }
        }

        //# 한국어 문구가 모지바케 없이 정상 디코딩되는지 — "고양이" 포함 (UTF-8 무결성 회귀).
        [Test]
        public void StringKorea_180부터184는_고양이_문구를_포함한다()
        {
            Dictionary<int, string> map = LoadStringMap("StringKorea.json");
            for (int id = 180; id <= 184; id++)
            {
                StringAssert.Contains("고양이", map[id], $"StringKorea {id} 한국어 문구 무결성");
            }
        }

        //# ── 원본 텍스트 검증: missionID 18~22 가 Mission.json 에 실제 존재 ─────────────
        //# (missionID 는 JSON 상 문자열이라 DTO 로는 못 잡음 → 원본 텍스트로 검증)
        [Test]
        public void Mission_json_원본에_missionID_18부터22가_존재한다()
        {
            string raw = ReadJson("Mission.json");
            for (int id = 18; id <= 22; id++)
            {
                StringAssert.Contains($"\"missionID\":\"{id}\"", raw, $"Mission.json 에 missionID {id} 행이 있어야 한다");
            }
        }

        //# Cat 미션은 tapIndex 1 (미션 탭 맨 앞) 에 배치 — 원본 텍스트 회귀 (tapIndex 도 문자열).
        [Test]
        public void Mission_json_원본에서_Cat미션_18부터22는_tapIndex_1이다()
        {
            string raw = ReadJson("Mission.json");
            string[] lines = raw.Replace("\r", "").Split('\n');
            for (int id = 18; id <= 22; id++)
            {
                bool found = false;
                foreach (string line in lines)
                {
                    if (line.Contains($"\"missionID\":\"{id}\""))
                    {
                        Assert.IsTrue(line.Contains("\"tapIndex\":\"1\""), $"missionID {id} 는 tapIndex 1 이어야 한다");
                        found = true;
                        break;
                    }
                }
                Assert.IsTrue(found, $"missionID {id} 행을 찾지 못함");
            }
        }
    }
}
