using EmoteWizard.Base;
using EmoteWizard.DataObjects;
using EmoteWizard.Extensions;
using UnityEditor;
using UnityEngine;
using static EmoteWizard.Extensions.EditorUITools;

namespace EmoteWizard
{
    [CustomEditor(typeof(ParametersWizard))]
    public class ParametersWizardEditor : AnimationWizardBaseEditor
    {
        ParametersWizard parametersWizard;

        void OnEnable()
        {
            parametersWizard = target as ParametersWizard;
        }

        public override void OnInspectorGUI()
        {
            var serializedObj = this.serializedObject;

            SetupOnlyUI(parametersWizard, () =>
            {
                if (GUILayout.Button("Repopulate Parameters"))
                {
                    parametersWizard.parameterItems.Clear();
                    parametersWizard.RefreshParameters();
                }
            });

            EditorGUILayout.PropertyField(serializedObj.FindProperty("vrcDefaultParameters"));
            if (GUILayout.Button("Collect Parameters"))
            {
                parametersWizard.RefreshParameters();
            }

            ParameterItemDrawer.DrawHeader();
            EditorGUILayout.PropertyField(serializedObj.FindProperty("parameterItems"), true);

            OutputUIArea(parametersWizard, () =>
            {
                if (GUILayout.Button("Generate Expression Parameters"))
                {
                    BuildExpressionParameters();
                }
                EditorGUILayout.PropertyField(serializedObj.FindProperty("outputAsset"));
            });

            serializedObj.ApplyModifiedProperties();
        }

        void BuildExpressionParameters()
        {
            var expressionParams = parametersWizard.ReplaceOrCreateOutputAsset(ref parametersWizard.outputAsset, "Expressions/@@@Generated@@@ExprParams.asset");

            expressionParams.parameters = parametersWizard.ToParameters();

            AssetDatabase.SaveAssets();
        }

    }
}