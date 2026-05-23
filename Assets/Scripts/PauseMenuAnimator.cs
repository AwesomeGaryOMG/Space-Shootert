using UnityEngine;
using System.Collections;

public class PauseMenuAnimator : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform headerText;
    public RectTransform[] leftButtons;
    public RectTransform[] rightButtons;

    [Header("Animation Settings")]
    public float animationDuration = 0.6f;
    public AnimationCurve bounceCurve; // Controls the header drop/bounce
    public AnimationCurve slideCurve;  // Controls the button slide

    [Header("Off-Screen Start Distances")]
    public float headerYOffset = 600f; // How high up the text starts
    public float buttonXOffset = 800f; // How far left/right buttons start

    private Vector2 headerStartPos;
    private Vector2[] leftButtonsStartPos;
    private Vector2[] rightButtonsStartPos;

    void Awake()
    {
        // Store the original positions you set in the Inspector so it knows exactly where to land
        headerStartPos = headerText.anchoredPosition;

        leftButtonsStartPos = new Vector2[leftButtons.Length];
        for (int i = 0; i < leftButtons.Length; i++)
            leftButtonsStartPos[i] = leftButtons[i].anchoredPosition;

        rightButtonsStartPos = new Vector2[rightButtons.Length];
        for (int i = 0; i < rightButtons.Length; i++)
            rightButtonsStartPos[i] = rightButtons[i].anchoredPosition;
    }

    // OnEnable runs automatically exactly when the Pause Menu is turned on
    void OnEnable() 
    {
        StartCoroutine(AnimateMenu());
    }

    IEnumerator AnimateMenu()
    {
        float time = 0f;

        // 1. Instantly move everything off-screen before the first frame draws
        headerText.anchoredPosition = headerStartPos + new Vector2(0, headerYOffset);

        for (int i = 0; i < leftButtons.Length; i++)
            leftButtons[i].anchoredPosition = leftButtonsStartPos[i] + new Vector2(-buttonXOffset, 0);

        for (int i = 0; i < rightButtons.Length; i++)
            rightButtons[i].anchoredPosition = rightButtonsStartPos[i] + new Vector2(buttonXOffset, 0);

        // 2. Animate them sliding into place
        while (time < animationDuration)
        {
            // CRITICAL: We use unscaledDeltaTime because the game is paused (timeScale = 0)!
            time += Time.unscaledDeltaTime; 
            float percent = time / animationDuration;

            // Read the graph curves for that bouncy feel
            float bouncePercent = bounceCurve.Evaluate(percent);
            float slidePercent = slideCurve.Evaluate(percent);

            // LerpUnclamped allows the elements to overshoot and bounce back
            headerText.anchoredPosition = Vector2.LerpUnclamped(headerStartPos + new Vector2(0, headerYOffset), headerStartPos, bouncePercent);

            for (int i = 0; i < leftButtons.Length; i++)
                leftButtons[i].anchoredPosition = Vector2.LerpUnclamped(leftButtonsStartPos[i] + new Vector2(-buttonXOffset, 0), leftButtonsStartPos[i], slidePercent);

            for (int i = 0; i < rightButtons.Length; i++)
                rightButtons[i].anchoredPosition = Vector2.LerpUnclamped(rightButtonsStartPos[i] + new Vector2(buttonXOffset, 0), rightButtonsStartPos[i], slidePercent);

            yield return null;
        }

        // 3. Snap exactly to final positions just to be safe
        headerText.anchoredPosition = headerStartPos;
        for (int i = 0; i < leftButtons.Length; i++) leftButtons[i].anchoredPosition = leftButtonsStartPos[i];
        for (int i = 0; i < rightButtons.Length; i++) rightButtons[i].anchoredPosition = rightButtonsStartPos[i];
    }
}