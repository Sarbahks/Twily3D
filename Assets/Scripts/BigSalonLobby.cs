using UnityEngine;

public class BigSalonLobby : MonoBehaviour
{
    private static BigSalonLobby instance;

    public static BigSalonLobby Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindFirstObjectByType<BigSalonLobby>();
            }
            return instance;
        }

    }


    [SerializeField]
    private Transform connectedOnTheBigSalon;
     [SerializeField]
    private Transform activesGamesOnTheSalon;

    [SerializeField]
    private Transform salonAreas;


    [SerializeField]
    private GameObject prefabParticipantBigSalon;

    [SerializeField]
    private GameObject prefabActiveGameOnBigSalon;
    [SerializeField]
    private GameObject salonPrefab;

    public void SetupData(BigSalonInfo salonInfo)
    {
        Helpers.Instance.ClearContainer(activesGamesOnTheSalon);
        Helpers.Instance.ClearContainer(connectedOnTheBigSalon);


        foreach(var connected in salonInfo.UserInBig)
        {
            var co = Instantiate(prefabParticipantBigSalon, connectedOnTheBigSalon);
            var roster = co.GetComponent<RosterItem>();
            
            roster.Setup(connected);
        }

        foreach(var subsalon in salonInfo.Salons)
        {
            if(subsalon.GameState != null)
            {
                //part on ongoing game
                var gm = Instantiate(prefabActiveGameOnBigSalon, activesGamesOnTheSalon);
                var item = gm.GetComponent<GameInSalonItem>();

                item.Setup(subsalon);
            }

            var salonGo = Instantiate(salonPrefab, salonAreas);
            var salonScript = salonGo.GetComponent<SalonItem>();
            if(salonScript != null)
            {
                salonScript.SetupSalonItem(subsalon);
            }

        }
    }





}
