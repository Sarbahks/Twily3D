using System.Collections;
using UnityEngine;

public class PanwScript : MonoBehaviour
{
    [SerializeField] private GameObject objectPawn;
    [SerializeField] private float spawnDuration = 0.5f;

    public void SpawnPawn()
    {
        SpawnPawn(Vector3.zero);
    }

    public void SpawnPawn(Vector3 position)
    {
        // Reset pawn
        objectPawn.SetActive(true);
        objectPawn.transform.position = position;
        objectPawn.transform.localScale = Vector3.zero;

        // Start pop-in effect
        StartCoroutine(AnimateSpawn(objectPawn));
    }

    private IEnumerator AnimateSpawn(GameObject obj)
    {
        float elapsed = 0f;
        Vector3 targetScale = Vector3.one;

        Material mat = obj.GetComponentInChildren<Renderer>().material;

        float startAlpha = 0f;
        float targetAlpha = 1f;

        float flash = 2.0f; // flash multiplier (2x brightness)

        // Set initial flash boost
        if (mat.HasProperty("_Flash"))
            mat.SetFloat("_Flash", flash);

        while (elapsed < spawnDuration)
        {
            float t = elapsed / spawnDuration;

            // Scale up
            obj.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);

            // Fade in alpha
            if (mat.HasProperty("_Alpha"))
                mat.SetFloat("_Alpha", Mathf.Lerp(startAlpha, targetAlpha, t));

            elapsed += Time.deltaTime;
            yield return null;
        }

        // End flash
        if (mat.HasProperty("_Flash"))
            mat.SetFloat("_Flash", 1.0f); // return to normal

        // Final values
        obj.transform.localScale = targetScale;
        if (mat.HasProperty("_Alpha"))
            mat.SetFloat("_Alpha", targetAlpha);
    }

}
