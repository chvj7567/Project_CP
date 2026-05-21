using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using ChvjUnityInfra;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using static Defines;

// 실패 화면에 표시할 '남은 목표 블록' 1종류
public class BlockTypeCount
{
    public Defines.EBlockState state;
    public int count;
}

// 게임 실패 사유 데이터. 클리어 시에는 생성하지 않는다(null).
public class GameEndFailInfo
{
    public Defines.EFailReason reason;
    public int curScore;                       // 일반/하드
    public int targetScore;                    // 일반/하드
    public List<BlockTypeCount> remainBlocks;  // 일반/하드, EBlockState별 그룹
    public float bossHpRatio;                  // 보스전, 0~1
}

public class UIGameEndArg : CHUIArg
{
    public Defines.EClearState clearState = Defines.EClearState.None;
    public Defines.EGameState result = Defines.EGameState.None;
    public int gold;
    public GameEndFailInfo failInfo;           // 실패 시에만, 클리어 시 null
}

public class UIGameEnd : UIBase
{
    // 게임 결과 화면은 ESC로 임의 닫히면 안 됨 — Next/Retry/Claim 버튼으로만 진행.
    public override bool BlockEscClose => true;

    private UIGameEndArg arg;

    [SerializeField] private TMP_Text resultText;
    [SerializeField] private GameObject successObj;
    [SerializeField] private GameObject failedObj;
    [SerializeField] private CHText goldText;
    [SerializeField] private CHText goldx2Text;
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button retryBtn;
    [SerializeField] private Button adBtn;
    [SerializeField] private Button claimBtn;

    [Header("실패 사유 표시")]
    [SerializeField] private CHText failReasonText;
    [SerializeField] private GameObject failBlockRoot;          // 남은 목표 블록 섹션 루트(FailReason2)
    [SerializeField] private CHText failBlockHeaderText;
    [SerializeField] private Transform failBlockIconContainer;
    [SerializeField] private FailBlockIconItem failBlockIconTemplate;
    [SerializeField] private CHText failDetailText;

    private bool received = false;

    // 결과 텍스트(Failed/CLEAR) 연출 시간(초)
    private const float ResultTextRevealDuration = 1f;
    // 광고 시청 보상으로 지급하는 골드 배수
    private const int AdRewardGoldMultiplier = 3;

