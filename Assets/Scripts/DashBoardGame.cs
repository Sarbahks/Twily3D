using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DashBoardGame : MonoBehaviour
{

    [SerializeField]
    private CanvasGroup _canvasGroup;   

    [SerializeField] private List<GameObject> cardEmplacementsPlayerRespoform;
    [SerializeField] private List<GameObject> cardEmplacementsPlayerRrespocli;
    [SerializeField] private List<GameObject> cardEmplacementsPlayerRespoqual;
    [SerializeField] private List<GameObject> cardEmplacementsPlayerRespoData;

    [SerializeField] private GameObject cardPrefab;

    private Dictionary<RoleGameType, List<GameObject>> playerEmplacements = new Dictionary<RoleGameType, List<GameObject>>();


    [SerializeField]
    private TwilyButton buttonValidateCrisis;

    [SerializeField]
    private TwilyButton buttonValidateBudget;

    [SerializeField]
    private List<CaseArray> casesTab;

    [SerializeField]
    private TMP_InputField inputFirstCause;
    [SerializeField]
    private TMP_InputField inputSecondCause;
    [SerializeField]
    private TMP_InputField inputThridCause;
    [SerializeField]
    private TMP_InputField inputFourthCause;
    [SerializeField]
    private TMP_InputField inputFifthCause;

    public void OpenDashBoard()
    {

        //actualize dashboard based on the gamedata
        var gameState = LobbySceneManager.Instance.CurrentGameState;
        SetBudgetValueFromServer(gameState.SpecialCardBudgetResponse);
        SetCrisisValueFromServer(gameState.SpecialCardCrisisResponse);
        SetupChoiceManagement(gameState);
        SetupCrisisButton(gameState);
        SetupBudgetButton(gameState);
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = 1.0f;


    }


    private void SetupCrisisButton(GameStateData gameStateData)
    {
        if (LobbySceneManager.Instance.IsPlayerInGame())
        {
            buttonValidateCrisis.gameObject.SetActive(false);
            return;
        }


        if (gameStateData.SpecialCardCrisisResponse.FirstCause != null && gameStateData.SpecialCardCrisisResponse.FirstCause != string.Empty &&
            gameStateData.SpecialCardCrisisResponse.SecondCause != null && gameStateData.SpecialCardCrisisResponse.SecondCause != string.Empty &&
            gameStateData.SpecialCardCrisisResponse.ThirdCause != null && gameStateData.SpecialCardCrisisResponse.ThirdCause != string.Empty &&
            gameStateData.SpecialCardCrisisResponse.FourthCause != null && gameStateData.SpecialCardCrisisResponse.FourthCause != string.Empty &&
            gameStateData.SpecialCardCrisisResponse.FifthCause != null && gameStateData.SpecialCardCrisisResponse.FifthCause != string.Empty )
        {
            buttonValidateCrisis.gameObject.SetActive(false);
        }
    }
    private void SetupBudgetButton(GameStateData gameStateData)
    {

        if (LobbySceneManager.Instance.IsPlayerInGame())
        {
            buttonValidateBudget.gameObject.SetActive(false);
            return;
        }

        if (gameStateData.SpecialCardBudgetResponse != null && gameStateData.SpecialCardBudgetResponse.SpecialCardBudgetDatas != null && gameStateData.SpecialCardBudgetResponse.SpecialCardBudgetDatas.Count > 0)
        {
            bool canBeEdited = false;

            foreach (var budget in gameStateData.SpecialCardBudgetResponse.SpecialCardBudgetDatas)
            {
                if(budget.BudgetValue < 0)
                {
                    canBeEdited = true;
                }
                buttonValidateBudget.gameObject.SetActive(canBeEdited);
                return;
            }
        }

        buttonValidateBudget.gameObject.SetActive(true);
    }


    public void CloseDashBoard()
    {
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = 0.0f;  
    }


    private void Awake()
    {
        // Map player index to their emplacement list
        playerEmplacements[RoleGameType.RESPOFORM] = cardEmplacementsPlayerRespoform;
        playerEmplacements[RoleGameType.RESPOCLI] = cardEmplacementsPlayerRrespocli;
        playerEmplacements[RoleGameType.RESPOQUAL] = cardEmplacementsPlayerRespoqual;
        playerEmplacements[RoleGameType.RESPODATA] = cardEmplacementsPlayerRespoData;
    }

    public void PlaceCardAndataOnTheEmplacement(GameStateData data)
    {
     
        foreach (var player in data.Players)
        {
            if (playerEmplacements.TryGetValue(player.roleGame, out var emplacements))
            {
                for (int i = 0; i < player.cardsProfile.Count && i < emplacements.Count; i++)
                {
                    // Spawn card prefab as a child of the emplacement
                    var go = Instantiate(cardPrefab, emplacements[i].transform.position, Quaternion.identity, emplacements[i].transform);
                    var script = go.GetComponent<CardProfile>();
                    if(script != null)
                    {
                        script.SetupCard(player.cardsProfile[i]);
                    }
                }
            }
  
        }
    }

    public void RemoveCardAndDataOnTheEmplacement()
    {
        // Destroy all cards from all emplacements
        foreach (var emplacementList in playerEmplacements.Values)
        {
            foreach (var emplacement in emplacementList)
            {
                foreach (Transform child in emplacement.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    [Header("Dropdown References")]
    [SerializeField] private TMP_Dropdown dropdownQualite;
    [SerializeField] private TMP_Dropdown dropdownFormation;
    [SerializeField] private TMP_Dropdown dropdownRelation;
    [SerializeField] private TMP_Dropdown dropdownAnalystes;

    [SerializeField] private TwilyButton buttonQualite;
    [SerializeField] private TwilyButton buttonFormation;
    [SerializeField] private TwilyButton buttonRelation;
    [SerializeField] private TwilyButton buttonAnalystes;

    public void ValidateManagerTypeQualite()
    {
        var type = (TypeManagementResponsableQualiteEtProcessus)dropdownQualite.value;

        if (type != TypeManagementResponsableQualiteEtProcessus.AUCUN)
        {
            Debug.Log($"Qualité sélectionné : {type}");
            buttonQualite.gameObject.SetActive(false);
        }
    }

    public void ValidateManagerTypeFormation()
    {
        var type = (TypeManagementResponsableFormationEtSupportInterne)dropdownFormation.value;

        if (type != TypeManagementResponsableFormationEtSupportInterne.AUCUN)
        {
            Debug.Log($"Formation sélectionné : {type}");
            buttonFormation.gameObject.SetActive(false);
        }
    }

    public void ValidateManagerTypeRelation()
    {
        var type = (TypeManagementResponsableRelationClientele)dropdownRelation.value;

        if (type != TypeManagementResponsableRelationClientele.AUCUN)
        {
            Debug.Log($"Relation sélectionné : {type}");
            buttonRelation.gameObject.SetActive(false);
        }
    }

    public void ValidateManagerTypeAnalysis()
    {
        var type = (TypeManagementResponsableAnalystesDeDonnees)dropdownAnalystes.value;

        if (type != TypeManagementResponsableAnalystesDeDonnees.AUCUN)
        {
            Debug.Log($"Analyste sélectionné : {type}");
            buttonAnalystes.gameObject.SetActive(false);
        }
    }



    private void Start()
    {
        // Setup each dropdown with enum values
        SetupDropdown<TypeManagementResponsableQualiteEtProcessus>(dropdownQualite, "Qualité & Processus");
        SetupDropdown<TypeManagementResponsableFormationEtSupportInterne>(dropdownFormation, "Formation & Support");
        SetupDropdown<TypeManagementResponsableRelationClientele>(dropdownRelation, "Relation Clientèle");
        SetupDropdown<TypeManagementResponsableAnalystesDeDonnees>(dropdownAnalystes, "Analystes de Données");
    }

    private void SetupDropdown<T>(TMP_Dropdown dropdown, string category) where T : Enum
    {
        dropdown.ClearOptions();
        var values = Enum.GetValues(typeof(T)).Cast<T>().ToList();
        var names = values.Select(v => v.ToString()).ToList();

        dropdown.AddOptions(names);

        // Add listener for selection
        dropdown.onValueChanged.AddListener(index =>
        {
            var selected = values[index];
            if (!selected.ToString().Equals("AUCUN", StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"[{category}] sélectionné : {selected}");
            }
        });
    }
    private void SetupChoiceManagement(GameStateData gameStateData)
    {
        // Setup all dropdowns normally
        SetupDropdown<TypeManagementResponsableQualiteEtProcessus>(dropdownQualite, "Qualité & Processus");
        SetupDropdown<TypeManagementResponsableFormationEtSupportInterne>(dropdownFormation, "Formation & Support");
        SetupDropdown<TypeManagementResponsableRelationClientele>(dropdownRelation, "Relation Clientèle");
        SetupDropdown<TypeManagementResponsableAnalystesDeDonnees>(dropdownAnalystes, "Analystes de Données");

        // Example: load saved value for player 1

        foreach(var p in gameStateData.Players)
        {
           
               
            //quality
            var quali = p.TypeManagementQual;

            if(quali == TypeManagementResponsableQualiteEtProcessus.AUCUN)
            {
                if(p.roleGame == RoleGameType.RESPOQUAL)
                {
                    buttonQualite.gameObject.SetActive(true);
                }
                else
                {
                    buttonQualite.gameObject.SetActive(false);
                }
            }
            else
            {
                int index = dropdownQualite.options.FindIndex(o => o.text == quali.ToString());

                if (index >= 0)
                {
                    dropdownQualite.value = index;
                    dropdownQualite.RefreshShownValue();
                }
            }

            //formation
            var formation = p.TypeManagementFroma;
            if (formation == TypeManagementResponsableFormationEtSupportInterne.AUCUN)
            {
                if (p.roleGame == RoleGameType.RESPOFORM)
                {
                    buttonFormation.gameObject.SetActive(true);
                }
                else
                {
                    buttonFormation.gameObject.SetActive(false);
                }
            }
            else
            {
                int index = dropdownFormation.options.FindIndex(o => o.text == formation.ToString());

                if (index >= 0)
                {
                    dropdownFormation.value = index;
                    dropdownFormation.RefreshShownValue();
                }
            }
            //client
            var client = p.TypeManagementClient;
            if (client == TypeManagementResponsableRelationClientele.AUCUN)
            {
                if (p.roleGame == RoleGameType.RESPOCLI)
                {
                    buttonRelation.gameObject.SetActive(true);
                }
                else
                {
                    buttonRelation.gameObject.SetActive(false);
                }
            }
            else
            {
                int index = dropdownRelation.options.FindIndex(o => o.text == formation.ToString());

                if (index >= 0)
                {
                    dropdownRelation.value = index;
                    dropdownRelation.RefreshShownValue();
                }
            }
            //data
            var data = p.TypeManagementData;
            if (data == TypeManagementResponsableAnalystesDeDonnees.AUCUN)
            {
                if (p.roleGame == RoleGameType.RESPOFORM)
                {
                    buttonAnalystes.gameObject.SetActive(true);
                }
                else
                {
                    buttonAnalystes.gameObject.SetActive(false);
                }
            }
            else
            {
                int index = dropdownAnalystes.options.FindIndex(o => o.text == formation.ToString());

                if (index >= 0)
                {
                    dropdownAnalystes.value = index;
                    dropdownAnalystes.RefreshShownValue();
                }
            }

        }
  
    }

    public SpecialCardBudgetResponse GetBudgetValueFromBoard()
    {
        SpecialCardBudgetResponse specialCardCrisisResponse = new SpecialCardBudgetResponse();
        specialCardCrisisResponse.SpecialCardBudgetDatas = new List<SpecialCardBudgetData>();
        foreach (var cases in casesTab)
        {
            specialCardCrisisResponse.SpecialCardBudgetDatas.Add(new SpecialCardBudgetData
            {
                Role = cases.Role,
                Budget = cases.Budget,
                BudgetValue = cases.GetValue()
            });
        }

        return specialCardCrisisResponse;
    }


    public void SetBudgetValueFromServer(SpecialCardBudgetResponse response)
    {
        foreach (var cases in casesTab)
        {
           foreach(var responseCase in response.SpecialCardBudgetDatas)
            {
                if(cases.Role == responseCase.Role)
                {
                    cases.Budget = responseCase.Budget;
                    cases.BudgetValue = responseCase.BudgetValue;
                    cases.SetupCase(); 

                }
            }
        }
    }


    public SpecialCardCrisisResponse GetCrisisValue()
    {
        SpecialCardCrisisResponse causes = new SpecialCardCrisisResponse();

        causes.FirstCause = inputFirstCause.text;
        causes.SecondCause = inputSecondCause.text;
        causes.ThirdCause = inputThridCause.text;
        causes.FourthCause = inputFourthCause.text;
        causes.FifthCause = inputFifthCause.text;

        return causes;


    }

    public void SetCrisisValueFromServer(SpecialCardCrisisResponse causes)
    {
        inputFirstCause.text = causes.FirstCause;
        inputSecondCause.text = causes.SecondCause;
        inputThridCause.text = causes.ThirdCause;
        inputFourthCause.text = causes.FourthCause;
        inputFifthCause.text = causes.FifthCause;
    }
}
