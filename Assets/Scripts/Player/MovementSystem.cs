/*Created by Pawe³ Mularczyk*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

#if UNITY_EDITOR
using UnityEditor;
using System.Net;
#endif

[RequireComponent(typeof(CharacterController))]
public class MovementSystem : MonoBehaviour
{
    [SerializeField] private bool _debug = false;
    [Header("MOVEMENT SETTINGS")]
    [SerializeField] private float _moveSpeed = 12f;
    [SerializeField] private float _moveInertia = 9f;
    [SerializeField] private float _jumpForce = 5f;
    [Space]
    [SerializeField] private float _gravityValue = -25f;
    [SerializeField] private float _gravityAccel = 1.3f;
    [Space]
    [SerializeField] private Vector3 _groundCheckerSize = new Vector3(0.6f, 0.2f, 0.6f);
    [SerializeField] private Vector3 _groundCheckerPosition = new Vector3(0, 0, 0);
    [SerializeField] private LayerMask _groundLayerMask;

    [Space]
    [Header("LOOK SETTINGS")]
    [SerializeField] private Transform _playerCamera;
    [SerializeField] private Sensitivity _sensitivity;
    [Space]
    [SerializeField] private Vector2 xRotationLimit = new Vector2(-80f, 80f);
    [Space]
    [SerializeField] private AnimationCurve _cameraMoveAnimCurve;
    [SerializeField] private float _animCurvePower = 1f;
    [SerializeField] private float _animCurveSpeed = 0.8f;

    [Space]
    [Header("HANDS SWAY SETTINGS")]
    [SerializeField] private float _smooth = 15f;
    [SerializeField] private float _input = 0.05f;
    [SerializeField] private float _inputMove = 0.04f;
    [SerializeField] private float _maxSwayLook = 0.8f;
    [SerializeField] private float _maxSwayMove = 0.8f;

    #region PRIVATE VARIABLES

    private CharacterController _controller;
    private WeaponController _weaponController;

    private InputMaster _inputMaster;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;

    public Transform _handsTransform;

    private Vector2 _moveInput;
    private Vector2 _lookInput;

    private Vector2 _swayLook;
    private Vector2 _swayMove;

    private Vector3 _moveVel;
    private Vector3 _gravityVel;

    private Vector3 _handsStartPosition;
    private Vector3 _handsRefPosition;

    private float _xCamRotation;
    private float _gamepadLookInputX;
    private float _curveTime;
    
    private bool _isJumping;
    private bool _jumpInput;

    private bool _useGamepad = false;

    #endregion

    private void Awake()
    {
        _inputMaster = new InputMaster();
    }

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _sensitivity = (Sensitivity) FindObjectOfType(typeof(Sensitivity));
        _weaponController = GetComponent<WeaponController>();
    }

    private void OnEnable()
    {
        _moveAction = _inputMaster.Player.Move;
        _lookAction = _inputMaster.Player.Look;
        _jumpAction = _inputMaster.Player.Jump;

        _jumpAction.Enable();
        _moveAction.Enable();
        _lookAction.Enable();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _lookAction.Disable();
        _jumpAction.Disable();
    }

    private void Update()
    {
        InputHandle();

        if (IsGrounded() && _isJumping)
            _isJumping = false;
    }

    private void FixedUpdate()
    {
        Gravity();
        Move();
        Look();
        HandsSway();
    }

    public void SwitchHands(Transform hands, Vector3 startPosition)
    {
        _handsTransform = hands;
        _handsStartPosition = startPosition;
    }

    private void HandsSway()
    {
        if (_handsTransform == null)
            return;

        _swayLook.x = Mathf.Clamp(_lookInput.x, -_maxSwayLook, _maxSwayLook);
        _swayLook.y = Mathf.Clamp(_lookInput.y, -_maxSwayLook, _maxSwayLook);

        _swayMove.x = Mathf.Clamp(_moveInput.x, -_maxSwayMove, _maxSwayMove);
        _swayMove.y = Mathf.Clamp(_moveInput.y, -_maxSwayMove, _maxSwayMove);

        Vector3 finalPos;
        Vector3 targetPos;
        float targetInput;
        float targetInputMove;

        if (_weaponController.GetAimingStatus())
        {
            targetInput = _input / 2f;
            targetInputMove = _inputMove;

            finalPos = new Vector3((_swayLook.x + _swayMove.x) * -targetInput, (_swayLook.y * -targetInput) + (CalculateAnimCurve() * -targetInputMove), _swayMove.y * -targetInputMove);
            targetPos = _weaponController.GetAimingLocalPosition();
        }
        else
        {
            targetInput = _input;
            targetInputMove = _inputMove;

            finalPos = new Vector3((_swayLook.x + _swayMove.x) * -targetInput, (_swayLook.y * -targetInput) + ((CalculateAnimCurve() * 3f) * -targetInputMove), _swayMove.y * (CalculateAnimCurve() * 3f) * -targetInputMove);
            targetPos = _handsStartPosition;
        }

        _handsTransform.localPosition = Vector3.SmoothDamp(_handsTransform.localPosition, finalPos + targetPos, ref _handsRefPosition, _smooth * Time.fixedDeltaTime);
    }

    private void Look()
    {
        _xCamRotation -= LookDirection().y;
        _xCamRotation = Mathf.Clamp(_xCamRotation, xRotationLimit.x, xRotationLimit.y);

        if(_moveInput.magnitude > 0.1f)
        {
            
            _playerCamera.localPosition += (transform.up * CalculateAnimCurve()) * _moveInput.magnitude * Time.fixedDeltaTime;
            _curveTime += Time.fixedDeltaTime * _animCurveSpeed;
        }

        _playerCamera.localRotation = Quaternion.Euler(_xCamRotation, 0, 0);
        transform.Rotate(Vector2.up * LookDirection().x);
    }

    private float CalculateAnimCurve()
    {
        float curveValue = _cameraMoveAnimCurve.Evaluate(_curveTime);
        return curveValue * _animCurvePower;
    }

    private Vector2 LookDirection()
    {
        if (_useGamepad)
        {
            if (_lookInput.x > 0.7f || _lookInput.x < -0.7f)
            {
                _gamepadLookInputX = Mathf.Lerp(_gamepadLookInputX, _lookInput.x * _sensitivity.GetSensitivity(ControllerType.Gamepad).x * 3f, 1.5f * Time.fixedDeltaTime);
            }
            else
            {
                _gamepadLookInputX = _lookInput.x * _sensitivity.GetSensitivity(ControllerType.Gamepad).x;
            }

            Vector2 gamepadLook = new Vector2(_gamepadLookInputX, _lookInput.y * _sensitivity.GetSensitivity(ControllerType.Gamepad).y) * Time.fixedDeltaTime;
            return gamepadLook;
        }

        Vector2 mouseLook = new Vector2(_lookInput.x * _sensitivity.GetSensitivity(ControllerType.Mouse).x, _lookInput.y * _sensitivity.GetSensitivity(ControllerType.Mouse).y) * Time.fixedDeltaTime;
        return mouseLook;
    }

    public void CheckCurrentControlScheme(PlayerInput playerInput)
    {
        _useGamepad = playerInput.currentControlScheme.Equals("Gamepad") ? true : false;
    }

    private void Move()
    {
        Vector3 moveDir = transform.right * _moveInput.x + transform.forward * _moveInput.y;

        if (IsGrounded())
        {
            _moveVel = Vector3.Lerp(_moveVel, moveDir * _moveSpeed, _moveInertia * Time.fixedDeltaTime);
        }
        else
        {
            _moveVel = Vector3.Lerp(_moveVel, moveDir / 1.5f * _moveSpeed, _moveInertia / 2 * Time.fixedDeltaTime);
        }

        _controller.Move(_moveVel * Time.fixedDeltaTime);
    }

    private void Gravity()
    {
        if (IsGrounded())
        {
            _gravityVel = Vector3.down * 10f;
        }
        else
        {
            _gravityVel += Vector3.up * _gravityValue * (Time.fixedDeltaTime * _gravityAccel);
        }

        if (_jumpInput)
            _gravityVel = Jump(_gravityVel);

        _controller.Move(_gravityVel * Time.fixedDeltaTime);
    }

    private Vector3 Jump(Vector3 inputVel)
    {
        if (IsGrounded())
        {
            _isJumping = true;
            Vector3 jumpVel = new Vector3(inputVel.x, Mathf.Sqrt(inputVel.y * -3 * _jumpForce), inputVel.z);
            return jumpVel;
        }
        else
            return inputVel;
    }

    private bool IsGrounded()
    {
        return Physics.CheckBox(transform.position + _groundCheckerPosition, new Vector3(_groundCheckerSize.x / 2, _groundCheckerSize.y / 2, _groundCheckerSize.z / 2), transform.rotation, _groundLayerMask);
    }

    private void InputHandle()
    {
        _moveInput = _weaponController.GetAimingStatus() == true ? _moveAction.ReadValue<Vector2>() / 2f : _moveAction.ReadValue<Vector2>();
        _lookInput = _lookAction.ReadValue<Vector2>();
        _jumpInput = _jumpAction.ReadValue<float>() > 0.05f ? true : false;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_debug)
        {
            if (IsGrounded())
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawWireCube(transform.position + _groundCheckerPosition, _groundCheckerSize);
        }
    }
#endif
}

#if UNITY_EDITOR

[CustomEditor(typeof(MovementSystem)), InitializeOnLoadAttribute]
public class MovementSystemEditor : Editor
{
    MovementSystem _ms;
    SerializedObject _serFPC;

    private void OnEnable()
    {
        _ms = (MovementSystem)target;
        _serFPC = new SerializedObject(_ms);
    }

    public override void OnInspectorGUI()
    {
        _serFPC.Update();

        EditorGUILayout.Space();
        GUILayout.Label("Movement System", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 20 });
        GUILayout.Label("Created by Pawe³ Mularczyk", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 12 });
        GUILayout.Label("Version: Modified", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 12 });
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        base.OnInspectorGUI();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_ms);
            Undo.RecordObject(_ms, "Settings Change");
            _serFPC.ApplyModifiedProperties();
        }
    }
}
#endif
