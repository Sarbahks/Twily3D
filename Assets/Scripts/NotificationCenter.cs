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
        Helpers.Instance.ClearContainer(notifArea);


        foreach (var salon in info.Salons)
        {
            if (salon.GameState == null || salon.GameState.Notifications == null)
                continue;

            foreach (var notif in salon.GameState.Notifications)
            {
                var go = Instantiate(notifPrefab, notifArea);
                var notifScript = go.GetComponent<NotificationItem>();
                notifScript.SetupNotifItem(salon.Id, salon.Name, notif);
            }
        }
    }

    private void SetupGames(BigSalonInfo info)
    {
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
        adminPannel.SetActive(true);
    }

    public void CloseNotifCenter()
    {
        adminPannel.SetActive(false);
    }
}
