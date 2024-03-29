using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum UIElementType
{
    None = 0,
    Button,
    Text,
    Image,
    UIWidget,
    GameObject,
    Slider,
    TMPText,
}

public class UIElement : MonoBehaviour
{
    public UIElementType elementType;
    [SerializeField]
    protected string refAlias;

    public static UIElementType ObjType2UIElementType(System.Type tp)
    {
        if (tp == typeof(Button))
            return UIElementType.Button;
        else if (tp == typeof(Text))
            return UIElementType.Text;
        else if (tp == typeof(Image))
            return UIElementType.Image;
        else if (tp == typeof(UIWidget))
            return UIElementType.UIWidget;
        else if (tp == typeof(GameObject))
            return UIElementType.GameObject;
        else if (tp == typeof(Slider))
            return UIElementType.Slider;
        else if (tp == typeof(TMP_Text))
            return UIElementType.TMPText;
        else
            Debug.LogError($"[UI]not implement objtype {tp} to element type");
        return UIElementType.None;
    }

    public string refName
    {
        get
        {
            if (string.IsNullOrEmpty(refAlias))
                return name;
            return refAlias;
        }
    }

    public Object obj
    {
        get
        {
            Object ret = null;
            if (elementType == UIElementType.Button)
                ret = GetComponent<Button>();
            else if (elementType == UIElementType.Text)
                ret = GetComponent<Text>();
            else if (elementType == UIElementType.Image)
                ret = GetComponent<Image>();
            else if (elementType == UIElementType.UIWidget)
                ret = GetComponent<UIWidget>();
            else if (elementType == UIElementType.GameObject)
                ret = gameObject;
            else if (elementType == UIElementType.Slider)
                ret = GetComponent<Slider>();
            else if (elementType == UIElementType.TMPText)
                ret = GetComponent<TMP_Text>();
            
            if (ret == null)
                Debug.LogError($"[UI]not implement elementType > {elementType} at {Utils.GetHierarchyPath(gameObject.transform)}");
            return ret;
        }
    }
}
