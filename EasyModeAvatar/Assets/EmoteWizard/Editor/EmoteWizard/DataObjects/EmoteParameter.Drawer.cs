using EmoteWizard.Base;
using EmoteWizard.Extensions;
using EmoteWizard.UI;
using UnityEditor;
using UnityEngine;
using static EmoteWizard.Tools.PropertyDrawerUITools;

namespace EmoteWizard.DataObjects
{
    [CustomPropertyDrawer(typeof(EmoteParameter))]
    public class EmoteParameterDrawer : PropertyDrawerWithContext<EmoteParameterDrawer.Context>
    {
        public static Context StartContext(EmoteWizardRoot emoteWizardRoot, bool isEditing) => PropertyDrawerWithContext<Context>.StartContext(new Context(emoteWizardRoot, isEditing));

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var context = EnsureContext(property);

            using (new EditorGUI.PropertyScope(position, label, property))
            {
                var normalizedTimeEnabled = property.FindPropertyRelative("normalizedTimeEnabled");
                var normalizedTimeLeft = property.FindPropertyRelative("normalizedTimeLeft");
                var normalizedTimeRight = property.FindPropertyRelative("normalizedTimeRight");
                if (context.IsEditing)
                {
                    EditorGUI.PropertyField(position.SliceV(0), normalizedTimeEnabled, new GUIContent("Normalized Time"));
                    using (new EditorGUI.IndentLevelScope())
                    using (new EditorGUI.DisabledScope(!normalizedTimeEnabled.boolValue))
                    {
                        EditorGUI.PropertyField(position.SliceV( 1), normalizedTimeLeft, new GUIContent("Parameter Left"));
                        EditorGUI.PropertyField(position.SliceV(2), normalizedTimeRight, new GUIContent("Parameter Right"));
                    }
                }
                else
                {
                    var parameterLabel = "";
                    if (normalizedTimeEnabled.boolValue)
                    {
                        parameterLabel += $"Normalized Time:{normalizedTimeLeft.stringValue}/{normalizedTimeRight.stringValue})";
                    }
                    GUI.Label(position, parameterLabel);
                }
            }
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var context = EnsureContext(property);

            if (context.IsEditing)
            {
                return LineHeight(3f);
            }
            if (property.FindPropertyRelative("normalizedTimeEnabled").boolValue)
            {
                return LineHeight(1f);
            }
            return LineHeight(0f);
        }
        
        public class Context : ContextBase
        {
            public readonly bool IsEditing;

            public Context() : base(null) { }
            public Context(EmoteWizardRoot emoteWizardRoot, bool isEditing) : base(emoteWizardRoot) => IsEditing = isEditing;
        }
    }
}