using System.Collections.Generic;
using System.Text.Json.Serialization;
using System;
using UnityEngine;
using System.Linq;
using UnityEngine.Playables;

public class Helpers : MonoBehaviour
{
    private static Helpers instance;

    public static Helpers Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindFirstObjectByType<Helpers>();
            }

            return instance;
        }
    }


    public void ClearContainer(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            Destroy(t.GetChild(i).gameObject);
        }
    }


    // optional helper if your UI gives a CSV field of pseudos
    public  List<string> ParseCsvPseudos(string csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return new List<string>();
        return csv
            .Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public List<string> ConvertStringToArray(string stringText)
    {
        var array =  stringText.Split(new[] { '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries);
        return array.ToList();
    }
}


//UNITY ONLY

[Serializable]
public class WPUserTemp
{
    public int id;
    public string name;      // sometimes WP uses "name" and also has "slug"
    public string slug;
    public string[] roles;
}


//SERVER AND CLIENT

[Serializable]
public class BigSalonInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> WhiteList { get; set; }
    public List<SalonInfo> Salons { get; set; }
    public List<UserInfo> UserInBig { get; set; }   // who’s currently connected to THIS big salon
    public List<NotificationTwily> Notifications { get; set; } = new List<NotificationTwily>();
}

[Serializable]
public class SalonInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<string> WhiteList { get; set; }
    public List<UserInfo> UsersInSalon { get; set; } // who’s currently connected to THIS sub-salon
    public GameStateData GameState { get; set; }     // null if not started
}


[Serializable]
public class JoinSalonRequest
{
    public UserInfo UserInfo { get; set; }

    public string SalonId { get; set; }

}
[Serializable]
public class DeleteNotificationRequest
{
    public string IdSalon { get; set; }
    public string IdNotification { get; set; }
}

[Serializable]
public class JoinSalonResponse
{
    public bool Joined { get; set; }
    public string SalonId { get; set; }

}

[Serializable]
public class CreateTeamRequest
{
    public string IdSalon { get; set; }

    public SalonInfo SalonInfo { get; set; }

}

[Serializable]
public class LeaveSalonRequest
{
    public UserInfo UserInfo { get; set; }

    public string SalonId { get; set; }

}
[Serializable]
public class LeaveSalonResponse
{
    public bool Leaved { get; set; }
    public string SalonId { get; set; }
}
[Serializable]

public class JoinTeamRequest
{
    public string SalonId { get; set; }

    public string TeamId { get; set; }
    public bool IsPlayer { get; set; }

    public UserInfo UserInfo { get; set; }

}

[Serializable]
public class LeaveTeamRequest
{
    public string SalonId { get; set; }


    public string TeamId { get; set; }
    public bool IsPlayer { get; set; }

    public UserInfo UserInfo { get; set; }
}


[Serializable]
public class StartGameRequest
{

    public string SalonId { get; set; }


    public string TeamId { get; set; }

    public UserInfo UserInfo { get; set; }
}


[Serializable]
public class LeaveGameRequest
{

    public string SalonId { get; set; }


    public string TeamId { get; set; }

    public UserInfo UserInfo { get; set; }
}



[Serializable]
public class ServerUserData
{
    public UserInfo UserInfo { get; set; }
    public string Token { get; set; }
}


[Serializable]
public class UserInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string[] Roles { get; set; }
}


[Serializable]
public class AuthResponse
{
    public string token;
    public string user_email;
    public string user_nicename;
}

public class InitializeGamePayload
{
    public string SalonId { get; set; } = "";
    public string TeamId { get; set; } = "";
    public GameStateData Game { get; set; } = new GameStateData();
}

public class GameBoardInitializedPayload
{
    public string SalonId { get; set; } = "";
    public string TeamId { get; set; } = "";
    public GameStateData Game { get; set; } = new GameStateData();
}


[Serializable]
public class GameRulesData
{
    public int NumberAreas { get; set; }
    public List<AreaData> AreaDatas { get; set; }
}

public class AreaData
{
    [JsonPropertyName("areaId")]
    public int AreaId { get; set; }

