using Unity.Cinemachine;
using UnityEngine;

public class cameramanager : MonoBehaviour
{
    public static cameramanager instance;

    [Header("Camera Shake Settings")]
    [SerializeField] private Vector2 shakeVelocity;

    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        instance = this;
        impulseSource = GetComponent<CinemachineImpulseSource>();
        
    }

    // Camera Shake function
    public void CameraShake()
    {
        impulseSource.DefaultVelocity = new Vector2 (shakeVelocity.x, shakeVelocity.y);
        impulseSource.GenerateImpulse();
    }
}
