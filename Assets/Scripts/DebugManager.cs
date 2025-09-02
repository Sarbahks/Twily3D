using System;
using System.Data;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    [SerializeField]
    private bool sendNotif;

    // Update is called once per frame
    void Update()
    {
        if(sendNotif)
        {
            sendNotif = false;
            //test notification
            var newnotif = new NotificationTwily
            {
                idNotification = new GUI().ToString(),
                notificationInfo = "test notification",
                typeNotification = TypeNotification.VALIDATION,
                idSalonNotif = LobbySceneManager.Instance.CurrentBigSalonId,
                idTeamNotif = LobbySceneManager.Instance.CurrentTeamId != null ? LobbySceneManager.Instance.CurrentTeamId : string.Empty,
                idUserNotif = Authentificator.Instance.Id,
                notificationTime = DateTime.Now
            };
            WsClient.Instance.SendNotification(LobbySceneManager.Instance.CurrentBigSalonId, newnotif);
        }
    }
}
