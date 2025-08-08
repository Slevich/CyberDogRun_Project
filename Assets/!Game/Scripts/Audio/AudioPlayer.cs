using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    #region Fields
    [SerializeField]
    private AudioSource _audioSource;
    #endregion

    #region Methods

    public void SetClipAndPlay(AudioClip Clip)
    {
        if(_audioSource == null)
            return;
        
        _audioSource.clip = Clip;
        _audioSource.Play();
    }
    #endregion
}
