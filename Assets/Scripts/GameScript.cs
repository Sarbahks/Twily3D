using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameScript : MonoBehaviour
    {

    private static GameScript instance;

    public static GameScript Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindAnyObjectByType<GameScript>();
            }

            return instance;
        }
    }

    [SerializeField]
    private List<ChoiceManager> managerProfile;

    [SerializeField]
    private GameObject presentationScreen;

    [SerializeField]
    private GameObject choiceProfileManager;

    [SerializeField]
    private List<GameObject> presentationPages;
    [SerializeField]
    private int actualPage = 0; // start at 0 for index safety

    [SerializeField]
    private CompanyInfoUI infoUI;

    public void NextPagePresentation()
    {
        if (presentationPages.Count == 0) return;

        // move forward but clamp to last page
        actualPage = Mathf.Min(actualPage + 1, presentationPages.Count - 1);

        UpdatePages();
    }

    public void PreviousPagePresentation()
    {
        if (presentationPages.Count == 0) return;

        // move backward but clamp to first page
        actualPage = Mathf.Max(actualPage - 1, 0);

        UpdatePages();
    }

    private void UpdatePages()
    {
        for (int i = 0; i < presentationPages.Count; i++)
        {
            presentationPages[i].SetActive(i == actualPage);
        }
    }


    public void StartTheGame()
    {
        if(!(LobbySceneManager.Instance.CurrentGameState.CurrentArea > 0))
        {
            OpenPresentation();
        }
    }


    public void OpenPresentation()
    {
        presentationScreen.SetActive(true);
    }

    public void ClosePresentation()
    {
        presentationScreen.SetActive(false);

        if(LobbySceneManager.Instance.IsPlayerInGame())
        {
        choiceProfileManager.SetActive(true);

        }
    }

    public void ProfileAllChosen()
    {
        choiceProfileManager.SetActive(false);
        Debug.Log("profile chosen"); 

        //set up the chose card ai depending on the player
        
    }

    public void OpenInfosLensUI()
    {
        infoUI.gameObject.SetActive(true);
    }

    public void CloseInfoLensUI()
    {
        infoUI.gameObject.SetActive(false);
    }

    public void SetProfileChoice(GameStateData payload)
    {
        foreach(var player in payload.Players)
        {
            if(player.userInfo != null)
            {
                foreach (var choice in managerProfile)
                {
                    if(choice.Role == player.roleGame)
                    {
                        choice.AssignUserToManager(player.userInfo);
                    }
                }
            }
        }

    }
}



