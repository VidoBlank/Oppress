using UnityEngine;

public class DebugLevelPathGUI : MonoBehaviour
{
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, 10, 300, 30), $"LevelPath.Count: {GameData.Instance.LevelPath.Count}", style);
        
    }
}
