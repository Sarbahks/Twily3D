using TMPro;
using UnityEngine;

public class RosterItem : MonoBehaviour
{
    public TMP_Text UserText;
    public void Setup(UserInfo u)
    {
        UserText.text = $"{u.Name} (id:{u.Id})";
    }
}
