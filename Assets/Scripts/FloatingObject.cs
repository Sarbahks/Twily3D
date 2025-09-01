using UnityEngine;

/// <summary>
/// FloatingObject: add to any GameObject to make it float/bob/sway,
/// optionally with a gentle rotation wobble. Works in local or world space.
/// </summary>
[DisallowMultipleComponent]
public class FloatingObject : MonoBehaviour
{
    [Header("General")]
    [Tooltip("Apply offsets in local space (recommended for most cases). If false, uses world space.")]
    public bool useLocalSpace = true;

    [Tooltip("Use unscaled time (ignores timescale).")]
    public bool useUnscaledTime = false;

    [Tooltip("Randomize starting phase so multiple objects don't bob in sync.")]
    public bool randomizePhaseOnEnable = true;

    [Header("Position Float")]
    public bool enablePosition = true;

    [Tooltip("Vertical bob amplitude in units.")]
    public float verticalAmplitude = 0.1f;

    [Tooltip("Vertical bob speed in cycles per second.")]
    public float verticalFrequency = 1.25f;

    [Space(5)]
    [Tooltip("Sway along local X (right/left). 0 to disable.")]
    public float swayXAmplitude = 0.0f;

    [Tooltip("Sway X speed in cycles per second.")]
    public float swayXFrequency = 0.8f;

    [Tooltip("Sway along local Z (forward/back). Positive values sway forward/back. Use a small value like 0.05.")]
    public float swayZAmplitude = 0.05f;

    [Tooltip("Sway Z speed in cycles per second.")]
    public float swayZFrequency = 0.9f;

    [Header("Rotation Wobble")]
    public bool enableRotation = false;

    [Tooltip("Rotation wobble amplitudes in degrees (pitch, yaw, roll).")]
    public Vector3 rotationAmplitude = new Vector3(4f, 0f, 4f);

    [Tooltip("Rotation wobble frequencies in cycles per second per axis.")]
    public Vector3 rotationFrequency = new Vector3(0.6f, 0.4f, 0.7f);

    [Header("Phase Offsets (advanced)")]
    [Tooltip("Manual phase offsets (radians) for position channels: X, Y, Z.")]
    public Vector3 positionPhase = Vector3.zero;

    [Tooltip("Manual phase offsets (radians) for rotation channels: X, Y, Z.")]
    public Vector3 rotationPhase = Vector3.zero;

    // cached bases
    private Vector3 _basePosLocal;
    private Vector3 _basePosWorld;
    private Quaternion _baseRotLocal;
    private Quaternion _baseRotWorld;

    // randomized phase offsets so instances de-sync
    private Vector3 _randPosPhase;
    private Vector3 _randRotPhase;

    private void OnEnable()
    {
        CacheBaseTransforms();
        if (randomizePhaseOnEnable)
        {
            // simple per-instance randomness
            float r1 = Random.value * Mathf.PI * 2f;
            float r2 = Random.value * Mathf.PI * 2f;
            float r3 = Random.value * Mathf.PI * 2f;
            _randPosPhase = new Vector3(r1, r2, r3);

            float r4 = Random.value * Mathf.PI * 2f;
            float r5 = Random.value * Mathf.PI * 2f;
            float r6 = Random.value * Mathf.PI * 2f;
            _randRotPhase = new Vector3(r4, r5, r6);
        }
        else
        {
            _randPosPhase = Vector3.zero;
            _randRotPhase = Vector3.zero;
        }
    }

    private void Reset()
    {
        // nice defaults
        useLocalSpace = true;
        useUnscaledTime = false;
        randomizePhaseOnEnable = true;

        enablePosition = true;
        verticalAmplitude = 0.1f;
        verticalFrequency = 1.25f;
        swayXAmplitude = 0.0f;
        swayXFrequency = 0.8f;
        swayZAmplitude = 0.05f;
        swayZFrequency = 0.9f;

        enableRotation = false;
        rotationAmplitude = new Vector3(4f, 0f, 4f);
        rotationFrequency = new Vector3(0.6f, 0.4f, 0.7f);
    }

    private void CacheBaseTransforms()
    {
        _basePosLocal = transform.localPosition;
        _basePosWorld = transform.position;
        _baseRotLocal = transform.localRotation;
        _baseRotWorld = transform.rotation;
    }

    /// <summary>
    /// Call if you need to re-anchor the float baseline at runtime (e.g., after teleport).
    /// </summary>
    public void ReanchorNow()
    {
        CacheBaseTransforms();
    }

    private void Update()
    {
        float t = useUnscaledTime ? Time.unscaledTime : Time.time;

        // ----- POSITION -----
        if (enablePosition)
        {
            // waves (sin for Y, cos for X/Z just to offset phase visually)
            float y = Mathf.Sin((t * Mathf.PI * 2f) * verticalFrequency + positionPhase.y + _randPosPhase.y) * verticalAmplitude;
            float x = Mathf.Cos((t * Mathf.PI * 2f) * swayXFrequency + positionPhase.x + _randPosPhase.x) * swayXAmplitude;
            float z = Mathf.Cos((t * Mathf.PI * 2f) * swayZFrequency + positionPhase.z + _randPosPhase.z) * swayZAmplitude;

            if (useLocalSpace)
            {
                // local axes
                Vector3 offset = new Vector3(x, y, z);
                transform.localPosition = _basePosLocal + offset;
            }
            else
            {
                // world axes
                Vector3 offset = new Vector3(x, y, z);
                transform.position = _basePosWorld + offset;
            }
        }

        // ----- ROTATION -----
        if (enableRotation)
        {
            float rx = Mathf.Sin((t * Mathf.PI * 2f) * rotationFrequency.x + rotationPhase.x + _randRotPhase.x) * rotationAmplitude.x;
            float ry = Mathf.Sin((t * Mathf.PI * 2f) * rotationFrequency.y + rotationPhase.y + _randRotPhase.y) * rotationAmplitude.y;
            float rz = Mathf.Sin((t * Mathf.PI * 2f) * rotationFrequency.z + rotationPhase.z + _randRotPhase.z) * rotationAmplitude.z;

            Quaternion wobble = Quaternion.Euler(rx, ry, rz);

            if (useLocalSpace)
                transform.localRotation = _baseRotLocal * wobble;
            else
                transform.rotation = _baseRotWorld * wobble;
        }
    }
}
