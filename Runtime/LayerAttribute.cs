using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
// https://discussions.unity.com/t/type-for-layer-selection/91723

/// <summary>
/// Attribute to select a single layer.
/// </summary>
public class LayerAttribute : PropertyAttribute
{
    // NOTHING - just oxygen.
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(LayerAttribute))]
class LayerAttributeEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // One line of  oxygen free code.
        property.intValue = EditorGUI.LayerField(position, label, property.intValue);
    }
}
#endif
