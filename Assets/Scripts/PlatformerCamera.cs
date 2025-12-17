using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformerCamera : MonoBehaviour
{
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private CinemachinePanTilt _panTilt;
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private PlayerController playerController;

    private InputSystem_Actions.PlayerActions _playerInput;

    public event Action<Vector2> Move;
    public event Action<Vector2> Look;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _panTilt = vcam.GetComponent<CinemachinePanTilt>();
        var inputActions = new InputSystem_Actions();
        _playerInput = inputActions.Player;
        AddListeners();
        _playerInput.Enable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        var delta = ctx.ReadValue<Vector2>();
        Move?.Invoke(delta);
    }

    private void OnLook(InputAction.CallbackContext ctx)
    {
        var delta = ctx.ReadValue<Vector2>() * lookSensitivity; 
        var tilt = _panTilt.TiltAxis.Value + delta.y * -1f;
        
        _panTilt.PanAxis.Value += delta.x;
        _panTilt.TiltAxis.Value = _panTilt.TiltAxis.ClampValue(tilt);
        
        Look?.Invoke(delta);
    }

    private void OnDisable()
    {
        RemoveListeners();
        _playerInput.Disable();
    }

    private void Update()
    {
        var dt = _playerInput.Move.ReadValue<Vector2>();
        playerController.Move(dt);
    }

    private void AddListeners()
    {
        _playerInput.Move.performed += OnMove;
        _playerInput.Look.performed += OnLook;
    }

    private void RemoveListeners()
    {
        _playerInput.Move.performed -= OnMove;
        _playerInput.Look.performed -= OnLook;
    }
}
