using UnityEngine;
using UnityEngine.UI;

public class PointDisplay : MonoBehaviour
{
    public string pointText = "POINT";
    public Color pointColor = Color.white;
    
    void Start()
    {
        UpdateDisplay();
    }
    
    public void SetPointText(string text)
    {
        pointText = text;
        UpdateDisplay();
    }
    
    public void SetPointColor(Color color)
    {
        pointColor = color;
        UpdateDisplay();
    }
    
    void UpdateDisplay()
    {
        Text textComponent = GetComponent<Text>();
        if (textComponent != null)
        {
            textComponent.text = pointText;
            textComponent.color = pointColor;
        }
    }
}
