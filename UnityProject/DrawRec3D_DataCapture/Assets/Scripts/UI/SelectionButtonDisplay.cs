using UnityEngine;  
using UnityEngine.UI;

public class SelectionButtonDisplay : MonoBehaviour
{
    private Text displayText;

    public void SetText(string newText)
    {
        displayText.text = newText;
    }
}
