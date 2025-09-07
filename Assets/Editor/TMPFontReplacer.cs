using UnityEngine;
using UnityEditor;
using TMPro;

public class TMPFontReplacer : EditorWindow
{
    public TMP_FontAsset newFont;

    [MenuItem("Tools/Replace TMP Fonts in Scene")]
    public static void ShowWindow()
    {
        GetWindow<TMPFontReplacer>("Replace TMP Fonts");
    }

    void OnGUI()
    {
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("New TMP Font", newFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("Replace All TMP Fonts"))
        {
            if (newFont == null)
            {
                Debug.LogWarning("No TMP font assigned.");
                return;
            }

            // Updated API
            var textsUGUI = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            var texts3D = FindObjectsByType<TextMeshPro>(FindObjectsSortMode.None);

            int count = 0;

            foreach (var t in textsUGUI)
            {
                Undo.RecordObject(t, "Replace TMP Font");
                t.font = newFont;
                EditorUtility.SetDirty(t);
                count++;
            }

            foreach (var t in texts3D)
            {
                Undo.RecordObject(t, "Replace TMP Font");
                t.font = newFont;
                EditorUtility.SetDirty(t);
                count++;
            }

            Debug.Log($"Replaced font in {count} TMP components.");
        }
    }
}
