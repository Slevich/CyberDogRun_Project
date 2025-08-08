using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FullscreenToggle : MonoBehaviour
{
    [SerializeField] private Button fullscreenButton;
    [SerializeField] private Sprite fullscreenIcon;  
    [SerializeField] private Sprite windowedIcon;    

    private void Start()
    {
        UpdateButtonIcon();
        Screen.fullScreen = false;
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        UpdateButtonIcon();
    }

    private void UpdateButtonIcon()
    {
        if (fullscreenButton == null) 
            return;

        if (Screen.fullScreen)
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
