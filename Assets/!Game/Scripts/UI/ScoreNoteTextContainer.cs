using TMPro;
using UnityEngine;

public class ScoreNoteTextContainer : MonoBehaviour
{
   [field: Header("Player name text."), SerializeField]
   public TextMeshProUGUI PlayerNameText { get; set; }

   [field: Header("Score text."), SerializeField]
   public TextMeshProUGUI ScoreText { get; set; }
}
