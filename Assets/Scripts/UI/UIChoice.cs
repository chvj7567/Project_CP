using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using static Defines;
using static Infomation;

public class UIChoiceArg : CHUIArg
{
    public ReactiveProperty<int> curScore = new ReactiveProperty<int>();
    public ReactiveProperty<int> power = new ReactiveProperty<int>();
    public ReactiveProperty<float> delay = new ReactiveProperty<float>();
    public ReactiveProperty<float> speed = new ReactiveProperty<float>();
    public ReactiveProperty<int> attackCatCount = new ReactiveProperty<int>();
    public List<AttackCat> attackCatList = new List<AttackCat>();
    public int maxPower;
    public float minDealy;
    public float maxSpeed;
    public List<Sprite> catPangImgList = new List<Sprite>();
}

public class UIChoice : UIBase
{
    UIChoiceArg arg;

    [SerializeField] Button backBtn;
    [SerializeField] Button select1Btn;
    [SerializeField] CHTMPro select1Text;
    [SerializeField] CHTMPro select1PriceText;
    [SerializeField] Button select2Btn;
    [SerializeField] CHTMPro select2Text;
    [SerializeField] CHTMPro select2PriceText;

    SelectInfo select1Info;
    SelectInfo select2Info;

    bool checkMaxSpeed = false;
    bool checkMinDelay = false;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIChoiceArg;
    }

    private void Start()
    {
        Time.timeScale = 0;

        do
        {
            var select1 = (Defines.ESelect)Random.Range(0, (int)Defines.ESelect.Max);
            select1Info = CHMMain.Json.GetSelectInfo(select1);
            select1Text.SetStringID(select1Info.titleStr);
            select1Text.SetText(select1Info.value);
            select1PriceText.SetText(select1Info.scoreCost);

            if (select1 == Defines.ESelect.Speed)
            {
                if (checkMaxSpeed == false)
                    break;
            }
            else if (select1 == Defines.ESelect.Delay)
            {
                if (checkMinDelay == false)
                    break;
            }
            else
            {
                break;
            }
        } while (true);

        do
        {
            var select2 = (Defines.ESelect)Random.Range(0, (int)Defines.ESelect.Max);
            select2Info = CHMMain.Json.GetSelectInfo(select2);
            select2Text.SetStringID(select2Info.titleStr);
            select2Text.SetText(select2Info.value);
            select2PriceText.SetText(select2Info.scoreCost);

            if (select1Info != select2Info)
            {
                if (select2 == Defines.ESelect.Speed)
                {
                    if (checkMaxSpeed == false)
                        break;
                }
                else if (select2 == Defines.ESelect.Delay)
                {
                    if (checkMinDelay == false)
                        break;
                }
                else
                {
                    break;
                }
            }
        } while (true);

        backBtn.OnClickAsObservable().Subscribe(_ =>
        {
            Time.timeScale = 1;
            CHMMain.UI.CloseUI(gameObject);
        });

        select1Btn.OnClickAsObservable().Subscribe(_ =>
        {
            SetSelectInfo(select1Info);
        });

        select2Btn.OnClickAsObservable().Subscribe(_ =>
        {
            SetSelectInfo(select2Info);
        });
    }

    void SetSelectInfo(SelectInfo _selectInfo)
    {
        if (arg.curScore.Value >= _selectInfo.scoreCost)
        {
            switch (_selectInfo.eSelect)
            {
                case Defines.ESelect.Power:
                    {
                        if (arg.attackCatList.First().attackPower >= arg.maxPower)
                        {
                            CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                            {
                                alarmText = $"Power Is Max"
                            });
                            break;
                        }

                        foreach (var cat in arg.attackCatList)
                        {
                            cat.attackPower += (int)_selectInfo.value;
                        }

                        arg.curScore.Value -= _selectInfo.scoreCost;
                        arg.power.Value = arg.attackCatList.First().attackPower;
                    }
                    break;
                case Defines.ESelect.Delay:
                    {
                        if (arg.attackCatList.First().attackDelay <= arg.minDealy)
                        {
                            checkMinDelay = true;
                            CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                            {
                                alarmText = $"Delay Is Min"
                            });
                            break;
                        }

                        var value = Mathf.Max(arg.attackCatList.First().attackDelay - _selectInfo.value, arg.minDealy);
                        if (Mathf.Approximately(value, arg.minDealy))
                        {
                            checkMinDelay = true;
                        }

                        foreach (var cat in arg.attackCatList)
                        {
                            cat.attackDelay = value;
                        }

                        arg.curScore.Value -= _selectInfo.scoreCost;
                        arg.delay.Value = arg.attackCatList.First().attackDelay;
                    }
                    break;
                case Defines.ESelect.Lotto:
                    {
                        arg.curScore.Value -= _selectInfo.scoreCost;
                        arg.curScore.Value += (int)_selectInfo.value;

                        CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                        {
                            alarmText = $"{(int)_selectInfo.value}Gold Acquired"
                        });
                    }
                    break;
                case Defines.ESelect.AddCat:
                    {
                        bool addCat = false;
                        foreach (var cat in arg.attackCatList)
                        {
                            if (cat.gameObject.activeSelf == false)
                            {
                                addCat = true;
                                cat.gameObject.SetActive(true);
                                break;
                            }
                        }

                        if (addCat == false)
                        {
                            CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                            {
                                alarmText = $"Cat Is Max"
                            });
                        }
                        else
                        {
                            arg.curScore.Value -= _selectInfo.scoreCost;
                        }
                    }
                    break;
                case Defines.ESelect.CatPangUpgrade:
                    {
                        arg.curScore.Value -= _selectInfo.scoreCost;

                        foreach (var cat in arg.attackCatList)
                        {
                            if (cat.attackImg.sprite.name == Defines.ESpecailBlockType.CatPang1.ToString())
                            {
                                
                                cat.attackImg.sprite = arg.catPangImgList[(int)Defines.ESpecailBlockType.CatPang2];
                                cat.attackPower += 500;
                                cat.attackSpeed += 5;
                            }
                            else if (cat.attackImg.sprite.name == Defines.ESpecailBlockType.CatPang2.ToString())
                            {
                                arg.curScore.Value -= _selectInfo.scoreCost;
                                cat.attackImg.sprite = arg.catPangImgList[(int)Defines.ESpecailBlockType.CatPang3];
                                cat.attackPower += 1000;
                                cat.attackSpeed += 10;
                            }
                            else if (cat.attackImg.sprite.name == Defines.ESpecailBlockType.CatPang3.ToString())
                            {
                                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                                {
                                    alarmText = $"CatPang3 Is Max"
                                });

                                arg.curScore.Value += _selectInfo.scoreCost;

                                break;
                            }

                            arg.power.Value = arg.attackCatList.First().attackPower;
                            arg.delay.Value = arg.attackCatList.First().attackDelay;
                        }
                    }
                    break;
                case Defines.ESelect.Speed:
                    {
                        if (arg.attackCatList.First().attackSpeed >= arg.maxSpeed)
                        {
                            checkMaxSpeed = true;
                            CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                            {
                                alarmText = $"Speed Is Max"
                            });
                            break;
                        }

                        var value = Mathf.Min(arg.attackCatList.First().attackSpeed + _selectInfo.value, arg.maxSpeed);
                        if (value == arg.maxSpeed)
                        {
                            checkMaxSpeed = true;
                        }

                        foreach (var cat in arg.attackCatList)
                        {
                            cat.attackSpeed = value;
                        }

                        arg.curScore.Value -= _selectInfo.scoreCost;
                        arg.speed.Value = arg.attackCatList.First().attackSpeed;
                    }
                    break;
            }

            Time.timeScale = 1;
            CHMMain.UI.CloseUI(gameObject);
        }
        else
        {
            CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
            {
                alarmText = "Not Enough Gold"
            });
        }
    }
}
