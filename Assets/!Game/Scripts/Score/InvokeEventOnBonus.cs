using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class InvokeEventOnBonus : MonoBehaviour
{
    [Header("Bonus types and events."), SerializeField]
    private BonusTypeEvent[] _bonusEvents =  Array.Empty<BonusTypeEvent>();

    public void CheckBonusTypeOnStartAndInvoke(BonusType Type)
    {
        if(_bonusEvents.Length == 0)
            return;
        
        IEnumerable<BonusTypeEvent> events = _bonusEvents.Where(bonus => bonus.Type == Type);

        if (events.Count() > 0)
        {
            events.First().OnBonusStartedEvent?.Invoke();
        }
    }
    
    public void CheckBonusTypeOnEndAndInvoke(BonusType Type)
    {
        if(_bonusEvents.Length == 0)
            return;
        
        IEnumerable<BonusTypeEvent> events = _bonusEvents.Where(bonus => bonus.Type == Type);

        if (events.Count() > 0)
        {
            events.First().OnBonusEndedEvent?.Invoke();
        }
    }
}

[Serializable]
public class BonusTypeEvent
{
    [field: Header("Bonus type."), SerializeField]
    public BonusType Type { get; set; }
    
    [field: Header("Event called when bonus with type started."), SerializeField]
    public UnityEvent OnBonusStartedEvent { get; set; }
    
    [field: Header("Event called when bonus with type ended."), SerializeField]
    public UnityEvent OnBonusEndedEvent { get; set; }
}