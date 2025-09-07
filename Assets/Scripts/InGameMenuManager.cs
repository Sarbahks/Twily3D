using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public DashBoardGame DashBoard { get => dashBoard; set => dashBoard = value; }

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
    private CanvasGroup documentsCG;

    [SerializeField]
    private TextMeshProUGUI textShared;

    [SerializeField]
    private DocumentViewer documentViewer;
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
        textShared.text = LobbySceneManager.Instance.CurrentGameState.SharedMessage;

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
                CloseCanvasGroup (documentsCG);
                OpenCanvasGroup(playercardCG);
                break;
            case MenuType.MENUCHARACTERS:
                DashBoard.OpenDashBoard();
                CloseCanvasGroup(playercardCG);
                CloseCanvasGroup(admincardCG);
                CloseCanvasGroup(documentsCG);
                OpenCanvasGroup(charactersCG);
                break;
            case MenuType.MENUCARDSADMIN:
                CloseCanvasGroup(charactersCG);
                CloseCanvasGroup(playercardCG);
                CloseCanvasGroup(documentsCG);
                OpenCanvasGroup(admincardCG);
                break;
                case MenuType.MENUCARDDOCS:
                CloseCanvasGroup(charactersCG);
                CloseCanvasGroup(playercardCG);
                CloseCanvasGroup(admincardCG);
                OpenCanvasGroup(documentsCG);
                break;
        }


    }

    public void HighlightCard(CardData card)
    {
       var cards =  menuCardAreaPlayer.GetAllCardsInArea();

        foreach(var carui in cards)
        {
            if(carui.BoardCard != null && carui.BoardCard.Id == card.Id)
            {
                carui.HilightCard();
            }
            else
            {
                carui.DelightCard();
            }
        }
    }

    public void ActualizeCardsUI(List<CardData> cards)
    {
        
            menuCardAreaPlayer.DeleteAllCardsInArea();


                cards = cards
            .OrderBy(c => c.IdArea == 0)   // false (non-0) before true (0)
            .ThenBy(c => c.IdArea)         // sort by areaId normally
            .ToList();


        // Create new UI elements for each card
        foreach (var newCard in cards)
        {
            if(newCard.TypeCard != TypeCard.PROFILE)//profile page
            { 
               var go = menuCardAreaPlayer.AddCardToArea(cardUIPrefab);//
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
        cards = cards
        .OrderBy(c => c.IdArea == 0)   // false (non-0) before true (0)
        .ThenBy(c => c.IdArea)         // sort by areaId normally
        .ToList();

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
    MENUCARDSADMIN,
    MENUCARDDOCS
}
