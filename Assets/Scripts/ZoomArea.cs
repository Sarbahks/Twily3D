using TMPro;
using UnityEngine;

public class ZoomArea : MonoBehaviour
{

    [SerializeField]
    private GameObject sharedMenu;



    [SerializeField]
    private TMP_InputField input;

    public void OpenSharedEditionInMenu()
    {
        sharedMenu.SetActive(true);
        input.text = LobbySceneManager.Instance.CurrentGameState.SharedMessage;
    }



    public void ValidateSharedModification()
    {
        var newText = input.text;

        LobbySceneManager.Instance.OnSendSharedMessage(newText);
        sharedMenu.SetActive(false);
    }
}
