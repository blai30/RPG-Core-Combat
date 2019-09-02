using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ContextMenu
{
    /// <summary>
    /// Convert Unity GUI text component to TextMesh Pro text component
    /// </summary>
    /// <param name="menuCommand"></param>
    [MenuItem("CONTEXT/Text/Convert to TextMesh Pro")]
    static void ConvertToTextMeshPro(MenuCommand menuCommand)
    {
        Text text = (Text) menuCommand.context;
        GameObject textGameObject = text.gameObject;
        string textValue = text.text;
        Object.DestroyImmediate(text);
        textGameObject.AddComponent<TMPro.TextMeshProUGUI>().text = textValue;
    }

    /// <summary>
    /// Convert Unity GUI text component to TextMesh Pro text component
    /// </summary>
    /// <param name="menuCommand"></param>
    [MenuItem("CONTEXT/TMP_Text/Convert to Unity GUI Text")]
    static void ConvertToUnityGuiText(MenuCommand menuCommand)
    {
        TMPro.TMP_Text text = (TMPro.TMP_Text) menuCommand.context;
        GameObject textGameObject = text.gameObject;
        string textValue = text.text;
        Object.DestroyImmediate(text);
        textGameObject.AddComponent<Text>().text = textValue;
    }
}
