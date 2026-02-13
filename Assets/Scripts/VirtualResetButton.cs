using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VirtualResetButton : MonoBehaviour
{
    [Header("Visual Settings")]
    public Transform buttonCap; // The part of the button that moves down
    public float pressDistance = 0.02f;
    public Color normalColor = Color.red;
    public Color pressedColor = Color.white;

    private Vector3 initialLocalPos;
    private MeshRenderer buttonRenderer;
    private bool isPressed = false;

    void Start()
    {
        if (buttonCap != null)
        {
            initialLocalPos = buttonCap.localPosition;
            buttonRenderer = buttonCap.GetComponent<MeshRenderer>();
            if (buttonRenderer != null) buttonRenderer.material.color = normalColor;
        }

        // Ensure we have an XRSimpleInteractable to detect "pokes" or "clicks"
        var interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnSelectEnter);
        }
    }

    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        if (!isPressed)
        {
            // Trigger Haptic on the hand that pressed it
            if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor inputInteractor)
            {
                inputInteractor.SendHapticImpulse(0.5f, 0.1f);
            }
            
            PressButton();
        }
    }

    public void PressButton()
    {
        isPressed = true;

        // 1. Visual Feedback
        if (buttonCap != null)
        {
            buttonCap.localPosition = initialLocalPos + Vector3.down * pressDistance;
            if (buttonRenderer != null) buttonRenderer.material.color = pressedColor;
        }

        // 2. Audio Feedback
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }

        // 3. Action: Reset the Puck
        if (PuckController.Instance != null)
        {
            PuckController.Instance.ResetPuck();
            Debug.Log("Virtual Button Pressed - Resetting Puck");
        }

        // 4. Reset button position after a short delay
        Invoke(nameof(ReleaseButton), 0.2f);
    }

    private void ReleaseButton()
    {
        isPressed = false;
        if (buttonCap != null)
        {
            buttonCap.localPosition = initialLocalPos;
            if (buttonRenderer != null) buttonRenderer.material.color = normalColor;
        }
    }
}
