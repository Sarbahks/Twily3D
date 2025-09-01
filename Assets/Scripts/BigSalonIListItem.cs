using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BigSalonListItem : MonoBehaviour
{
    public TMP_Text NameText;

    private string _id;
    // private Action<string> _onJoin;
    private BigSalonInfo info;
    public void Setup(BigSalonInfo salonInfo)
    {
        info = salonInfo;
        NameText.text = salonInfo.Name ;
  /*      _onJoin = onJoin;
        JoinButton.OnClick.RemoveAllListeners();
        JoinButton.OnClick.AddListener(() => _onJoin?.Invoke(_id));*/
    }

    public void  JoinBigSalon()
    {
        LobbySceneManager.Instance.AskJoinBigSalonLobby(info.Id);
    }
}
