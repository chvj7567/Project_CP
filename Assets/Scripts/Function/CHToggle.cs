using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

[RequireComponent(typeof(Toggle))]
public class CHToggle : MonoBehaviour
{
    [NonSerialized]
    public Toggle toggle;
    bool first = true;

    private void Start()
    {
        toggle = GetComponent<Toggle>();

        toggle.OnValueChangedAsObservable().Subscribe(_ =>
        {
            if (first == true)
            {
                first = false;
            }
            else
            {
                CHMMain.Sound.Play(Defines.ESound.Button);
            }
        });
    }
}