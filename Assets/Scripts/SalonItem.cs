using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

/// <summary>
/// Manages a single salon entry in the lobby list.
/// Handles Join, Leave, and Delete button callbacks via provided actions.
/// </summary>
public class SalonItem : MonoBehaviour
{/*
    [Header("UI References")]
    public TMP_Text salonIdText;
    public TwilyButton joinButton;
  //  public TwilyButton leaveButton;
    public TwilyButton deleteButton;

    private string salonId;
    private Action<string> onJoin;
    private Action<string> onLeave;
    private Action<string> onDelete;

    /// <summary>
    /// Read-only access to this item's salon ID.
    /// </summary>
    public string SalonId => salonId;

    /// <summary>
    /// Configure this item:
    /// - Displays the salon ID
    /// - Shows/hides delete button based on isAdmin
    /// - Wires up join/leave/delete callbacks
    /// </summary>
    /// <param name="salonId">Identifier of the salon.</param>
    /// <param name="isAdmin">Whether the current user is an admin; controls delete button visibility.</param>
    /// <param name="onJoin">Callback invoked when Join is clicked.</param>
    /// <param name="onLeave">Callback invoked when Leave is clicked.</param>
    /// <param name="onDelete">Callback invoked when Delete is clicked.</param>
    public void Setup(
        string salonId,
        bool isAdmin,
        Action<string> onJoin,
        Action<string> onLeave,
        Action<string> onDelete)
    {
        this.salonId = salonId;
        this.onJoin = onJoin;
        this.onLeave = onLeave;
        this.onDelete = onDelete;

        // Update UI
        salonIdText.text = salonId;
        deleteButton.gameObject.SetActive(isAdmin);

        // Clear previous listeners
        joinButton.OnClick.RemoveAllListeners();
   //     leaveButton.onClick.RemoveAllListeners();
        deleteButton.OnClick.RemoveAllListeners();

        // Wire new listeners
        joinButton.OnClick.AddListener(() => this.onJoin?.Invoke(salonId));
    //    leaveButton.onClick.AddListener(() => this.onLeave?.Invoke(salonId));
        deleteButton.OnClick.AddListener(() => this.onDelete?.Invoke(salonId));
    }*/
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
