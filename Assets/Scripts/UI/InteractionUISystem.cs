using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionUISystem : MonoBehaviour
{
    [SerializeField] private GameObject _interactionUIObject;
    [SerializeField] private Transform _playerCamera;

    public void ShowInteractionUI(GameObject interactObject)
    {
        if(interactObject == null)
        {
            _interactionUIObject.SetActive(false);
            return;
        }

        _interactionUIObject.SetActive(true);

        _interactionUIObject.transform.position = interactObject.transform.position;
        _interactionUIObject.transform.rotation = Quaternion.LookRotation(_interactionUIObject.transform.position - _playerCamera.position);
    }
}
