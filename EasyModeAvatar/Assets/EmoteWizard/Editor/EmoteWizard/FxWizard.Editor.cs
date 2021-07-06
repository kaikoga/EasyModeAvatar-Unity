using System.Linq;
using EmoteWizard.Base;
using EmoteWizard.Collections;
using EmoteWizard.DataObjects;
using EmoteWizard.Extensions;
using EmoteWizard.UI;
using EmoteWizard.Utils;
using UnityEditor;
using UnityEngine;

namespace EmoteWizard
{
    [CustomEditor(typeof(FxWizard))]
    public class FxWizardEditor : AnimationWizardBaseEditor
    {
        FxWizard fxWizard;

        ExpandableReorderableList emotesList;
        ExpandableReorderableList parametersList;
        ExpandableReorderableList mixinsList;

        void OnEnable()
        {
            fxWizard = target as FxWizard;

            emotesList = new ExpandableReorderableList(serializedObject,
                serializedObject.FindProperty("emotes"),
                "Emotes",
                new EmoteListDrawerBase(),
                PagerNameGeneratorUtils.AsEmoteName);
            parametersList = new ExpandableReorderableList(serializedObject,
                serializedObject.FindProperty("parameters"),
                "Parameters",
                new ParameterEmoteListDrawerBase(),
                (property, index) => property.FindPropertyRelative("name").stringValue);
            mixinsList = new ExpandableReorderableList(serializedObject,
                serializedObject.FindProperty("mixins"),
                "Mixins",
                new AnimationMixinListDrawerBase(),
                (property, index) => property.FindPropertyRelative("name").stringValue);
        }

        public override void OnInspectorGUI()
        {
            var serializedObj = serializedObject;
            var emoteWizardRoot = fxWizard.EmoteWizardRoot;
            var parametersWizard = fxWizard.GetComponent<ParametersWizard>();

            EmoteWizardGUILayout.SetupOnlyUI(fxWizard, () =>
            {
                if (GUILayout.Button("Repopulate Emotes: 7 items"))
                {
                    RepopulateDefaultFxEmotes();
                }
                if (GUILayout.Button("Repopulate Emotes: 14 items"))
                {
                    RepopulateDefaultFxEmotes14();
                }
                if (parametersWizard != null)
                {
                    if (GUILayout.Button("Repopulate Parameters"))
                    {
                        parametersWizard.TryRefreshParameters();
                        fxWizard.parameters.Clear();
                        fxWizard.RefreshParameters(parametersWizard != null ? parametersWizard.parameterItems : null);
                    }
                }
            });

            var advancedAnimations = serializedObj.FindProperty("advancedAnimations");
            EditorGUILayout.PropertyField(advancedAnimations);

            EmoteWizardGUILayout.PropertyFieldWithGenerate(serializedObj.FindProperty("globalClip"), () => emoteWizardRoot.EnsureAsset<AnimationClip>("FX/@@@Generated@@@GlobalFX.anim"));
            EmoteWizardGUILayout.PropertyFieldWithGenerate(serializedObj.FindProperty("ambienceClip"), () => emoteWizardRoot.EnsureAsset<AnimationClip>("FX/@@@Generated@@@AmbienceFX.anim"));

            using (EmoteDrawer.StartContext(emoteWizardRoot, advancedAnimations.boolValue))
            {
                emotesList.DrawAsProperty(emoteWizardRoot.listDisplayMode);
            }

            EmoteWizardGUILayout.RequireAnotherWizard(fxWizard, parametersWizard, () =>
            {
                if (GUILayout.Button("Collect Parameters"))
                {
                    parametersWizard.TryRefreshParameters();
                    fxWizard.RefreshParameters(parametersWizard != null ? parametersWizard.parameterItems : null);
                }
            });
            using (ParameterEmoteDrawer.StartContext(emoteWizardRoot, fxWizard, fxWizard.LayerName, ParameterEmoteDrawer.EditTargets))
            {
                parametersList.DrawAsProperty(emoteWizardRoot.listDisplayMode);
            }

            using (AnimationMixinDrawer.StartContext(emoteWizardRoot, GeneratedAssetLocator.MixinDirectoryPath(fxWizard.LayerName)))
            {
                mixinsList.DrawAsProperty(emoteWizardRoot.listDisplayMode);
            }

            EmoteWizardGUILayout.OutputUIArea(() =>
            {
                if (GUILayout.Button("Generate Animation Controller"))
                {
                    BuildAnimatorController("FX/@@@Generated@@@FX.controller", animatorController =>
                    {
                        var resetClip = fxWizard.ProvideResetClip();
                        BuildResetClip(resetClip);
                        
                        var resetLayer = PopulateLayer(animatorController, "Reset");
                        BuildStaticStateMachine(resetLayer.stateMachine, "Reset", resetClip);

                        var allPartsLayer = PopulateLayer(animatorController, "AllParts");
                        BuildStaticStateMachine(allPartsLayer.stateMachine, "Global", fxWizard.globalClip);

                        var ambienceLayer = PopulateLayer(animatorController, "Ambience");
                        BuildStaticStateMachine(ambienceLayer.stateMachine, "Global", fxWizard.ambienceClip);

                        var leftHandLayer = PopulateLayer(animatorController, "Left Hand", VrcSdkAssetLocator.HandLeft()); 
                        BuildGestureStateMachine(leftHandLayer.stateMachine, true, advancedAnimations.boolValue);
                
                        var rightHandLayer = PopulateLayer(animatorController, "Right Hand", VrcSdkAssetLocator.HandRight()); 
                        BuildGestureStateMachine(rightHandLayer.stateMachine, false, advancedAnimations.boolValue);

                        foreach (var parameterEmote in fxWizard.ActiveParameters)
                        {
                            var expressionLayer = PopulateLayer(animatorController, parameterEmote.name);
                            BuildParameterStateMachine(expressionLayer.stateMachine, parameterEmote);
                        }
                        
                        foreach (var mixin in fxWizard.mixins)
                        {
                            var mixinLayer = PopulateLayer(animatorController, mixin.name); 
                            BuildMixinLayerStateMachine(mixinLayer.stateMachine, mixin);
                        }
                        
                        BuildParameters(animatorController, parametersWizard.CustomParameterItems);
                    });
                }

                EditorGUILayout.PropertyField(serializedObj.FindProperty("outputAsset"));
                EditorGUILayout.PropertyField(serializedObj.FindProperty("resetClip"));
            });

            serializedObj.ApplyModifiedProperties();
        }

        void RepopulateDefaultFxEmotes()
        {
            var newEmotes = Emote.HandSigns
                .Select(Emote.Populate)
                .ToList();
            fxWizard.emotes = newEmotes;
        }

        void RepopulateDefaultFxEmotes14()
        {
            var newEmotes = Enumerable.Empty<Emote>()
                .Concat(Emote.HandSigns
                    .Select(handSign => new Emote
                    {
                        gesture1 = EmoteGestureCondition.Populate(handSign, GestureParameter.Gesture),
                        gesture2 = EmoteGestureCondition.Populate(handSign, GestureParameter.GestureOther),
                        parameter = EmoteParameter.Populate(handSign)
                    }))
                .Concat(Emote.HandSigns
                    .Select(handSign => new Emote
                    {
                        gesture1 = EmoteGestureCondition.Populate(handSign, GestureParameter.Gesture),
                        gesture2 = EmoteGestureCondition.Populate(handSign, GestureParameter.GestureOther, GestureConditionMode.NotEqual),
                        parameter = EmoteParameter.Populate(handSign)
                    }))
                .ToList();
            fxWizard.emotes = newEmotes;
        }
    }
}