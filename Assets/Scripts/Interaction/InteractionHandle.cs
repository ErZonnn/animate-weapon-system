using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractionHandle : MonoBehaviour
{
    [SerializeField] private UnityEvent _interactionEvent;

    public void TriggerInteractions()
    {
        _interactionEvent.Invoke();
    }
}
