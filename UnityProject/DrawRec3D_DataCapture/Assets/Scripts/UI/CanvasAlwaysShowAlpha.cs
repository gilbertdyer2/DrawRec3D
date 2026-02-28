// (Script Adapted from Oculus SDK)
using UnityEngine;
using UnityEngine.Assertions;

public class CanvasAlwaysShowAlpha : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float animationSpeed;
    private bool visible;

    public void ToggleVisible()
    {
        visible = !visible;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    void Start()
    {
        Assert.IsNotNull(canvasGroup);
        visible = true;
        canvasGroup.alpha = 1;
    }

    void Update()
    {
        // canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, visible ? 1.0f : 0.0f, animationSpeed * Time.deltaTime);
    }
}
