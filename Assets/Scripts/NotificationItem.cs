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
        salonNotif.text = notif.notificationInfo + " demande une intervention";

        switch (notif.typeNotification)
        {
            case TypeNotification.VALIDATION:
                descriNotif.text = "Il s'agit d'une validation de carte";
                break;
            case TypeNotification.PM:
                break;
            case TypeNotification.ASKJOIN:
                break;
            case TypeNotification.STUCK:
                break;
        }
    
    }

    public void GoToSalon()
    {
        Debug.Log("Go to salon : " + idSalon);
    }
}
