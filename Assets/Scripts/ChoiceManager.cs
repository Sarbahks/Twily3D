using TMPro;
using UnityEngine;

public class ChoiceManager : MonoBehaviour
{
    [SerializeField]
    private RoleGameType role;

    [SerializeField]
    private TextMeshProUGUI nameManager;

    [SerializeField]
    private UserInfo userAsManager;


public RoleGameType Role { get => role; set => role = value; }
    public UserInfo UserAsManager { get => userAsManager; set => userAsManager = value; }
    public bool AssignUserToManager(UserInfo userToAssign)
    {
        if (userToAssign == null)
            return false;

        userAsManager = userToAssign;
        nameManager.text = userToAssign.Name;

        return true;
    }
    public void ChoseRole()
    {
        //ask server give this role
        WsClient.Instance.SelectRole(LobbySceneManager.Instance.currentBigSalon.Id, LobbySceneManager.Instance.CurrentTeamId, Role);
    }
}
