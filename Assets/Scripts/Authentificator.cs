using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;

public class Authentificator : MonoBehaviour
{
    private static Authentificator instance;
    public static Authentificator Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindAnyObjectByType<Authentificator>();
            }

            return instance;
        }
    }

    public string Username { get => username; set => username = value; }
    public string Roles { get => roles; set => roles = value; }
    public string[] RolesArray
    {
        get => string.IsNullOrWhiteSpace(Roles)
            ? Array.Empty<string>()
            : Roles.Split(',', StringSplitOptions.RemoveEmptyEntries);

        set => Roles = value == null ? string.Empty : string.Join(",", value);
    }
    public int Id { get => id; set => id = value; }
    public string JwtToken { get => jwtToken; set => jwtToken = value; }

    [SerializeField]
    private string username;

    [SerializeField]
    private string roles;

    [SerializeField]
    private int id;

    [SerializeField]
    private string jwtToken;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    [SerializeField]
    private AuthManager authManager;
    public void SetAuthTestById(int idAuth)
    {
        switch (idAuth)
        {
            case 1:
               // authManager.OnLoginDebug("d.gely@wanadoo.fr", "admin");

                Authentificator.Instance.Username = "David";
                Authentificator.Instance.Id = 0;
                Authentificator.Instance.Roles = "administrator, player";
          
                Debug.Log("Loading LobbyScene...");
                SceneManager.LoadScene("LobbyScene");

                break;
            case 2:
                //authManager.OnLoginDebug("nora@gmail.com", "123456");

                Authentificator.Instance.Username = "Nora";
                Authentificator.Instance.Id = 1;
                Authentificator.Instance.Roles = "administrator, player";
                Debug.Log("Loading LobbyScene...");
                SceneManager.LoadScene("LobbyScene");

                break;
            case 3:
              //  authManager.OnLoginDebug("nath@gmail.com", "123456");
                Authentificator.Instance.Username = "Nath";
                Authentificator.Instance.Id = 2;
                Authentificator.Instance.Roles = "administrator, player";
                Debug.Log("Loading LobbyScene...");
                SceneManager.LoadScene("LobbyScene");
                break;
            case 4:
                //  authManager.OnLoginDebug("steph@gmail.com", "123456");
                Authentificator.Instance.Username = "Steph";
                Authentificator.Instance.Id = 3;
                Authentificator.Instance.Roles = "administrator, player";
                Debug.Log("Loading LobbyScene...");
                SceneManager.LoadScene("LobbyScene");
                break;


        }
    }

    public UserInfo GetUser()
    {
        return new UserInfo
        {
            Id = id,
            Name = username,
            Roles = RolesArray
        };
    }


}
