using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchArray : SwitchTarget
{
    [SerializeField] SwitchTarget _secondaryTarget;

    int[] _expectedOrder;
    public int _place;
}
