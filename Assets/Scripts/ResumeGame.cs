using System.Linq;
using TMPro;
using UnityEngine;

public class ResumeGame : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI salonName;

    [SerializeField]
    private TextMeshProUGUI dateDebut;
    
    [SerializeField]
    private TextMeshProUGUI dernierTour;
    
    [SerializeField]
    private TextMeshProUGUI points;
    
    [SerializeField]
    private TextMeshProUGUI termine;   
    
    [SerializeField]
    private TextMeshProUGUI timeTermine;
    
    [SerializeField]
    private TextMeshProUGUI shared;

    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    private TextMeshProUGUI cartesJouées;

    [SerializeField]
    private TextMeshProUGUI anctiveArea;


    [SerializeField]
    private Transform playerArea;
    public void SetupResumeGame(GameStateData gameState, string salonName)
    {
        this.salonName.text = salonName;
       // dateDebut.text = gameState.StartGame.ToString();
       // dernierTour.text = gameState.TimeLastTurn.ToString();
        points.text = gameState.TotalScore.ToString();
      //  termine.text = gameState.Completed ? "Oui": "Non";
      //  timeTermine.text = gameState.EndedTime.ToString();
        shared.text = gameState.SharedMessage;

        string cardsUnlocked = gameState.Board.Where(x => x.Unlocked).ToList().Count.ToString();
        string totalCard = gameState.Board.Count.ToString() ;

        cartesJouées.text = cardsUnlocked  +"/"+ totalCard;
        /*
        foreach (var player in gameState.Players)
        {
            if (player.userInfo == null)
                continue;

            var go = Instantiate(playerPrefab, playerArea);
            var script = go.GetComponent<ParticipantResumeItem>();
            if(script != null)
            {
                script.SetupParticipant(player.userInfo.Name, player.score);
            }
       
        }*/
    }

}
