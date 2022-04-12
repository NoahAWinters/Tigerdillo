using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTarget : MonoBehaviour
{
    //Switch Action SO
    [SerializeField] SwitchAction _awakeAction;
    [SerializeField] SwitchAction _toggleAction;

    void Awake()
    {
        _awakeAction.Perform(this);
    }

    public void PerformSwitchAction()
    {
        _toggleAction.Perform(this);
    }

    public void UndoSwitchAction()
    {
        _awakeAction.Perform(this);
    }
}
