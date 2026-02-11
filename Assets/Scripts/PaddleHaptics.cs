using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PaddleHaptics : MonoBehaviour
{
    [Header("Haptic Settings")]
    [Range(0, 1)]
    public float intensity = 0.5f;
    public float duration = 0.1f;

    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only trigger haptics if the paddle is currently being held
        if (grabInteractable != null && grabInteractable.isSelected)
        {
            // Send haptic impulse to the interactor (the hand) holding the paddle
            TriggerHaptic(grabInteractable.firstInteractorSelecting);
        }
    }

    private void TriggerHaptic(IXRSelectInteractor interactor)
    {
        // Try to cast the interactor to a controller interactor which has built-in haptic support
        if (interactor is XRBaseInputInteractor controllerInteractor)
        {
            controllerInteractor.SendHapticImpulse(intensity, duration);
        }

        // --- NEW: Send haptic signal to ESP32 via WebSocket ---
        if (WebSocketClientExample.Instance != null)
        {
            // Convert intensity (0-1) to byte (0-255) for ESP32
            int espIntensity = Mathf.RoundToInt(intensity * 255);
            WebSocketClientExample.Instance.SendHapticSignal(espIntensity);

            // Send a "turn off" signal after a short delay to create a pulse effect
            Invoke(nameof(TurnOffEspLed), 0.5f);
        }
    }

    private void TurnOffEspLed()
    {
        if (WebSocketClientExample.Instance != null)
        {
            WebSocketClientExample.Instance.SendHapticSignal(0);
        }
    }
}
