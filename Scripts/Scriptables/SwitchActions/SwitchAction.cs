using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchAction : ScriptableObject
{
    public virtual void Perform(SwitchTarget target){}
}
