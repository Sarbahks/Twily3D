using TMPro;
using UnityEngine;

public class ChatMessageItem : MonoBehaviour
{
    public TMP_Text userText;
    public TMP_Text messageText;
    public TMP_Text timeText;

    public void Setup(string user, string msg, long ts, bool isSelf)
    {
        userText.text = isSelf ? "You" : user;
        messageText.text = msg;
        timeText.text = FormatTime(ts);

        // Optional styling tweak
        if (isSelf) userText.color = new Color(0.2f, 0.8f, 0.2f);
    }

    private string FormatTime(long tsMs)
    {
        // ts is Unix ms
        System.DateTime epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        var dt = epoch.AddMilliseconds(tsMs).ToLocalTime();
        return dt.ToString("HH:mm");
    }
}
