using Newtonsoft.Json;
using System.IO;
using UnityEngine;

public class ConfigLoader : MonoBehaviour
{
    private static ConfigLoader instance;
    public static ConfigLoader Instance
    {
        get
        {
            if (instance == null) instance = FindAnyObjectByType<ConfigLoader>();
            return instance;
        }
    }

    public ConnextionInfo Raw { get; private set; }
    public bool IsLoaded => Raw != null;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    static bool IsDebug => Debug.isDebugBuild || Application.isEditor;

    public string ApiUrl =>
        IsDebug && !string.IsNullOrEmpty(Raw?.serverWebAPIAdressDebug)
            ? Raw.serverWebAPIAdressDebug : Raw?.serverWebAPIAdress;

    public string TokenUrl =>
        IsDebug && !string.IsNullOrEmpty(Raw?.serverWebTokenDebug)
            ? Raw.serverWebTokenDebug : Raw?.serverWebToken;

    public string GameUrl =>
        IsDebug && !string.IsNullOrEmpty(Raw?.serverGameAdressDebug)
            ? Raw.serverGameAdressDebug : Raw?.serverGameAdress;

    // Synchronous load, works in Editor, Standalone, and WebGL (via Resources)
    public void Load()
    {
        if (Raw != null) return;

#if UNITY_WEBGL
        // WebGL: must use Resources (StreamingAssets sync read is not available)
        var ta = Resources.Load<TextAsset>("config"); // file at Assets/Resources/config.json
        if (ta == null)
        {
            Debug.LogError("Resources/config.json not found.");
            return;
        }
        Raw = JsonConvert.DeserializeObject<ConnextionInfo>(ta.text);
#else
        // You can also unify by using Resources everywhere. If you prefer StreamingAssets on PC:
        var res = Resources.Load<TextAsset>("config");
        if (res != null)
        {
            Raw = JsonConvert.DeserializeObject<ConnextionInfo>(res.text);
        }
        else
        {
            var path = Path.Combine(Application.streamingAssetsPath, "config.json");
            if (!File.Exists(path))
            {
                Debug.LogError("Config not found. Put config.json in Resources or StreamingAssets.");
                return;
            }
            var json = File.ReadAllText(path);
            Raw = JsonConvert.DeserializeObject<ConnextionInfo>(json);
        }
#endif

        if (Raw == null) Debug.LogError("Config parse failed.");
    }
}
