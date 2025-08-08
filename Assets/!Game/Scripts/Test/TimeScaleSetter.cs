using UnityEditor;
using UnityEngine;

public class TimeScaleSetter : MonoBehaviour
{
    [SerializeField] 
    private float _timeScale = 1;

    public void SetTimeScale()
    {
        if(!Application.isPlaying)
            return;
        
        Time.timeScale = _timeScale;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TimeScaleSetter))]
public class TimeScaleSetterEditor : Editor
{
    override public void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        serializedObject.Update();
        
        bool setTimeScaleButtonPressed = GUILayout.Button("Set Time Scale");
        if (setTimeScaleButtonPressed)
        {
            TimeScaleSetter setter = (TimeScaleSetter)target;
            setter.SetTimeScale();
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}
#endif