    [JsonPropertyName("maxCaseQuestion")]
    public int MaxCaseQuestion { get; set; }

    [JsonPropertyName("maxCaseBonus")]
    public int MaxCaseBonus { get; set; }

    [JsonPropertyName("maxCaseProfile")]
    public int MaxCaseProfile { get; set; }

    [JsonPropertyName("maxCaseDefi")]
    public int MaxCaseDefi { get; set; }

    [JsonPropertyName("maxCaseKpi")]
    public int MaxCaseKpi { get; set; }

    [JsonPropertyName("maxCaseProfileManagement")]
    public int MaxCaseProfileManagement { get; set; }
}




[Serializable]
public class CardData
{
    public int Id { get; set; }
    public int IdArea { get; set; }
    public TypeCard TypeCard { get; set; }

    public string Title { get; set; }
    public string Instruction { get; set; }

    // Optional fields per type
    public string Question { get; set; }         // for QuestionCard
    public string Response { get; set; }         // for QuestionCard
    public bool Unlocked { get; set; }
    public bool NeedProEvaluation { get; set; }
    public EvaluationResult AutoEvaluationResult { get; set; }
    public EvaluationResult ProEvaluationResult { get; set; }

    public int Points { get; set; }             // for BonusCard

    public string Description { get; set; }      // for ProfileCard
    public string Degree { get; set; }
    public string StrongPoints { get; set; }
    public string WeakPoints { get; set; }

    public string Role { get; set; }
    public string Experience { get; set; }
    public string Seniority { get; set; }

    public string OldService { get; set; }

    public int SpecialCardEffect { get; set; } = 0;

    public int AttachedDocupentId { get; set; } = 0;

}

[Serializable]
public class ChoseRoleGameRequest
{

    public string SalonId { get; set; }


    public string TeamId { get; set; }

    public UserInfo UserInfo { get; set; }

    public RoleGameType RoleWanted { get; set; }
}


[Serializable]
public class GameStateData
{
   
    public bool ChooseCarrd { get; set; } = false;
    public int IdCardChossen { get; set; } = 0;
    public bool Active { get; set; }
    public bool Completed { get; set; }

    public int CurrentPosition { get; set; } // Position commune sur le plateau


    public List<PlayerData> Players { get; set; }
    public List<CardData> Board { get; set; }

    public GameRulesData GameRules { get; set; }
    public int CurrentPlayerId { get; set; } // explicitly indicates whose turn it is

    public string SharedMessage { get; set; }

    public int CurrentArea { get; set; } =  0;
    public List<AreaStateData> AreaStates { get; set; } = new List<AreaStateData>();

    //new things to add
    public int TotalScore { get; set; }

    public DateTime StartGame { get; set; }
    public DateTime TimeLastTurn { get; set; }
    public DateTime EndedTime { get; set; }

    public StepGameType Step { get; set; }
    public SpecialCardBudgetResponse SpecialCardBudgetResponse { get; set; } = new SpecialCardBudgetResponse();
    public SpecialCardCrisisResponse SpecialCardCrisisResponse { get; set; } = new SpecialCardCrisisResponse();

    public bool FirstCardsProfileChosen { get; set; } = false;
    public bool SecondCardsProfileChosen { get; set; } = false;
}

[Serializable]
public class SpecialCardCrisisResponse
{
    public string FirstCause { get; set; }
    public string SecondCause { get; set; }
    public string ThirdCause { get; set; }
    public string FourthCause { get; set; }
    public string FifthCause { get; set; }
}

[Serializable]
public class SpecialCardBudgetResponse
{
    public List<SpecialCardBudgetData> SpecialCardBudgetDatas { get; set; } = new List<SpecialCardBudgetData>();
}

[Serializable]
public class SpecialCardBudgetData
{
    public RoleGameType Role { get; set; }
    public BudgetType Budget { get; set; }
    public int BudgetValue { get; set; }
}
[JsonConverter(typeof(JsonStringEnumConverter))]
[Serializable]
public enum BudgetType
{
    SALARYRESPO,
    SALARYMEMBERS,
    FORMATION,
    OUTILTECH,
    FRAISOP,
    TOTAL
}


