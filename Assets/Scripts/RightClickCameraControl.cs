using UnityEngine;
using Cinemachine;

public class RightClickCameraControl : MonoBehaviour
{
    public CinemachineFreeLook freeLookCamera = null;
    [Range(0.1f, 25f)] public float xRotationSpeed = 5f;
    [Range(0.1f, 5f)] public float yRotationSpeed = 0.1f;

    // Zoom script source: https://discussions.unity.com/t/cinemachine-how-to-add-zoom-control-to-freelook-camera/683421/32
    private CinemachineFreeLook.Orbit[] originalOrbits;
    [Tooltip("The minimum scale for the orbits")]
    [Range(0.01f, 1f)]
    public float minScale = 0.5f;

    [Tooltip("The maximum scale for the orbits")]
    [Range(1F, 5f)]
    public float maxScale = 1;

    [Tooltip("The zoom axis.  Value is 0..1.  How much to scale the orbits")]
    [AxisStateProperty]
    public AxisState zAxis = new AxisState(-0.5f, 1, false, true, 50f, 0.1f, 0.1f, "Mouse ScrollWheel", true);

    void OnValidate()
    {
        minScale = Mathf.Max(0.01f, minScale);
        maxScale = Mathf.Max(minScale, maxScale);
    }
    void Awake()
    {
        freeLookCamera = GetComponent<CinemachineFreeLook>();
        if (freeLookCamera != null)
        {
            originalOrbits = new CinemachineFreeLook.Orbit[freeLookCamera.m_Orbits.Length];
            for (int i = 0; i < originalOrbits.Length; i++)
            {
                originalOrbits[i].m_Height = freeLookCamera.m_Orbits[i].m_Height;
                originalOrbits[i].m_Radius = freeLookCamera.m_Orbits[i].m_Radius;
            }
#if UNITY_EDITOR
                SaveDuringPlay.SaveDuringPlay.OnHotSave -= RestoreOriginalOrbits;
                SaveDuringPlay.SaveDuringPlay.OnHotSave += RestoreOriginalOrbits;
#endif
        }
    }

#if UNITY_EDITOR
        private void OnDestroy()
        {
            SaveDuringPlay.SaveDuringPlay.OnHotSave -= RestoreOriginalOrbits;
        }

        private void RestoreOriginalOrbits()
        {
            if (originalOrbits != null)
            {
                for (int i = 0; i < originalOrbits.Length; i++)
                {
                    freeLookCamera.m_Orbits[i].m_Height = originalOrbits[i].m_Height;
                    freeLookCamera.m_Orbits[i].m_Radius = originalOrbits[i].m_Radius;
                }
            }
        }
#endif

    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            freeLookCamera.m_XAxis.Value += mouseX * xRotationSpeed; // Adjust sensitivity as needed
            freeLookCamera.m_YAxis.Value -= mouseY * yRotationSpeed; // Adjust sensitivity as needed
        }
        if (originalOrbits != null)
        {
            zAxis.Update(Time.deltaTime);
            float scale = Mathf.Lerp(minScale, maxScale, zAxis.Value);
            for (int i = 0; i < originalOrbits.Length; i++)
            {
                freeLookCamera.m_Orbits[i].m_Height = originalOrbits[i].m_Height * scale;
                freeLookCamera.m_Orbits[i].m_Radius = originalOrbits[i].m_Radius * scale;
            }
        }

    }
}
