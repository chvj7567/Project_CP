#if UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace CatPang.Tests.EditMode
{
    //# 블럭 비율 정규화(9×9 기준 풋프린트 고정) 테스트.
    //# 대상 production: Assets/Scripts/Function/CHInstantiateButton.cs (factor = 9 / boardSize)
    //#                  Assets/Scripts/Scenes/GPGameScene.cs CreateMap DOScale(GetScaleFactor()).
    //#
    //# 어셈블리 제약(중요) — CHInstantiateButton / Block 은 Assets/Scripts/ 에 asmdef 가 없어
    //#   자동생성 Assembly-CSharp 에 속한다. asmdef 기반 테스트 어셈블리는 Assembly-CSharp 를
    //#   직접 참조할 수 없으므로(Unity 제약), 본 스위트는 리플렉션으로 실제 production 타입을 로드해
    //#   InstantiateButton(...) 을 호출하고 static 게터(GetScaleFactor/GetHorizontalDistance/
    //#   GetBlockInfo)·스폰 블럭 localScale 을 검증한다. (production 코드 미수정 — 구조 변경은
    //#   gameplay-programmer 권한. Assets/Scripts/ 에 asmdef 를 두면 리플렉션 없이 직접 참조 가능.)
    //#
    //# static 상태 누수 방지 — scaleFactor/buttonWidth/margin/blockDict/index 가 static 이라
    //#   매 테스트가 InstantiateButton 을 새로 호출하고 SetUp/TearDown 에서 blockDict 를 비운다.
    public class BlockScaleNormalizeTests
    {
        private const float Delta = 1e-4f;

        //# Assembly-CSharp 의 production 타입 (리플렉션 캐시)
        private Type _buttonType;
        private Type _blockType;
        private MethodInfo _getScaleFactor;
        private MethodInfo _getHorizontalDistance;
        private MethodInfo _getVerticalDistance;
        private MethodInfo _getBlockInfo;
        private MethodInfo _resetBlockDict;
        private MethodInfo _instantiateButton;
        private FieldInfo _buttonWidthField;
        private FieldInfo _marginField;

        //# 테스트가 만든 GameObject 정리용 (origin·spawn·instance)
        private readonly List<GameObject> _spawned = new List<GameObject>();
        private GameObject _buttonInstanceGo;
        private Component _buttonInstance;

        [SetUp]
        public void SetUp()
        {
            _buttonType = FindType("CHInstantiateButton");
            _blockType = FindType("Block");
            Assert.IsNotNull(_buttonType, "CHInstantiateButton 타입을 Assembly-CSharp 에서 찾지 못함");
            Assert.IsNotNull(_blockType, "Block 타입을 Assembly-CSharp 에서 찾지 못함");

            _getScaleFactor = _buttonType.GetMethod("GetScaleFactor", BindingFlags.Public | BindingFlags.Static);
            _getHorizontalDistance = _buttonType.GetMethod("GetHorizontalDistance", BindingFlags.Public | BindingFlags.Static);
            _getVerticalDistance = _buttonType.GetMethod("GetVerticalDistance", BindingFlags.Public | BindingFlags.Static);
            _getBlockInfo = _buttonType.GetMethod("GetBlockInfo", BindingFlags.Public | BindingFlags.Static);
            _resetBlockDict = _buttonType.GetMethod("ResetBlockDict", BindingFlags.Public | BindingFlags.Static);
            _instantiateButton = _buttonType.GetMethod("InstantiateButton", BindingFlags.Public | BindingFlags.Instance);
            _buttonWidthField = _buttonType.GetField("buttonWidth", BindingFlags.NonPublic | BindingFlags.Static);
            _marginField = _buttonType.GetField("margin", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(_getScaleFactor, "GetScaleFactor() 가 노출되어야 한다 (계약)");
            Assert.IsNotNull(_getHorizontalDistance, "GetHorizontalDistance() 가 노출되어야 한다");
            Assert.IsNotNull(_getVerticalDistance, "GetVerticalDistance() 가 노출되어야 한다");
            Assert.IsNotNull(_getBlockInfo, "GetBlockInfo(Vector2) 가 노출되어야 한다");
            Assert.IsNotNull(_instantiateButton, "InstantiateButton(...) 가 노출되어야 한다");

            //# CHInstantiateButton 인스턴스를 직접 생성 (Instance 게터의 DontDestroyOnLoad 회피)
            _buttonInstanceGo = new GameObject("CHInstantiateButton_TestHost");
            _spawned.Add(_buttonInstanceGo);
            _buttonInstance = _buttonInstanceGo.AddComponent(_buttonType);

            _resetBlockDict?.Invoke(null, null);
        }

        [TearDown]
        public void TearDown()
        {
            _resetBlockDict?.Invoke(null, null);
            foreach (GameObject go in _spawned)
            {
                if (go != null)
                    UnityEngine.Object.DestroyImmediate(go);
            }
            _spawned.Clear();
        }

        private static Type FindType(string name)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType(name);
                if (t != null)
                    return t;
            }
            return null;
        }

        //# 중앙 앵커 + sizeDelta(cellSize) 로 origin 을 만들면 rect.x = -cellSize/2 →
        //# base buttonWidth = |rect.x*2| = cellSize 로 결정적. baseCellSize 가 9×9 한 칸 기준 크기.
        private GameObject MakeOrigin(float cellSize)
        {
            GameObject origin = new GameObject("Origin", typeof(RectTransform));
            _spawned.Add(origin);
            RectTransform rt = origin.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(cellSize, cellSize);
            return origin;
        }

        //# InstantiateButton 을 리플렉션으로 호출. boardArr 은 Block[boardSize, boardSize] 동적 생성.
        //# 호출 후 spawn 된 블럭 자식들을 _spawned 에 등록해 정리.
        private GameObject CallInstantiate(float cellSize, float margin, int boardSize)
        {
            GameObject origin = MakeOrigin(cellSize);
            GameObject parent = new GameObject("Parent", typeof(RectTransform));
            _spawned.Add(parent);

            Array boardArr = Array.CreateInstance(_blockType, boardSize, boardSize);
            object[] args =
            {
                origin,
                margin,
                boardSize,
                boardSize,
                parent.transform,
                boardArr,
            };
            _instantiateButton.Invoke(_buttonInstance, args);

            //# spawn 된 블럭 정리 등록
            foreach (Transform child in parent.transform)
                _spawned.Add(child.gameObject);

            return parent;
        }

        private float ScaleFactor() => (float)_getScaleFactor.Invoke(null, null);
        private float HorizontalDistance() => (float)_getHorizontalDistance.Invoke(null, null);
        private float VerticalDistance() => (float)_getVerticalDistance.Invoke(null, null);
        private float ButtonWidth() => (float)_buttonWidthField.GetValue(null);
        private float Margin() => (float)_marginField.GetValue(null);

        //# ── factor 공식: factor = 9 / boardSize ────────────────────────────────
        [Test]
        public void boardSize_9이면_scaleFactor가_1이다()
        {
            CallInstantiate(cellSize: 100f, margin: 0f, boardSize: 9);
            Assert.AreEqual(1.0f, ScaleFactor(), Delta, "9×9 는 factor 1.0 (회귀 기준)");
        }

        [Test]
        public void boardSize_3이면_scaleFactor가_3이다()
        {
            CallInstantiate(cellSize: 100f, margin: 0f, boardSize: 3);
            Assert.AreEqual(3.0f, ScaleFactor(), Delta, "3×3 는 최대 운용 factor 3.0");
        }

        [Test]
        public void boardSize_5이면_scaleFactor가_1점8이다()
        {
            CallInstantiate(cellSize: 100f, margin: 0f, boardSize: 5);
            Assert.AreEqual(1.8f, ScaleFactor(), Delta, "5×5 → 9/5 = 1.8");
        }

        [Test]
        public void boardSize_6이면_scaleFactor가_1점5이다()
        {
            CallInstantiate(cellSize: 100f, margin: 0f, boardSize: 6);
            Assert.AreEqual(1.5f, ScaleFactor(), Delta, "6×6 → 9/6 = 1.5");
        }

        [Test]
        public void boardSize_4이면_scaleFactor가_2점25이다()
        {
            CallInstantiate(cellSize: 100f, margin: 0f, boardSize: 4);
            Assert.AreEqual(2.25f, ScaleFactor(), Delta, "4×4 → 9/4 = 2.25");
        }

        [Test]
        public void boardSize_7이면_scaleFactor가_약1점286이다()
        {
            CallInstantiate(cellSize: 100f, margin: 0f, boardSize: 7);
            Assert.AreEqual(9f / 7f, ScaleFactor(), Delta, "7×7 → 9/7 ≈ 1.2857");
        }

        [Test]
        public void boardSize_8이면_scaleFactor가_1점125이다()
        {
            CallInstantiate(cellSize: 100f, margin: 0f, boardSize: 8);
            Assert.AreEqual(1.125f, ScaleFactor(), Delta, "8×8 → 9/8 = 1.125");
        }

        //# ── 9×9 회귀: factor=1.0 일 때 buttonWidth/margin/블럭 localScale 무변화 ──
        [Test]
        public void boardSize_9에서_buttonWidth가_원본cellSize와_동일하다()
        {
            CallInstantiate(cellSize: 120f, margin: 0f, boardSize: 9);
            Assert.AreEqual(120f, ButtonWidth(), Delta, "9×9 는 buttonWidth = base cellSize (factor 1.0 무변화)");
        }

        [Test]
        public void boardSize_9에서_margin이_원본과_동일하다()
        {
            CallInstantiate(cellSize: 120f, margin: 10f, boardSize: 9);
            Assert.AreEqual(10f, Margin(), Delta, "9×9 는 margin *= 1f → 무변화");
        }

        [Test]
        public void boardSize_9에서_모든_블럭_localScale이_1이다()
        {
            GameObject parent = CallInstantiate(cellSize: 100f, margin: 0f, boardSize: 9);
            foreach (Transform child in parent.transform)
            {
                Vector3 s = child.localScale;
                Assert.AreEqual(1.0f, s.x, Delta, "9×9 블럭 localScale.x = 1");
                Assert.AreEqual(1.0f, s.y, Delta, "9×9 블럭 localScale.y = 1");
            }
        }

        //# ── 블럭 localScale = factor (작은 보드) ─────────────────────────────────
        [Test]
        public void boardSize_3에서_모든_블럭_localScale이_3이다()
        {
            GameObject parent = CallInstantiate(cellSize: 100f, margin: 0f, boardSize: 3);
            int count = 0;
            foreach (Transform child in parent.transform)
            {
                Vector3 s = child.localScale;
                Assert.AreEqual(3.0f, s.x, Delta, "3×3 블럭 localScale.x = factor 3.0");
                Assert.AreEqual(3.0f, s.y, Delta, "3×3 블럭 localScale.y = factor 3.0");
                Assert.AreEqual(3.0f, s.z, Delta, "Vector3.one * factor → z 도 factor");
                count++;
            }
            Assert.AreEqual(9, count, "3×3 = 9 블럭 스폰");
        }

        [Test]
        public void boardSize_5에서_모든_블럭_localScale이_1점8이다()
        {
            GameObject parent = CallInstantiate(cellSize: 100f, margin: 0f, boardSize: 5);
            foreach (Transform child in parent.transform)
            {
                Assert.AreEqual(1.8f, child.localScale.x, Delta, "5×5 블럭 localScale = factor 1.8");
                Assert.AreEqual(1.8f, child.localScale.y, Delta, "5×5 블럭 localScale = factor 1.8");
            }
        }

        //# ── 종횡비 보존: x·y 동일 단일 배율 (찌그러짐 없음, 기획서 §2.4) ──────────
        [Test]
        public void 블럭_localScale의_x와_y가_항상_동일하다()
        {
            foreach (int boardSize in new[] { 3, 5, 6, 9 })
            {
                _resetBlockDict.Invoke(null, null);
                GameObject parent = CallInstantiate(cellSize: 100f, margin: 0f, boardSize);
                foreach (Transform child in parent.transform)
                {
                    Assert.AreEqual(child.localScale.x, child.localScale.y, Delta,
                        $"boardSize {boardSize}: localScale x==y (종횡비 보존)");
                }
            }
        }

        //# ── buttonWidth 가 factor 추종 (간격·히트판정의 단일 진실) ───────────────
        [Test]
        public void boardSize_3에서_buttonWidth가_원본의_3배다()
        {
            CallInstantiate(cellSize: 100f, margin: 0f, boardSize: 3);
            Assert.AreEqual(300f, ButtonWidth(), Delta, "3×3 buttonWidth = 100 * factor3");
        }

        //# ── 풋프린트 일치: (margin + buttonWidth) * boardSize 가 9×9 와 동일 ──────
        //# 9×9 가로 풋프린트 = (margin9 + width9) * 9. 작은 보드도 같아야 함 (factor 가 정확히 9/n 이므로).
        [Test]
        public void 작은보드의_가로풋프린트가_9x9와_동일하다()
        {
            //# 9×9 기준 풋프린트 (margin 0)
            CallInstantiate(cellSize: 100f, margin: 0f, boardSize: 9);
            float footprint9 = (Margin() + ButtonWidth()) * 9;

            foreach (int boardSize in new[] { 3, 4, 5, 6, 7, 8 })
            {
                _resetBlockDict.Invoke(null, null);
                CallInstantiate(cellSize: 100f, margin: 0f, boardSize);
                float footprint = (Margin() + ButtonWidth()) * boardSize;
                Assert.AreEqual(footprint9, footprint, Delta,
                    $"boardSize {boardSize} 가로 풋프린트가 9×9 와 동일해야 한다");
            }
        }

        //# margin 이 0 이 아닐 때도 풋프린트가 일치하는지 (margin 도 factor 추종 — plan delta)
        [Test]
        public void margin이_있어도_가로풋프린트가_9x9와_동일하다()
        {
            CallInstantiate(cellSize: 100f, margin: 8f, boardSize: 9);
            float footprint9 = (Margin() + ButtonWidth()) * 9;

            foreach (int boardSize in new[] { 3, 5, 6 })
            {
                _resetBlockDict.Invoke(null, null);
                CallInstantiate(cellSize: 100f, margin: 8f, boardSize);
                float footprint = (Margin() + ButtonWidth()) * boardSize;
                Assert.AreEqual(footprint9, footprint, Delta,
                    $"margin 8 일 때 boardSize {boardSize} 풋프린트가 9×9 와 동일 (margin 도 factor 추종)");
            }
        }

        //# ── GetHorizontalDistance / GetVerticalDistance 가 factor 추종 ───────────
        [Test]
        public void GetHorizontalDistance가_factor적용_buttonWidth더하기margin이다()
        {
            CallInstantiate(cellSize: 100f, margin: 12f, boardSize: 3);
            //# factor 3 → width 300, margin 36
            float expected = ButtonWidth() + Margin();
            Assert.AreEqual(expected, HorizontalDistance(), Delta, "GetHorizontalDistance = buttonWidth + margin (factor 반영)");
            Assert.AreEqual(336f, HorizontalDistance(), Delta, "3×3, cell100 margin12 → 300 + 36");
        }

        [Test]
        public void GetVerticalDistance가_factor적용_buttonHeight더하기margin이다()
        {
            CallInstantiate(cellSize: 100f, margin: 12f, boardSize: 3);
            Assert.AreEqual(336f, VerticalDistance(), Delta, "정사각 → 수직 거리도 336");
        }

        [Test]
        public void 보드가_작을수록_GetHorizontalDistance가_커진다()
        {
            CallInstantiate(cellSize: 100f, margin: 0f, boardSize: 9);
            float d9 = HorizontalDistance();
            _resetBlockDict.Invoke(null, null);
            CallInstantiate(cellSize: 100f, margin: 0f, boardSize: 5);
            float d5 = HorizontalDistance();
            _resetBlockDict.Invoke(null, null);
            CallInstantiate(cellSize: 100f, margin: 0f, boardSize: 3);
            float d3 = HorizontalDistance();

            Assert.Less(d9, d5, "9×9 간격 < 5×5 간격");
            Assert.Less(d5, d3, "5×5 간격 < 3×3 간격");
        }

        //# ── GetBlockInfo 히트반경(= buttonWidth/2) 이 factor 추종 ────────────────
        //# 스폰 좌표는 anchoredPosition 기반. (0,0) 의 블럭(row0,col0)은 buttonX 위치 = origin.x = 0.
        //# 9×9 에서는 잡히지 않던 오프셋 거리가 3×3(반경 3배)에서는 잡혀야 한다.
        [Test]
        public void GetBlockInfo_히트반경이_9x9에서_buttonWidth절반이다()
        {
            CallInstantiate(cellSize: 100f, margin: 0f, boardSize: 9);
            float radius = ButtonWidth() / 2f; //# 50

            //# block(0,0) 은 anchoredPosition (0,0). y축 위로 프로빙 — 세로 이웃 block(1,0) 은
            //# y=-100 이고 가로 이웃 block(0,1) 은 x=100 이라, +y 짧은 거리는 block(0,0) 만 후보.
            //# 반경 안(49)은 잡히고 밖(51)은 못 잡는다.
            object insideTuple = _getBlockInfo.Invoke(null, new object[] { new Vector2(0f, radius - 1f) });
            object outsideTuple = _getBlockInfo.Invoke(null, new object[] { new Vector2(0f, radius + 1f) });

            Assert.AreEqual(50f, radius, Delta, "9×9 히트반경 = 100/2 = 50 (factor 1.0)");
            Assert.IsNotNull(TupleBlock(insideTuple), "반경 안(49)의 좌표는 블럭을 잡아야 한다");
            Assert.IsNull(TupleBlock(outsideTuple), "반경 밖(51)의 좌표는 블럭을 못 잡아야 한다");
        }

        [Test]
        public void GetBlockInfo_히트반경이_3x3에서_3배로_커진다()
        {
            //# 3×3, cell100 → buttonWidth 300, 반경 150.
            CallInstantiate(cellSize: 100f, margin: 0f, boardSize: 3);
            float radius = ButtonWidth() / 2f; //# 150

            //# block(0,0)=(0,0). +y 100 거리는 9×9 반경(50) 밖이지만 3×3 반경(150) 안 → 3×3 에서는 잡힘.
            //# (세로 이웃 block(1,0)=y-300, 가로 이웃 block(0,1)=x300 모두 100 보다 멀어 후보 아님.)
            object hit = _getBlockInfo.Invoke(null, new object[] { new Vector2(0f, 100f) });
            Assert.AreEqual(150f, radius, Delta, "3×3 히트반경 = 300/2 = 150 (factor 추종)");
            Assert.IsNotNull(TupleBlock(hit), "3×3 확대 반경(150) 안의 좌표(100)는 블럭을 잡아야 한다");
        }

        //# ValueTuple<RectTransform, Block> 의 Item2(Block) 를 리플렉션으로 추출
        private static object TupleBlock(object tuple)
        {
            if (tuple == null)
                return null;
            FieldInfo item2 = tuple.GetType().GetField("Item2");
            return item2?.GetValue(tuple);
        }

        //# ── 스폰 좌표 회귀: (margin+buttonWidth) 간격으로 등간격 배치 ──────────────
        [Test]
        public void 인접_블럭_x간격이_GetHorizontalDistance와_일치한다()
        {
            GameObject parent = CallInstantiate(cellSize: 100f, margin: 5f, boardSize: 3);
            float dist = HorizontalDistance();

            //# row0 의 col0, col1 블럭 anchoredPosition.x 차이 = dist 여야 한다.
            Dictionary<int, float> colX = new Dictionary<int, float>();
            foreach (Transform child in parent.transform)
            {
                //# 이름 규칙 Block{row}/{col}
                string[] parts = child.name.Replace("Block", "").Split('/');
                int r = int.Parse(parts[0]);
                int c = int.Parse(parts[1]);
                if (r == 0)
                    colX[c] = child.GetComponent<RectTransform>().anchoredPosition.x;
            }

            Assert.IsTrue(colX.ContainsKey(0) && colX.ContainsKey(1), "row0 col0,col1 블럭이 있어야 한다");
            Assert.AreEqual(dist, colX[1] - colX[0], Delta, "인접 x 간격 = GetHorizontalDistance");
        }
    }
}
#endif
