using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BigSalonListItem : MonoBehaviour
{
    public TMP_Text NameText;
    public TMP_Text DescriText;

    [SerializeField]
    private GameObject deleteButton;

    private string _id;
    // private Action<string> _onJoin;
    private BigSalonInfo info;
    public void Setup(BigSalonInfo salonInfo)
    {
        _id = salonInfo.Id;
        info = salonInfo;
        NameText.text = salonInfo.Name ;
        DescriText.text = salonInfo.Description ;
        if(!LobbySceneManager.Instance.IsAdmin() )
        {
            deleteButton.SetActive(false);
        }
        
        /*      _onJoin = onJoin;
        JoinButton.OnClick.RemoveAllListeners();
        JoinButton.OnClick.AddListener(() => _onJoin?.Invoke(_id));*/
    }

    public void  JoinBigSalon()
    {
        LobbySceneManager.Instance.AskJoinBigSalonLobby(info.Id);
    }

    public void DeleteBigSalon()
    {
        LobbySceneManager.Instance.DeleteBigSalon(info.Id);
    }
}
