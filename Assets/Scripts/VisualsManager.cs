using System.Collections.Generic;
using UnityEngine;

public class VisualsManager : MonoBehaviour
{
    private static VisualsManager instance;

    public static VisualsManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindAnyObjectByType<VisualsManager>();
            }

            return instance;
        }
    }

    public List<AreaBoard> AreaBoards { get => areaBoards; set => areaBoards = value; }

    [SerializeField]
    private Camera cameraGame;

    [SerializeField]
    private List<AreaBoard> areaBoards = new List<AreaBoard>();

    [Header("Rotation Speeds (Degrees per Second)")]
    [SerializeField] private float ring1Speed = 30f;
    [SerializeField] private float ring2Speed = 30f;
    [SerializeField] private float ring3Speed = 30f;

    [Header("Rotation Direction (true = clockwise)")]
    [SerializeField] private bool ring1Clockwise = true;
    [SerializeField] private bool ring3Clockwise = true;

    void Update()
    {
        float deltaTime = Time.deltaTime;

        foreach (var area in AreaBoards)
        {
            if (area != null && area.IsActive)
            {

                RotateRing(area.Ring1, ring1Speed, ring1Clockwise, deltaTime);
                RotateRing(area.Ring2, ring2Speed, !ring1Clockwise, deltaTime); // Opposite direction
                RotateRing(area.Ring3, ring3Speed, ring3Clockwise, deltaTime);
            }

        }
    }

    private void RotateRing(GameObject ring, float speed, bool clockwise, float deltaTime)
    {
        if (ring == null) return;

        float direction = clockwise ? 1f : -1f;
        ring.transform.Rotate(0f, speed * direction * deltaTime, 0f, Space.Self);
    }
}