[Serializable]
public class NotificationTwily
{
    public string idNotification { get; set; }
    public string notificationInfo { get; set; }
    public DateTime notificationTime { get; set; }
    public TypeNotification typeNotification { get; set; }
    public string idSalonNotif { get; set; }
    public string idTeamNotif { get; set; }

    public int idUserNotif { get; set; }
}
[JsonConverter(typeof(JsonStringEnumConverter))]
[SerializeField]
public enum TypeNotification
{
    VALIDATION,
    PM,
    ASKJOIN,
    STUCK
}

[Serializable]
public class AreaStateData
{
    public int idArea { get; set; }
    public List<CaseStateData> casesOnBoard { get; set; }
}

[Serializable]
public class CaseStateData
{
    public bool isVisited { get; set; }
    public int idCardOn { get; set; }
}


[Serializable]
public class PlayerData
{

    public UserInfo userInfo { get; set; }
    public int score { get; set; }
    public RoleGameType roleGame { get; set; }

    public List<CardData> cardsProfile { get; set; } = new List<CardData>();


    public TypeManagementResponsableQualiteEtProcessus TypeManagementQual { get; set; } = TypeManagementResponsableQualiteEtProcessus.AUCUN;
    public TypeManagementResponsableFormationEtSupportInterne TypeManagementFroma { get; set; } = TypeManagementResponsableFormationEtSupportInterne.AUCUN;
    public TypeManagementResponsableRelationClientele TypeManagementClient { get; set; } = TypeManagementResponsableRelationClientele.AUCUN;
    public TypeManagementResponsableAnalystesDeDonnees TypeManagementData { get; set; } = TypeManagementResponsableAnalystesDeDonnees.AUCUN;
}
[JsonConverter(typeof(JsonStringEnumConverter))]
[Serializable]
public enum TypeManagementResponsableQualiteEtProcessus
{
    AUCUN,
    DIRECTIF,
    PARTICIPATIF,
    PAROBJECTIFS,
    DELEGATIF,
    TRANSFORMATIONNEL,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
[Serializable]
public enum TypeManagementResponsableFormationEtSupportInterne
{
    AUCUN,
    COACH,
    INSPIRANT,
    COLLABORATIF,
    AXESURLACOMMUNICATION,
    PAROBJECTIFS
}

[JsonConverter(typeof(JsonStringEnumConverter))]
[Serializable]
public enum TypeManagementResponsableRelationClientele
{
    AUCUN,
    COLLABORATIF,
    EMOTIONNEL,
    COACH,
    TRANSACTIONNEL,
    DEPROXIMITE
}

[JsonConverter(typeof(JsonStringEnumConverter))]
[Serializable]
public enum TypeManagementResponsableAnalystesDeDonnees
{
    AUCUN,
    AXESURLESRESULTATS,
    STRUCTURE,
    PARTICIPATIF,
    TRANSFORMATEUR,
    PARPROJET
}
[JsonConverter(typeof(JsonStringEnumConverter))]
[Serializable]
public enum RoleGameType
{
    NOROLE,
    RESPOQUAL,
    RESPOCLI,
    RESPODATA,
    RESPOFORM
    
}
[JsonConverter(typeof(JsonStringEnumConverter))]
[Serializable]
public enum StepGameType
{
    NOTSTARTED,
    STARTED,
    CHOSEROLE,
    ROLECHOSEN,
    PLAYCARD,
    SELECTTEAM,
    NEXTPARTCARD,
    NEXTSELECTTEAM,
    FINALCARD

}

[JsonConverter(typeof(JsonStringEnumConverter))] // System.Text.Json
public enum TypeCard
{
    QUESTION,
    BONUS,
    PROFILE,
    DEFI,
    KPI,
    PROFILMANAGEMENT,
    BLOCAGE
}

[JsonConverter(typeof(JsonStringEnumConverter))] // System.Text.Json
public enum EvaluationResult
{
    NONE,
    WAITING,
    BAD,
    MID,
    GOOD
}


// Simple DTO that matches server messages
[Serializable]
public class ChatDTO
{
    public int FromId {  get; set; } 
    public string FromName { get; set; }
    public string Text { get; set; }
    public long Ts { get; set; }
    public ChatTarget Target { get; set; }


}
[JsonConverter(typeof(JsonStringEnumConverter))]
[Serializable]
public class ChatTarget
{
    public TypeChatTarget TypeChatTarget { get; set; }
    public int IdTarget { get; set; }
    public string StringIdTeam { get; set; }
    public string StringIdSalon { get; set; }
}
[JsonConverter(typeof(JsonStringEnumConverter))]
[Serializable]
public enum TypeChatTarget
{
    SALON,
    LOBBY,
    ADMIN,
    OBSERVER,
    PLAYER
}


[Serializable]
public class ChoseCardRequest
{
    public string IdSalon { get; set; }
    public string IdTeam { get; set; }
    public int IdCard { get; set; }
}

[Serializable]
public class ChoseCardResponse
{
    public string IdSalon { get; set; }
    public string IdTeam { get; set; }
    public int IdCard { get; set; }
    public GameStateData Game { get; set; }
}

[Serializable]
public class SendNotificationRequest
{
    public string IdSalon { get; set; }
    public NotificationTwily Notification { get; set; }
}

[Serializable]
public class AnswerCardRequest
{
    public string IdSalon { get; set; }
    public string IdTeam { get; set; }
    public CardData CardAnsewerd { get; set; }
    public int IdPlayerAnswered { get; set; }
}



[Serializable]
public class AnswerCardResponse
{
    public string IdSalon { get; set; }
    public string IdTeam { get; set; }
    public GameStateData Game { get; set; }

}


[Serializable]
public class ChoseProfileRequest
{
    public string IdSalon { get; set; }
    public string IdTeam { get; set; }

