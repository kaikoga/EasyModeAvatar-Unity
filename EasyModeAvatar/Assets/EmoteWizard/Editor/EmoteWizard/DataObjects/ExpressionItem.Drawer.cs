using EmoteWizard.Extensions;
using UnityEditor;
using UnityEngine;
using static EmoteWizard.Extensions.EditorUITools;

namespace EmoteWizard.DataObjects
{
    [CustomPropertyDrawer(typeof(ExpressionItem))]
    public class ExpressionItemDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.Box(position, GUIContent.none);
            position = PropertyDrawerUITools.InsideBox(position);
            using (new EditorGUI.PropertyScope(position, label, property))
            using (new EditorGUI.IndentLevelScope())
            {
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 1f;
                EditorGUI.PropertyField(new Rect(position.x + position.width * 0.00f, position.y + PropertyDrawerUITools.LineTop(0), position.width * 0.4f, PropertyDrawerUITools.LineHeight(1)), property.FindPropertyRelative("icon"), new GUIContent(" "));
                EditorGUI.PropertyField(new Rect(position.x + position.width * 0.40f, position.y + PropertyDrawerUITools.LineTop(0), position.width * 0.6f, PropertyDrawerUITools.LineHeight(1)), property.FindPropertyRelative("path"), new GUIContent(" "));
                EditorGUI.PropertyField(new Rect(position.x + position.width * 0.00f, position.y + PropertyDrawerUITools.LineTop(1), position.width * 0.4f, PropertyDrawerUITools.LineHeight(1)), property.FindPropertyRelative("parameter"), new GUIContent(" "));
                EditorGUI.PropertyField(new Rect(position.x + position.width * 0.40f, position.y + PropertyDrawerUITools.LineTop(1), position.width * 0.2f, PropertyDrawerUITools.LineHeight(1)), property.FindPropertyRelative("value"), new GUIContent(" "));
                EditorGUI.PropertyField(new Rect(position.x + position.width * 0.60f, position.y + PropertyDrawerUITools.LineTop(1), position.width * 0.4f, PropertyDrawerUITools.LineHeight(1)), property.FindPropertyRelative("controlType"), new GUIContent(" "));
                EditorGUIUtility.labelWidth = labelWidth;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return PropertyDrawerUITools.BoxHeight(PropertyDrawerUITools.LineHeight(2f));
        }
    }
}