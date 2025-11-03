using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    // 输入动作资产
    private GamePlayerInput inputActionAsset;

    public GamePlayerInput InputActionAsset { get => inputActionAsset; set => inputActionAsset = value; }

    private void Awake()
    {
        // 初始化输入控制系统
        InputActionAsset = new GamePlayerInput();
    }
    
    private void OnEnable()
    {
        InputActionAsset.Enable();
    }
    
    private void OnDisable()
    {
        InputActionAsset.Disable();
    }
}