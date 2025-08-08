using UnityEngine;

public class BonusContainer : MonoBehaviour
{
    #region Properties
    [field: Header("Bonus scriptable link."), SerializeField]
    public BonusScriptable Bonus { get; set; }
    #endregion
}