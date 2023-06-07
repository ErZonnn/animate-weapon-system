using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorControl : MonoBehaviour
{
    [SerializeField] private Animator _doorAnimator;
    [SerializeField] private float _interactionCoolDown;

    private bool _isOpen = false;
    private bool _isInteract = false;

    private void Update()
    {
        _doorAnimator.SetBool("_open", _isOpen);
    }

    public void DoorHandle()
    {
        if (!_isInteract)
        {
            _isOpen = !_isOpen;
            _isInteract = true;
            StartCoroutine(InteractCoolDown(_interactionCoolDown));
        }

        return;
    }

    private IEnumerator InteractCoolDown(float time)
    {
        yield return new WaitForSeconds(time);
        _isInteract = false;
    }
}
