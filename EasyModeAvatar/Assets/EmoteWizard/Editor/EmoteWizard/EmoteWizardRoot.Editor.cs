using EmoteWizard.Tools;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using static EmoteWizard.Tools.EmoteWizardEditorTools;

namespace EmoteWizard
{
    [CustomEditor(typeof(EmoteWizardRoot))]
    public class EmoteWizardRootEditor : Editor
    {
        EmoteWizardRoot emoteWizardRoot;

        void OnEnable()
        {
            emoteWizardRoot = target as EmoteWizardRoot;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using (new GUILayout.HorizontalScope())
            {
                emoteWizardRoot.generatedAssetRoot = EditorGUILayout.TextField("Generated Assets Root", emoteWizardRoot.generatedAssetRoot);
                if (GUILayout.Button("Browse"))
                {
                    SelectFolder("Select Generated Assets Root", ref emoteWizardRoot.generatedAssetRoot);
                }
            }
            if (!emoteWizardRoot.GetComponent<SetupWizard>())
            {
                if (GUILayout.Button("Setup"))
                {
                    EnsureComponent<SetupWizard>();
                }
            }
            var avatarDescriptor = emoteWizardRoot.avatarDescriptor;
            if (avatarDescriptor)
            {
                if (GUILayout.Button("Update Avatar"))
                {
                    UpdateAvatar(avatarDescriptor);
                }
            }
        }

        void UpdateAvatar(VRCAvatarDescriptor avatarDescriptor)
        {
            avatarDescriptor.customizeAnimationLayers = true;
            avatarDescriptor.baseAnimationLayers = new[]
            {
                new VRCAvatarDescriptor.CustomAnimLayer
                {
                    animatorController = null,
                    isDefault = true,
                    isEnabled = false,
                    mask = null,
                    type = VRCAvatarDescriptor.AnimLayerType.Base
                },
                new VRCAvatarDescriptor.CustomAnimLayer
                {
                    animatorController = null,
                    isDefault = true,
                    isEnabled = false,
                    mask = null,
                    type = VRCAvatarDescriptor.AnimLayerType.Additive
                },
                new VRCAvatarDescriptor.CustomAnimLayer
                {
                    animatorController = emoteWizardRoot.GetComponent<GestureWizard>()?.outputAsset,
                    isDefault = false,
                    isEnabled = false,
                    mask = VrcSdkAssetLocator.HandsOnly(),
                    type = VRCAvatarDescriptor.AnimLayerType.Gesture
                },
                new VRCAvatarDescriptor.CustomAnimLayer
                {
                    animatorController = null,
                    isDefault = true,
                    isEnabled = false,
                    mask = null,
                    type = VRCAvatarDescriptor.AnimLayerType.Action
                },
                new VRCAvatarDescriptor.CustomAnimLayer
                {
                    animatorController = emoteWizardRoot.GetComponent<FxWizard>()?.outputAsset,
                    isDefault = false,
                    isEnabled = false,
                    mask = null,
                    type = VRCAvatarDescriptor.AnimLayerType.FX
                }
            };
            avatarDescriptor.customExpressions = true;
            avatarDescriptor.expressionsMenu = emoteWizardRoot.GetComponent<ExpressionWizard>()?.outputAsset;
            avatarDescriptor.expressionParameters = emoteWizardRoot.GetComponent<ParametersWizard>()?.outputAsset;
        }

        void EnsureComponent<T>()
            where T : Component
        {
            var go = emoteWizardRoot.gameObject;
            if (!go.GetComponent<T>()) go.AddComponent<T>();
        }
    }
}