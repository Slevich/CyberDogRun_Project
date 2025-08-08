using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class DataSender : MonoBehaviour
{
    #region Fields
    [Header("Input field to read information."), SerializeField]
    private TMP_InputField _nicknameInputField;

    [Header("Event called when data succeed sent."), SerializeField]
    private UnityEvent _onSuccessSendingEvent;
    
    [Header("Event called when data sent with an error."), SerializeField]
    private UnityEvent _onErrorSendingEvent;
    #endregion

    #region Methods
    public async void SendData()
    {
        if (_nicknameInputField == null)
        {
            Debug.LogError("No Nickname field assigned.");
            return;
        }
        
        if (_nicknameInputField.text != String.Empty)
        {
            UnityWebRequest.Result result = await LeaderboardDataManager.PostScore(_nicknameInputField.text, ScoreCounter.Instance.CurrentScore);
            
            if(result == UnityWebRequest.Result.Success)
                _onSuccessSendingEvent?.Invoke();
            else
                _onErrorSendingEvent?.Invoke();
        }
        else
        {
            Debug.LogError("Nickname field is empty!");
            _onErrorSendingEvent?.Invoke();
        }
    }
    #endregion
}
