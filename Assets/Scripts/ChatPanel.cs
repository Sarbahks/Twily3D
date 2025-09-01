using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatPanel : MonoBehaviour
{
    [Header("Wiring")]
    public RectTransform content;        // ScrollView/Viewport/Content
    public ScrollRect scrollRect;        // the ScrollRect
    public TMP_InputField inputField;
    public Button sendButton;
    public GameObject chatItemPrefab;    // ChatMessageItem prefab

    private string myUserId;

    private void Awake()
    {
        myUserId = PlayerPrefs.GetInt("user_id", -1).ToString();
        if (sendButton != null) sendButton.onClick.AddListener(OnClickSend);
        if (inputField != null) inputField.onSubmit.AddListener(_ => OnClickSend());
    }

    public void OnClickSend()
    {
        var text = inputField.text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        // Ask LobbySceneManager to send
        LobbySceneManager.Instance.SendChat(text);
        inputField.text = "";
        inputField.ActivateInputField();
    }

    public void AddMessage(string userId, string userName, string text, long ts)
    {
        var go = Instantiate(chatItemPrefab, content);
        var item = go.GetComponent<ChatMessageItem>();
        bool isSelf = (userId == myUserId);
        item.Setup(userName, text, ts, isSelf);

        // Force scroll to bottom on new message
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
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

    public void ChangeChatTarget()
    {

    }


}

