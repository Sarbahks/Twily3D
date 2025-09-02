using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

/// <summary>
/// Manages a single salon entry in the lobby list.
/// Handles Join, Leave, and Delete button callbacks via provided actions.
/// </summary>
public class SalonItem : MonoBehaviour
{
    [SerializeField]
    private string textStatutGameNull = "Non débutée";
    [SerializeField]
    private string textStatutGameOngoing = "En cours";
    [SerializeField]
    private string textStatutGameEnded = "Terminée";


    [SerializeField]
    private TextMeshProUGUI salonName;

    [SerializeField]
    private TextMeshProUGUI participants;
        [SerializeField]
    private TextMeshProUGUI state;

    private SalonInfo info;


    public void SetupSalonItem(SalonInfo salon)
    {
        info = salon;

        salonName.text = salon.Name;

        if(salon.UsersInSalon == null || salon.UsersInSalon.Count == 0)
        {
            participants.text = "Aucun participant";
        }
        else
        {
            participants.text = salon.UsersInSalon.ToArray().ToString();
        }


        if(salon.GameState == null)
        {
            state.text = textStatutGameNull;
        }
        else
        {
            if(salon.GameState.Completed)
            {
                state.text = textStatutGameEnded;
            }
            else
            {
                state.text = textStatutGameOngoing;
            }
        }
    }

    public void JoinSalon()
    {
        string id = string.Empty;
        if(info?.Id != null)
        {
            id = info.Id;
        }
        LobbySceneManager.Instance.JoinSalon(id);
    }

    public void DeleteSalon()
    {
        string id = string.Empty;
        if (info?.Id != null)
        {
            id = info.Id;
        }
        LobbySceneManager.Instance.DeleteSalon(id);
    }
}
