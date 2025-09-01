using TMPro;
using UnityEngine;

public class ParticipantResumeItem : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameParticipant; 
    [SerializeField]
    private TextMeshProUGUI pointsParticipant;

    public void SetupParticipant(string name, int points)
    {
        nameParticipant.text = name;
        pointsParticipant.text = points.ToString();
    }
}
