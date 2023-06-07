using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionSystem : MonoBehaviour
{
    [SerializeField] private Transform _playerCamera;
    [SerializeField] private LayerMask _interactionLayerMask;
    [SerializeField] private float _interactRadius;
    [SerializeField] private float _maxInteractAngel;

    private InteractionUISystem _interactionUI;

    private InputMaster _inputMaster;
    private InputAction _interactAction;

    private bool _interactInput;

    private void Awake()
    {
        _inputMaster = new InputMaster();
    }

    private void OnEnable()
    {
        _interactAction = _inputMaster.Player.Interaction;

        _interactAction.Enable();
    }

    private void OnDisable()
    {
        _interactAction.Disable();
    }

    private void Start()
    {
        _interactionUI = GetComponent<InteractionUISystem>();
    }

    private void Update()
    {
        InputHandle();
        Interaction();
    }

    private void InputHandle()
    {
        _interactInput = _interactAction.ReadValue<float>() > 0.1f ? true : false;
    }

    private void Interaction()
    {
        GameObject interactObject = CheckObjects();

        _interactionUI.ShowInteractionUI(interactObject);

        if (interactObject == null)
            return;

        if (_interactInput)
        {
            interactObject.GetComponent<InteractionHandle>().TriggerInteractions();
            return;
        }
    }

    private GameObject CheckObjects()
    {
        Collider[] checkingObjects = Physics.OverlapSphere(transform.position, _interactRadius, _interactionLayerMask);

        for(int i = 0; i < checkingObjects.Length; i++)
        {
            if (CheckAngel(checkingObjects[i].gameObject.transform) <= _maxInteractAngel)
                return checkingObjects[i].gameObject;
        }

        return null;
    }

    private float CheckAngel(Transform checkingObject)
    {
        Vector3 dir = checkingObject.position - _playerCamera.position;
        float angel = Vector3.Angle(_playerCamera.forward, dir);

        return angel;
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _interactRadius);
    }

#endif
}
