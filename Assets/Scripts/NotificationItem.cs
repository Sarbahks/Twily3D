using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class NotificationItem : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI salonNotif;

    [SerializeField]
    private TextMeshProUGUI descriNotif;

    private string idSalon;

    public void SetupNotifItem(string idSalon,string salonName,  NotificationTwily notif )
    {
        this.idSalon = idSalon;
        salonNotif.text = salonName;
        descriNotif.text = notif.notificationInfo;
    }

    public void GoToSalon()
    {
        Debug.Log("Go to salon : " + idSalon);
    }
}