    public List<CardData> CardsChosen { get; set; } = new List<CardData>();

    public UserInfo UserInfo { get; set; }
}

[Serializable]
public class ChoseProfileResponse
{
    public string IdSalon { get; set; }
    public string IdTeam { get; set; }
    public GameStateData Game { get; set; }
}

public class ValidateCardAdminRequest
{
    public string IdSalon { get; set; }
    public string IdTeam { get; set; }
    public int CardId { get; set; }
    public EvaluationResult NewState { get; set; }
}

[Serializable]
public class ValidateCardAdminResponse
{
    public string IdSalon { get; set; }
    public string IdTeam { get; set; }
    public GameStateData Game { get; set; }
}

// DTOs shared with server
public class SubmitBudgetRequest
{
    public string IdSalon { get; set; }
    public string IdTeam { get; set; }
    public SpecialCardBudgetResponse Budget { get; set; }
    public int UserId { get; set; } // optional
}

public class SubmitCrisisRequest
{
    public string IdSalon { get; set; }
    public string IdTeam { get; set; }
    public SpecialCardCrisisResponse Crisis { get; set; }
    public int UserId { get; set; } // optional
}

[Serializable]
public class BudgetSubmittedResponse
{
    public string IdSalon { get; set; }
    public string IdTeam { get; set; }
    public GameStateData Game { get; set; }
}

[Serializable]
public class CrisisSubmittedResponse
{
    public string IdSalon { get; set; }
    public string IdTeam { get; set; }
    public GameStateData Game { get; set; }
}


[Serializable]
public class SharedMessageRequest
{
    public string IdSalon { get; set; }
    public string IdTeam { get; set; }

    public string TextShared { get; set; }

}

[Serializable]
public class SharedMessageResponse
{
    public string IdSalon { get; set; }
    public string IdTeam { get; set; }

    public string TextShared { get; set; }
}
[Serializable]
public class DeleteTeamRequest
{
    public string IdSalon { get; set; } = ""; // big salon id
    public string IdTeam { get; set; } = ""; // team/sub-salon id
}
[Serializable]
public class ConnextionInfo
{
    public string serverWebAPIAdressDebug { get; set; }
    public string serverWebTokenDebug { get; set; }
    public string serverGameAdressDebug { get; set; }

    public string serverWebAPIAdress { get; set; }
    public string serverWebToken { get; set; }
    public string serverGameAdress { get; set; }

}