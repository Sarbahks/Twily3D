using System;
using UnityEngine;

public class NotificationCenter : MonoBehaviour
{
    private static NotificationCenter instance;

    public static NotificationCenter Instance
    {
        get
        {
            if(instance == null)
            {
               instance = FindFirstObjectByType<NotificationCenter>();
            }

            return instance;
        }
    }

    [SerializeField]
    private GameObject adminPannel;
        [SerializeField]
    private GameObject notificationButton;

    [SerializeField]
    private GameObject haveNotification;
    [SerializeField]
    private Transform notifArea;

    [SerializeField]
    private Transform resumeArea;

    [SerializeField]
    private GameObject resumePrefab;

    [SerializeField]
    private GameObject notifPrefab;
    public void EnableNotification()
    {
        notificationButton.SetActive(true);
    }

    public void DisableNotification()
    {
        notificationButton.SetActive(false);
    }

   
    public void SetupNotificationCenter(BigSalonInfo info)
    {

        if (LobbySceneManager.Instance.IsAdmin() || LobbySceneManager.Instance.IsObserver())
        {
            EnableNotification();
        }
        else
        {
            DisableNotification();
            return;
        }

        SetupNotification(info);
        SetupGames(info);
     
    }

    private void SetupNotification(BigSalonInfo info)
    {
        if (info == null || info.Salons == null)
            return;

        Helpers.Instance.ClearContainer(notifArea);

        try
        {
            if(info.Notifications == null || info.Notifications.Count <=0)
            {
                haveNotification.SetActive(false);
            }
            else
            {
                haveNotification.SetActive(true);
            }


            foreach (var notif in info.Notifications)
            {
                if (notif == null)
                    continue;

                    var go = Instantiate(notifPrefab, notifArea);
                    var notifScript = go.GetComponent<NotificationItem>();
                    notifScript.SetupNotifItem(notif.idTeamNotif, notif.idSalonNotif, notif);


               
            }



        }
        catch(Exception e) { Debug.LogException(e); };

    }

    private void SetupGames(BigSalonInfo info)
    {
        if (info == null || info.Salons == null)
            return;

        Helpers.Instance.ClearContainer(resumeArea);
        foreach (var salon in info.Salons)
        {
            if (salon.GameState == null )
                continue;

            
                var go = Instantiate(resumePrefab, resumeArea);
                var resume = go.GetComponent<ResumeGame>();
            resume.SetupResumeGame(salon.GameState, salon.Name);
            
        }

    }

    public void OpenNotifCenter()
    {
        WsClient.Instance.GetLobby(LobbySceneManager.Instance.CurrentBigSalonId);
        adminPannel.SetActive(true);
    }

    public void CloseNotifCenter()
    {
        adminPannel.SetActive(false);
    }
}
