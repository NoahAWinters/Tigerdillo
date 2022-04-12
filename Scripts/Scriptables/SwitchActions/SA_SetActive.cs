using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SA_SetActive : SwitchAction
{
    [SerializeField] private bool _toggle;

    public override void Perform(SwitchTarget target)
    {
        target.gameObject.SetActive(_toggle);
    }
}
