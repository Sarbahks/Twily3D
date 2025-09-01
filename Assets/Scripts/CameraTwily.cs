// CameraTwily.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraTwily : MonoBehaviour
{

    [Header("Focus Mode (Cinematics)")]
    public bool focusPlayer = false;
    

    private static CameraTwily instance;

    public static CameraTwily Instance
    {
        get
        {

            if (instance == null)
            {
                instance = FindFirstObjectByType<CameraTwily>();
            }
            return instance;
        }
    }

    public void SetFocusPlayer(bool focusPlayer)
    {
        this.focusPlayer = focusPlayer;
    }



    [SerializeField] private GameObject pawn;
    [SerializeField] private Vector3 offset = new Vector3(0f, 10f, -10f); // relative position
    [SerializeField] private float followSpeed = 5f; // higher = snappier

    private void LateUpdate()
    {
      //  if (!focusPlayer) return;

        // target position relative to pawn
        Vector3 targetPos = pawn.transform.position + offset;

        // smooth follow
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        // look at the pawn
        transform.LookAt(pawn.transform);
    }



}
