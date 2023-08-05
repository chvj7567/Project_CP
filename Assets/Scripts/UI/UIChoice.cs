using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIChoiceArg : CHUIArg
{
    public ReactiveProperty<int> totScore = new ReactiveProperty<int>();
    public ReactiveProperty<int> power = new ReactiveProperty<int>();
    public ReactiveProperty<float> delay = new ReactiveProperty<float>();
    public ReactiveProperty<int> attackCatCount = new ReactiveProperty<int>();
    public List<AttackCat> attackCatList = new List<AttackCat>();
}

public class UIChoice : UIBase
{
    UIChoiceArg arg;

    [SerializeField] Button backBtn;
    [SerializeField] Button redBtn;
    [SerializeField] Button blueBtn;
    [SerializeField] Button greenBtn;
    [SerializeField] Button yellowBtn;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIChoiceArg;
    }

    private async void Start()
    {
        Time.timeScale = 0;

        backBtn.OnClickAsObservable().Subscribe(_ =>
        {
            Time.timeScale = 1;
            CHMMain.UI.CloseUI(gameObject);
        });

        redBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (arg.totScore.Value >= 10)
            {
                arg.totScore.Value -= 10;

                foreach (var cat in arg.attackCatList)
                {
                    cat.attackPower += 5;
                }

                arg.power.Value = arg.attackCatList.First().attackPower;
                Time.timeScale = 1;
                CHMMain.UI.CloseUI(gameObject);
            }
            else
            {
                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    alarmText = "Not Enough Score"
                });
            }
        });

        blueBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (arg.totScore.Value >= 20)
            {
                arg.totScore.Value -= 20;

                foreach (var cat in arg.attackCatList)
                {
                    cat.attackDelay -= .1f;
                }

                arg.delay.Value = arg.attackCatList.First().attackDelay;
                Time.timeScale = 1;
                CHMMain.UI.CloseUI(gameObject);
            }
            else
            {
                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    alarmText = "Not Enough Score"
                });
            }
        });

        greenBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (arg.attackCatCount.Value == 9)
            {
                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    alarmText = "Max Cat"
                });
            }
            else if (arg.totScore.Value >= 100)
            {
                arg.totScore.Value -= 100;

                foreach (var cat in arg.attackCatList)
                {
                    if (cat.gameObject.activeSelf == false)
                    {
                        cat.gameObject.SetActive(true);
                        break;
                    }
                }

                arg.attackCatCount.Value += 1;
                Time.timeScale = 1;
                CHMMain.UI.CloseUI(gameObject);
            }
            else
            {
                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    alarmText = "Not Enough Score"
                });
            }
        });

        yellowBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (arg.totScore.Value >= 1000)
            {
                arg.totScore.Value -= 1000;

                foreach (var cat in arg.attackCatList)
                {
                    cat.attackPower += 50;
                }

                arg.power.Value = arg.attackCatList.First().attackPower;
                Time.timeScale = 1;
                CHMMain.UI.CloseUI(gameObject);
            }
            else
            {
                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    alarmText = "Not Enough Score"
                });
            }
        });
    }
}
