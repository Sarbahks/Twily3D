using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Purchasing.MiniJSON;



public class AuthManager : MonoBehaviour
{

    private static AuthManager instance;

    public static AuthManager Instance
    {
        get
        { if(instance == null)
            {
                AuthManager instance = FindAnyObjectByType<AuthManager>();
            }
        return instance;
        }
    }

    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public TextMeshProUGUI statusText;

    // Endpoints
    private string TOKEN_URL = "http://localhost/wordpress/wp-json/jwt-auth/v1/token";
    private string USER_URL = "http://localhost/wordpress/wp-json/wp/v2/users/me";

    void Start()
    {
        ConfigLoader.Instance.Load(); // sync
        TOKEN_URL = ConfigLoader.Instance.TokenUrl;
        USER_URL = ConfigLoader.Instance.ApiUrl;
 
    }
        // Optionally: auto-login if token exists   remove it fot test
       /* var savedToken = PlayerPrefs.GetString("jwt_token", "");
        if (!string.IsNullOrEmpty(savedToken))
            StartCoroutine(ValidateAndFetchUser(savedToken));*/
  

    public void OnLoginClicked()
    {
        string email = emailInput.text.Trim();
        string pass = passwordInput.text;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            statusText.text = "Please enter email and password.";
            return;
        }
        StartCoroutine(LoginCoroutine(email, pass));
    }


    public void OnLoginDebug(string email, string pass)
    {
    //    string email = emailInput.text.Trim();
     //   string pass = passwordInput.text;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            statusText.text = "Please enter email and password.";
            return;
        }
        StartCoroutine(LoginCoroutine(email, pass));
    }

    IEnumerator LoginCoroutine(string email, string password)
    {
        statusText.text = "Connexion…";

        // Prepare form
        WWWForm form = new WWWForm();
        form.AddField("username", email);
        form.AddField("password", password);

        using (UnityWebRequest www = UnityWebRequest.Post(TOKEN_URL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                statusText.text = "Error: " + www.error;
                yield break;
            }

            // Parse JSON
            AuthResponse auth = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);
            if (string.IsNullOrEmpty(auth.token))
            {
                statusText.text = "Login failed. Check credentials.";
                yield break;
            }

            // Store token
          //  PlayerPrefs.SetString("jwt_token", auth.token);

            Authentificator.Instance.JwtToken = auth.token;
            statusText.text = "Login successful! Fetching user info…";

            // Fetch user data
            yield return FetchUserInfo(auth.token);
        }
    }

    public void LauchAsClientTest(int slot)
    {
        Authentificator.Instance.SetAuthTestById(slot);
    }



    IEnumerator FetchUserInfo(string token)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(USER_URL))
        {
            www.SetRequestHeader("Authorization", "Bearer " + token);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                statusText.text = "Failed to get user info: " + www.error;
                yield break;
            }

            var wp = JsonUtility.FromJson<WPUserTemp>(www.downloadHandler.text);

            // 2) Map to your UserInfo (fallback to slug if name is empty)
            UserInfo user = new UserInfo
            {
                Id = wp.id,
                Name = string.IsNullOrEmpty(wp.name) ? wp.slug : wp.name,
                Roles = wp.roles ?? Array.Empty<string>()
            };

        


            Authentificator.Instance.Username = user.Name;
            Authentificator.Instance.Id = user.Id;
            Authentificator.Instance.Roles = string.Join(",", user.Roles);
            Authentificator.Instance.Roles += ", player";//for debug only


            Debug.Log("Loading LobbyScene...");
            SceneManager.LoadScene("LobbyScene");


        }
    }

    IEnumerator ValidateAndFetchUser(string token)
    {
        statusText.text = "Validating session…";
        yield return FetchUserInfo(token);
    }
}
