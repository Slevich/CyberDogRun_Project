using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FullscreenToggle : MonoBehaviour
{
    [SerializeField] private Button fullscreenButton;
    [SerializeField] private Sprite fullscreenIcon;  
    [SerializeField] private Sprite windowedIcon;    

    private bool _currentFullscreenState = false;
    
    private void Start()
    {
        UpdateButtonIcon();
    }

    public void ToggleFullscreen()
    {
        _currentFullscreenState = !_currentFullscreenState;
        UpdateButtonIcon(_currentFullscreenState);
        Screen.fullScreen = _currentFullscreenState;
    }

    private void UpdateButtonIcon(bool fullScreenState = false)
    {
        if (fullscreenButton == null) 
            return;

        if (fullScreenState)
        {
            fullscreenButton.image.sprite = windowedIcon;
        }
        else
        {
            fullscreenButton.image.sprite = fullscreenIcon;
        }
    }

    private void OnEnable() => Screen.fullScreen = false;
}
