using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class InGameMenuManager : MonoBehaviour
{
    private static InGameMenuManager instance;

    public static InGameMenuManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance =FindAnyObjectByType<InGameMenuManager>();
            }

            return instance;    
        }
    }


    [SerializeField]
    private DashBoardGame dashBoard;

    public MenuType ActualMenu { get => actualMenu; set => actualMenu = value; }
    public List<TabUIClickable> Tabs { get => tabs; set => tabs = value; }
    public TextMeshProUGUI TextShared { get => textShared; set => textShared = value; }

    [SerializeField]
    private CanvasGroup menuUI;


    [SerializeField]
    private GameObject cardUIPrefab;

  
    [SerializeField]
    private MenuCardArea menuCardAreaPlayer;
        [SerializeField]
    private MenuCardArea menuCardAreaAdmin;
 
    


    [SerializeField]
    private MenuType actualMenu = MenuType.MENUCARDSPLAYER;


    [SerializeField]
    private List<TabUIClickable> tabs;

    [SerializeField]
    private CanvasGroup playercardCG;

    
    [SerializeField]
    private CanvasGroup admincardCG;

    
    [SerializeField]
    private CanvasGroup charactersCG;

    [SerializeField]
    private TextMeshProUGUI textShared;


    private void  OpenCanvasGroup(CanvasGroup cg)
    {
        cg.alpha = 1;
        cg.blocksRaycasts = true;
    }
    private void  CloseCanvasGroup(CanvasGroup cg)
    {
        cg.alpha = 0;
        cg.blocksRaycasts = false;
    }


    public void OpenPageMenu(MenuType type)
    {
        foreach(var t in tabs)
        {
            t.SetSelected(t.TabType == type);
        }

        actualMenu = type;

        switch (type)
        {
            case MenuType.MENUCARDSPLAYER:
                CloseCanvasGroup(admincardCG);
                CloseCanvasGroup(charactersCG);
                OpenCanvasGroup(playercardCG);
                break;
            case MenuType.MENUCHARACTERS:
                dashBoard.OpenDashBoard();
                CloseCanvasGroup(playercardCG);
                CloseCanvasGroup(admincardCG);
                OpenCanvasGroup(charactersCG);
                break;
            case MenuType.MENUCARDSADMIN:
                CloseCanvasGroup(charactersCG);
                CloseCanvasGroup(playercardCG);
                OpenCanvasGroup(admincardCG);
                break;
        }


    }

    public void ActualizeCardsUI(List<CardData> cards)
    {
        
            menuCardAreaPlayer.DeleteAllCardsInArea();
        

        // Create new UI elements for each card
        foreach (var newCard in cards)
        {
            if(newCard.TypeCard == TypeCard.PROFILE)//profile page
            {
                Debug.Log("card that are not questions");
          
            }
            else
            {
                
               var go = menuCardAreaPlayer.AddCardToArea(cardUIPrefab);//
          //  var go =   Instantiate(cardUIPrefab); // cleaner instantiation
                var bc = go.GetComponent<BoardCaseUI>();
                bc.Initialize(newCard);
      

            }

        }



        if (LobbySceneManager.Instance.IsAdmin()|| LobbySceneManager.Instance.IsObserver())
        {
               ActualizeCardsUIAdmin(cards);
          
        }
    }

    public void ActualizeCardsUIAdmin(List<CardData> cards)
    {
        // Destroy all existing card UI elements
        menuCardAreaAdmin.DeleteAllCardsInArea();

        // Create new UI elements for each card
        foreach (var newCard in cards)
        {
            if (newCard.TypeCard != TypeCard.PROFILE)
            {
                var go = menuCardAreaAdmin.AddCardToArea(cardUIPrefab); // cleaner instantiation
                var bc = go.GetComponent<BoardCaseUI>();
                bc.InitializeAdmin(newCard);
            }

        }
    }

    public void ShowMenu()
    {
        menuUI.alpha = 1;
        menuUI.blocksRaycasts = true;
   
    }

    public void HideMenu()
    {
        menuUI.alpha = 0;
        menuUI.blocksRaycasts = false;
    }

}


public enum MenuType
{
    MENUCARDSPLAYER,
    MENUCHARACTERS,
    MENUCARDSADMIN
}