    // 실패 사유 로컬라이제이션 문자열 ID (StringKorea/StringEnglish.json)
    private const int FailTimeOverStringID = 174;
    private const int FailMoveOverStringID = 175;
    private const int FailHpOverStringID = 176;
    private const int FailBossHpStringID = 177;
    private const int FailScoreStringID = 178;
    private const int FailBlockHeaderStringID = 179;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIGameEndArg;
    }

    private void OnDestroy()
    {
        ChvjUnityInfra.CHMAdmob.Instance.AcquireReward -= AcquireReward;
    }

    private void Start()
    {
        if (arg.result == Defines.EGameState.GameOver)
        {
            if (successObj != null) successObj.SetActive(false);
            if (failedObj != null) failedObj.SetActive(true);

            resultText.DOText("Failed...", ResultTextRevealDuration);
            goldText.SetText(0);
            goldx2Text.SetText(0);

            ShowFailReason(arg.failInfo);
        }
        else if (arg.result == Defines.EGameState.GameClear)
        {
            if (successObj != null) successObj.SetActive(true);
            if (failedObj != null) failedObj.SetActive(false);

            resultText.DOText("CLEAR!", ResultTextRevealDuration);

            if (arg.clearState == Defines.EClearState.Clear)
            {
                goldText.SetText(0);
                goldx2Text.SetText(0);
            }
            else
            {
                goldText.SetText(arg.gold);
                goldx2Text.SetText(arg.gold * 2);
            }
        }

        ChvjUnityInfra.CHMAdmob.Instance.AcquireReward += AcquireReward;

        BindUI();
    }

    // 실패 화면에 종료 사유와 미달 목표를 표시한다.
    private void ShowFailReason(GameEndFailInfo info)
    {
        // 템플릿 자체는 항상 숨김 — 복제본만 표시
        if (failBlockIconTemplate != null)
            failBlockIconTemplate.gameObject.SetActive(false);

        if (info == null)
        {
            if (failReasonText != null) failReasonText.gameObject.SetActive(false);
            if (failBlockRoot != null) failBlockRoot.SetActive(false);
            if (failDetailText != null) failDetailText.gameObject.SetActive(false);
            return;
        }

        // 트리거 줄
        if (failReasonText != null)
        {
            failReasonText.gameObject.SetActive(true);
            failReasonText.SetText(CHMString.Instance.GetString(FailReasonStringID(info.reason)));
        }

        // 남은 목표 블록 섹션 (일반/하드 전용) — 루트(FailReason2)를 통째로 토글
        bool hasBlocks = info.remainBlocks != null && info.remainBlocks.Count > 0;
        if (failBlockRoot != null)
            failBlockRoot.SetActive(hasBlocks);
        if (hasBlocks)
        {
            if (failBlockHeaderText != null)
                failBlockHeaderText.SetText(CHMString.Instance.GetString(FailBlockHeaderStringID));
            if (failBlockIconTemplate != null && failBlockIconContainer != null)
            {
                foreach (var entry in info.remainBlocks)
                {
                    var item = Instantiate(failBlockIconTemplate, failBlockIconContainer);
                    item.gameObject.SetActive(true);
                    item.Setup(entry.state, entry.count);
                }
            }
        }

        // 점수 줄(일반/하드) 또는 보스 HP 줄(보스전)
        if (failDetailText != null)
        {
            if (info.reason == Defines.EFailReason.HpOver)
            {
                failDetailText.gameObject.SetActive(true);
                failDetailText.SetText(string.Format(
                    CHMString.Instance.GetString(FailBossHpStringID),
                    Mathf.RoundToInt(info.bossHpRatio * 100f)));
            }
            else if (info.targetScore > 0)
            {
                // 목표 점수가 있는 스테이지면 달성 여부와 무관하게 항상 점수 줄 표시
                failDetailText.gameObject.SetActive(true);
                failDetailText.SetText(string.Format(
                    CHMString.Instance.GetString(FailScoreStringID),
                    info.curScore.ToString("N0"), info.targetScore.ToString("N0")));
            }
            else
            {
                failDetailText.gameObject.SetActive(false);
            }
        }
    }

    // EFailReason → 트리거 문자열 ID
    private static int FailReasonStringID(Defines.EFailReason reason)
    {
        switch (reason)
        {
            case Defines.EFailReason.TimeOver: return FailTimeOverStringID;
            case Defines.EFailReason.MoveOver: return FailMoveOverStringID;
            case Defines.EFailReason.HpOver:   return FailHpOverStringID;
            default:                           return FailTimeOverStringID;
        }
    }

    private void BindUI()
    {
        nextBtn.OnClickAsObservable().Subscribe(_ =>
        {
            int currentStage = 0;
            Defines.ESelectStage selectStage = (Defines.ESelectStage)PlayerPrefs.GetInt(CHMString.Instance.SelectStage);
            switch (selectStage)
            {
                case ESelectStage.Hard: currentStage = PlayerPrefs.GetInt(CHMString.Instance.HardStage); break;
                case ESelectStage.Boss: currentStage = PlayerPrefs.GetInt(CHMString.Instance.BossStage); break;
                case ESelectStage.Normal: currentStage = PlayerPrefs.GetInt(CHMString.Instance.NormalStage); break;
            }

            int nextStage = currentStage + 1;

            // 다음 스테이지 PlayerPrefs도 갱신 — LBLobbyScene이 StageSelect 흐름에서 이 값을 기준으로 페이지를 잡는다.
            switch (selectStage)
            {
                case ESelectStage.Hard: PlayerPrefs.SetInt(CHMString.Instance.HardStage, nextStage); break;
                case ESelectStage.Boss: PlayerPrefs.SetInt(CHMString.Instance.BossStage, nextStage); break;
                case ESelectStage.Normal: PlayerPrefs.SetInt(CHMString.Instance.NormalStage, nextStage); break;
            }

            // FirstScene 진입 후 자동으로 다음 스테이지의 UIGameStart 띄우도록 요청.
            LBLobbyScene.fromGame = true;
            LBLobbyScene.pendingShowGameStartStage = nextStage;

            // 게임 상태 정리 (claimBtn과 동일 패턴).
            Time.timeScale = 1;
            CHInstantiateButton.ResetBlockDict();
            CHMPool.Instance.Clear();

            SceneManager.LoadScene((int)EScene.FirstScene);

        }).AddTo(this);

        if (retryBtn != null)
        {
            retryBtn.OnClickAsObservable().Subscribe(_ =>
            {
                int currentStage = 0;
                Defines.ESelectStage selectStage = (Defines.ESelectStage)PlayerPrefs.GetInt(CHMString.Instance.SelectStage);
                switch (selectStage)
                {
                    case ESelectStage.Hard: currentStage = PlayerPrefs.GetInt(CHMString.Instance.HardStage); break;
                    case ESelectStage.Boss: currentStage = PlayerPrefs.GetInt(CHMString.Instance.BossStage); break;
                    case ESelectStage.Normal: currentStage = PlayerPrefs.GetInt(CHMString.Instance.NormalStage); break;
                }

                // 같은 스테이지로 재시작 — PlayerPrefs는 그대로 두고, 로비 진입 후 동일 stage의 UIGameStart 자동 표시.
                LBLobbyScene.fromGame = true;
                LBLobbyScene.pendingShowGameStartStage = currentStage;

                Time.timeScale = 1;
                CHInstantiateButton.ResetBlockDict();
                CHMPool.Instance.Clear();

                SceneManager.LoadScene((int)EScene.FirstScene);

            }).AddTo(this);
        }

        claimBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (received == true)
            {
                // 로비 진입 시 startBtn 단계 건너뛰고 스테이지 메뉴 직행.
                LBLobbyScene.fromGame = true;

                Time.timeScale = 1;
                CHInstantiateButton.ResetBlockDict();
                CHMUI.Instance.CloseUI(Defines.EUI.UIAlarm);
                CHMPool.Instance.Clear();

                SceneManager.LoadScene((int)EScene.FirstScene);
                return;
            }

            var before = CHMData.Instance.GetCollectionData(CHMString.Instance.Gold).value;
            var after = CHMData.Instance.GetCollectionData(CHMString.Instance.Gold).value += arg.gold;

            received = true;

            Debug.Log($"Gold {before} => {after}");

            CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
            {
                stringID = 63
            });

            // 로비 진입 시 startBtn 단계 건너뛰고 스테이지 메뉴 직행.
            LBLobbyScene.fromGame = true;

            Time.timeScale = 1;
            CHInstantiateButton.ResetBlockDict();
            CHMUI.Instance.CloseUI(Defines.EUI.UIAlarm);
            CHMPool.Instance.Clear();

            CHMData.Instance.SaveData(CHMString.Instance.CatPang);
            SceneManager.LoadScene((int)EScene.FirstScene);
        }).AddTo(this);

        adBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (ChvjUnityInfra.CHMIAP.Instance.CanBuyFromID(CHMString.Instance.Product_ID_RemoveAD) == false)
            {
                AcquireReward();
            }
            else
            {
                ChvjUnityInfra.CHMAdmob.Instance.ShowRewardedAd();
            }
        }).AddTo(this);
    }

    private void AcquireReward()
    {
        if (received == true)
        {
            CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
            {
                stringID = 64
            });

            return;
        }

        var before = CHMData.Instance.GetCollectionData(CHMString.Instance.Gold).value;
        var after = CHMData.Instance.GetCollectionData(CHMString.Instance.Gold).value += arg.gold * AdRewardGoldMultiplier;

        received = true;

        Debug.Log($"Gold {before} => {after}");

        CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
        {
            stringID = 63
        });
    }
}
