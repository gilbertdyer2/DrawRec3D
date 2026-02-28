using UnityEngine;

[ExecuteInEditMode] // Runs in play mode too
public class Test_AlphaValTrap : MonoBehaviour
{
    // private CanvasGroup _canvasGroup;
    // private float _lastAlpha;

    // void Start()
    // {
    //     _canvasGroup = GetComponent<CanvasGroup>();
    //     _lastAlpha = _canvasGroup.alpha;
    // }

    // void Update()
    // {
    //     if (_canvasGroup.alpha != _lastAlpha)
    //     {
    //         // This is the "Aha!" moment
    //         Debug.Log($"Alpha changed from {_lastAlpha} to {_canvasGroup.alpha} by something!", gameObject);
            
    //         // If you want to stop the code execution exactly when it happens:
    //         Debug.Break(); 
            
    //         _lastAlpha = _canvasGroup.alpha;
    //     }
    // }

    // private CanvasGroup _canvasGroup;

    // void Awake() => _canvasGroup = GetComponent<CanvasGroup>();

    // void Update()
    // {
    //     // If the alpha is suddenly 0, we pause the entire engine.
    //     if (_canvasGroup.alpha == 0)
    //     {
    //         Debug.Log("ALPHA IS ZERO! Look at the list below this message in the Console.");
    //         Debug.Break(); // This pauses the editor
    //     }
    // }
    
    private CanvasGroup _group;

    void Awake() => _group = GetComponent<CanvasGroup>();

    void LateUpdate()
    {
        if (_group.alpha < 1.0f)
        {
            // If it's zero, print exactly when it happened
            Debug.Log($"[Frame {Time.frameCount}] Alpha was detected as {_group.alpha}. Forcing back to 1.0.");
            
            // Try to force it back to 1.0
            _group.alpha = 1.0f;
        }
    }
}