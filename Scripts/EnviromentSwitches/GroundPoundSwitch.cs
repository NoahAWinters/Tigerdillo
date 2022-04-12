using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundPoundSwitch : MonoBehaviour
{
    //Reference Variables
    bool _isToggled; //Determines if switch is pressed, to prevent over pressing
    [SerializeField] SwitchTarget _target; //What will be affected by the switch
    [SerializeField] SwitchTimer _timer; //If the timer is set to on

    int _switchID;

    Vector3 _changeInPosition = new Vector3(0, .15f, 0);

    void Awake()
    {
        _isToggled = false;
    }

    void OnTriggerEnter(Collider coll)
    {
        if(coll.gameObject.tag == "Player")
		{
            if(coll.GetComponent<PlayerActions>().IsGroundPounding)
            {
                if(_target != null && !_isToggled) //Make sure the switch activates something, and that its not already pressed;
                {
                    ActivateSwitch();
                }
            }
        }
    }

    void ActivateSwitch()
    {
        if(!_isToggled)
        {
            _isToggled = true;
            _target.PerformSwitchAction();
            
            transform.position -= _changeInPosition;

            if(_timer.UseTimer)
            {
                StartCoroutine(StartTimer());
            }
        }
    }

    void DeactivateSwitch()
    {
        if(_target != null)
        {
            _isToggled = false;
            _target.UndoSwitchAction();
        
            transform.position += _changeInPosition;
        }
    }

    IEnumerator StartTimer()
    {
        float startTime = Time.time;

        while(Time.time < startTime + _timer.TimerTime.Value)
        {
            yield return null;
        }

        DeactivateSwitch();
    }

    public void ChangeTarget(SwitchTarget newTarget)
    {
        _target = newTarget;
    }

}
