using UnityEngine;

public class InputManager : MonoBehaviourSingleton<InputManager>
{
    private bool _inputBlocked = false;

    public bool IsInputBlocked => _inputBlocked;

    public void BlockInput(bool block)
    {
        _inputBlocked = block;
    }
}