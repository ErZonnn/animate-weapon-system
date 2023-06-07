using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyController : MonoBehaviour
{
    [SerializeField] private Animator _bodyAnimator;

    public void BodyAnimationControl(Vector2 input)
    {
        _bodyAnimator.SetBool("_move", input.magnitude > 0 ? true : false);
        _bodyAnimator.SetFloat("_inputHorizontal", input.x);
        _bodyAnimator.SetFloat("_inputVertical", input.y);
    }
}
