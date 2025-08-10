using UnityEngine;
using UnityEngine.InputSystem;

public static class InputProvider
{
    #region Fields
    private static InputSystem_Actions _inputActions;
    #endregion

    #region Properties
    public static InputAction JumpInputAction { get; private set; }
    public static InputAction ForceFallInputAction { get; private set; }
    public static InputAction EscapeInputAction { get; private set; }
    public static InputSystem_Actions.PlayerActions PlayerInputAction => _inputActions.Player;
    
    public static bool PlayerInputEnabled
    {
        set
        {
            if (value)
                _inputActions.Player.Enable();
            else
                _inputActions.Player.Disable();
        }
    }
    public static bool UIInputEnabled
    {
        set
        {
            if(value)
                _inputActions.UI.Enable();
            else
                _inputActions.UI.Disable();
        }
    }
    #endregion

    #region Constuctor
    static InputProvider()
    {
        if (Application.isPlaying)
        {
            Initialize();
        }
    }
    #endregion
    
    #region Methods
    private static void Initialize()
    {
        _inputActions = new InputSystem_Actions();
        JumpInputAction = _inputActions.Player.Jump;
        ForceFallInputAction = _inputActions.Player.ForceFall;
        EscapeInputAction = _inputActions.UI.Escape;
        _inputActions.Enable();
        PlayerInputEnabled = false;
        UIInputEnabled = false;
    }
    #endregion
}
