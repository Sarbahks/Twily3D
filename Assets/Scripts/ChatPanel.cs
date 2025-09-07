using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatPanel : MonoBehaviour
{
    private static ChatPanel instance;
    public static ChatPanel Instance
        { get {
            if(instance == null)
            {
                instance = FindAnyObjectByType<ChatPanel>();
            }
            return instance; 
        } }


    [Header("Wiring")]
    public RectTransform content;        // ScrollView/Viewport/Content
    public ScrollRect scrollRect;        // the ScrollRect
    public TMP_InputField inputField;
    public Button sendButton;
    public GameObject chatItemPrefab;    // ChatMessageItem prefab



    private void Awake()
    {
      
        if (sendButton != null) sendButton.onClick.AddListener(OnClickSend);
        if (inputField != null) inputField.onSubmit.AddListener(_ => OnClickSend());
    }

    public void OnClickSend()
    {
        var text = inputField.text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        // Ask LobbySceneManager to send

        //
        ChatDTO chatDTO = new ChatDTO
        {
            FromId = Authentificator.Instance.Id,
            FromName = Authentificator.Instance.Username,
            Target = new ChatTarget
            {
                TypeChatTarget = chatTarget,
                IdTarget = idTarget,
                StringIdSalon = LobbySceneManager.Instance.CurrentBigSalonId,
                StringIdTeam = LobbySceneManager.Instance.CurrentTeamId
            },
            Text = text
        };
        WsClient.Instance.SendChatMessage(chatDTO);

        inputField.text = "";
        inputField.ActivateInputField();
    }

    public void AddMessage(int userId, string userName, string text, long ts)
    {
        var go = Instantiate(chatItemPrefab, content);
        var item = go.GetComponent<ChatMessageItem>();
        bool isSelf = (userId == Authentificator.Instance.Id);
        item.Setup(userName, text, ts, isSelf);

        // Force scroll to bottom on new message
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }

    public void AddMessageToChat(ChatDTO chatDTO)
    {
        AddMessage(chatDTO.FromId, chatDTO.FromName, chatDTO.Text, chatDTO.Ts);
    }

    public void SetHistory(IEnumerable<ChatDTO> history)
    {
        // clear existing
        for (int i = content.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(content.GetChild(i).gameObject);
#else
            GameObject.Destroy(content.GetChild(i).gameObject);
#endif
        }
        foreach (var m in history)
        {
            AddMessage(m.FromId, m.FromName, m.Text, m.Ts);
        }
    }

    [SerializeField]
    private TypeChatTarget chatTarget = TypeChatTarget.SALON;

    [SerializeField]
    private TextMeshProUGUI textButtonChat;

    [SerializeField]
    private int idTarget;

    [SerializeField] 
    private string userName;

    [SerializeField]
    private string idTeam;

    private int indexUser = 0;
    public void ChangeChatTarget()
    {
        // Start from SALON
        if (chatTarget == TypeChatTarget.SALON)
        {
            // Change to LOBBY if team exists
            if (!string.IsNullOrEmpty(LobbySceneManager.Instance.CurrentTeamId))
            {
                chatTarget = TypeChatTarget.LOBBY;
                idTeam = LobbySceneManager.Instance.CurrentTeamId;
                textButtonChat.text = "Equipe";
            }
        }
        else if (chatTarget == TypeChatTarget.LOBBY)
        {
            // Get administrators of current team
            var salon = LobbySceneManager.Instance.actualBigSalon
                .Salons
                .Find(x => x.Id == LobbySceneManager.Instance.CurrentTeamId);

            if (salon != null)
            {
                var usersAdmin = salon.UsersInSalon
                    .Where(x => x.Roles.Any(r => r.Equals("administrator", StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (usersAdmin.Count > 0)
                {
                    // Take the next admin if possible
                    var nextAdmin = usersAdmin.FirstOrDefault(u => u.Id != idTarget);

                    if (nextAdmin != null)
                    {
                        chatTarget = TypeChatTarget.ADMIN;
                        idTarget = nextAdmin.Id;
                        userName = nextAdmin.Name;
                        textButtonChat.text = userName;
                        return; // stop here
                    }
                }
            }

            // If no admins available go back to SALON
            chatTarget = TypeChatTarget.SALON;
            idTarget = 0;
            userName = null;
            textButtonChat.text = "Salon";
        }
        else if (chatTarget == TypeChatTarget.ADMIN)
        {
            // We were targeting a user admin find next admin
            var salon = LobbySceneManager.Instance.actualBigSalon
                .Salons
                .Find(x => x.Id == LobbySceneManager.Instance.CurrentTeamId);

            if (salon != null)
            {
                var usersAdmin = salon.UsersInSalon
                    .Where(x => x.Roles.Any(r => r.Equals("administrator", StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (usersAdmin.Count > 0)
                {
                    // Find current index in the list
                    int index = usersAdmin.FindIndex(u => u.Id == idTarget);

                    // Get the next one (wrap around if needed)
                    int nextIndex = (index + 1) % usersAdmin.Count;
                    var nextAdmin = usersAdmin[nextIndex];

                    // If next admin is the same as current  cycle is done back to SALON
                    if (nextAdmin.Id == idTarget)
                    {
                        chatTarget = TypeChatTarget.SALON;
                        idTarget = 0;
                        userName = null;
                        textButtonChat.text = "Salon";
                    }
                    else
                    {
                        chatTarget = TypeChatTarget.ADMIN;
                        idTarget = nextAdmin.Id;
                        userName = nextAdmin.Name;
                        textButtonChat.text = userName;
                    }

                    return;
                }
            }

            // If no admins available back to SALON
            chatTarget = TypeChatTarget.SALON;
            idTarget = 0;
            userName = null;
            textButtonChat.text = "Salon";
        }
    }
    [SerializeField]
    private CanvasGroup chatGameWindow;
    [SerializeField]
    private GameObject buttonChat;
    public void OpenCloseChat()
    {
        if(chatGameWindow.alpha > 0f)
        {
            HideChat();
        }
        else
        {
            ShowChat();
        }
    }

    public void ActivateChat()
    {
        buttonChat.SetActive(true);
        ShowChat();
    }

    public void ShowChat()
    {
        chatGameWindow.alpha = 1.0f;
        chatGameWindow.blocksRaycasts = true;
    }

    public void HideChat()
    {
        chatGameWindow.alpha = 0.0f;
        chatGameWindow.blocksRaycasts = false;
    }
    public void Desactivate()
    {
        buttonChat.SetActive(true);
        HideChat();
    }
}

