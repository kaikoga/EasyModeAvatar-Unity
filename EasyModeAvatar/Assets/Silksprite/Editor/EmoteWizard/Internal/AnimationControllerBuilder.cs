using System;
using System.Collections.Generic;
using System.Linq;
using Silksprite.EmoteWizard.Base;
using Silksprite.EmoteWizard.DataObjects;
using Silksprite.EmoteWizard.Extensions;
using Silksprite.EmoteWizard.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Silksprite.EmoteWizard.Internal
{
    public class AnimationControllerBuilder
    {
        public AnimationWizardBase AnimationWizardBase;
        public ParametersWizard ParametersWizard;
        public string DefaultRelativePath;
        
        AnimatorController _animatorController;
        AnimatorController AnimatorController
        {
            get
            {
                if (_animatorController) return _animatorController;
                return _animatorController = AnimationWizardBase.ReplaceOrCreateOutputAsset(ref AnimationWizardBase.outputAsset, DefaultRelativePath);
            }
        }

        bool AssertParameterExists(string parameterName)
        {
            return ParametersWizard == null || ParametersWizard.AssertParameterExists(parameterName);
        }

        public AnimatorControllerLayer PopulateLayer(string layerName, AvatarMask avatarMask = null)
        {
            layerName = AnimatorController.MakeUniqueLayerName(layerName);
            var layer = new AnimatorControllerLayer
            {
                name = layerName,
                defaultWeight = 1.0f,
                avatarMask = avatarMask,
                stateMachine = new AnimatorStateMachine
                {
                    name = layerName,
                    hideFlags = HideFlags.HideInHierarchy,
                    anyStatePosition = new Vector3(0, 0, 0),
                    entryPosition = new Vector3(0, 100, 0),
                    exitPosition = new Vector3(0, 200, 0)
                }
            };

            AssetDatabase.AddObjectToAsset(layer.stateMachine, AssetDatabase.GetAssetPath(AnimatorController));
            AnimatorController.AddLayer(layer);
            return layer;
        }

        static IEnumerable<Vector3> Positions()
        {
            var position = new Vector3(300, 0, 0);
            for (;;)
            {
                yield return position;
                position.y += 60;
            }
        }

        public void BuildStaticStateMachine(AnimatorStateMachine stateMachine, string stateName, Motion clip)
        {
            using (var positions = Positions().GetEnumerator())
            {
                positions.MoveNext();
                var state = stateMachine.AddState(stateName, positions.Current);
                state.motion = clip;
                state.writeDefaultValues = false;
                stateMachine.defaultState = state;
            }
        }

        public void BuildGestureStateMachine(AnimatorStateMachine stateMachine, bool isLeft, bool isAdvanced)
        {
            var emotes = AnimationWizardBase.emotes;

            using (var positions = Positions().GetEnumerator())
            {
                foreach (var emote in emotes)
                {
                    positions.MoveNext();
                    var gesture1 = emote.gesture1;
                    var gesture2 = emote.gesture2;
                    
                    var clip = isLeft || !isAdvanced ? emote.clipLeft : emote.clipRight;
                    var state = stateMachine.AddState(emote.ToStateName(), positions.Current);
                    state.motion = clip ? clip : AnimationWizardBase.EmoteWizardRoot.ProvideEmptyClip();
                    state.writeDefaultValues = false;
                    if (clip != null && emote.parameter != null && emote.parameter.normalizedTimeEnabled)
                    {
                        var timeParameter = isLeft ? emote.parameter.normalizedTimeLeft : emote.parameter.normalizedTimeRight;
                        if (AssertParameterExists(timeParameter))
                        {
                            state.timeParameterActive = true;
                            state.timeParameter = timeParameter;
                            clip.SetLoopTimeRec(false);
                            EditorUtility.SetDirty(clip);
                        }
                    }

                    var transition = stateMachine.AddAnyStateTransition(state);
                    transition.AddCondition(gesture1.ResolveMode(), gesture1.ResolveThreshold(), gesture1.ResolveParameter(isLeft));
                    if (gesture2.mode != GestureConditionMode.Ignore)
                    {
                        transition.AddCondition(gesture2.ResolveMode(), gesture2.ResolveThreshold(), gesture2.ResolveParameter(isLeft));
                    }

                    var validConditions = emote.conditions
                        .Where(condition => AssertParameterExists(condition.parameter));
                    foreach (var condition in validConditions)
                    {
                        transition.AddCondition(condition.AnimatorConditionMode, condition.threshold, condition.parameter);
                    }

                    transition.hasExitTime = false;
                    transition.duration = 0.1f;
                    transition.canTransitionToSelf = false;
                }
            }
            
            stateMachine.defaultState = stateMachine.states.FirstOrDefault().state;
        }

        public void BuildParameterStateMachine(AnimatorStateMachine stateMachine, ParameterEmote parameterEmote)
        {
            if (!AssertParameterExists(parameterEmote.parameter)) return;
            switch (parameterEmote.emoteKind)
            {
                case ParameterEmoteKind.Transition:
                    BuildTransitionStateMachine(stateMachine, parameterEmote);
                    break;
                case ParameterEmoteKind.NormalizedTime:
                    BuildNormalizedTimeStateMachine(stateMachine, parameterEmote);
                    break;
                case ParameterEmoteKind.BlendTree:
                    BuildBlendTreeStateMachine(stateMachine, parameterEmote);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void BuildTransitionStateMachine(AnimatorStateMachine stateMachine, ParameterEmote parameterEmote)
        {
            void AddTransition(AnimatorState state, string parameterName, float value, float? nextValue)
            {
                AnimatorStateTransition transition;
                switch (parameterEmote.valueKind)
                {
                    case ParameterValueKind.Int:
                        transition = stateMachine.AddAnyStateTransition(state);
                        transition.AddCondition(AnimatorConditionMode.Equals, value, parameterName);
                        break;
                    case ParameterValueKind.Float:
                        transition = stateMachine.AddAnyStateTransition(state);
                        if (nextValue is float nextVal)
                        {
                            transition.AddCondition(AnimatorConditionMode.Less, nextVal, parameterName);
                        }
                        else
                        {
                            transition.AddCondition(AnimatorConditionMode.Greater, value - 1f, parameterName);
                        }
                        return;
                    case ParameterValueKind.Bool:
                        transition = stateMachine.AddAnyStateTransition(state);
                        transition.AddCondition(value != 0 ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, value, parameterName);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                transition.hasExitTime = false;
                transition.duration = 0.1f;
                transition.canTransitionToSelf = false;
            }

            using (var positions = Positions().GetEnumerator())
            {
                var validStates = parameterEmote.states.Where(state => state.enabled).ToList();
                var stateAndNextValue = validStates.Zip(
                    validStates.Skip(1).Select(state => (float?) state.value).Concat(Enumerable.Repeat((float?) null, 1)),
                    (s, v) => (s, v));
                foreach (var (parameterEmoteState, nextValue) in stateAndNextValue)
                {
                    positions.MoveNext();
                    var stateName = $"{parameterEmote.parameter} = {parameterEmoteState.value}";
                    var state = stateMachine.AddState(stateName, positions.Current);
                    state.motion = parameterEmoteState.clip;
                    state.writeDefaultValues = false;
                    AddTransition(state, parameterEmote.parameter, parameterEmoteState.value, nextValue);
                }
            }
            stateMachine.defaultState = stateMachine.states.FirstOrDefault().state;
        }

        void BuildNormalizedTimeStateMachine(AnimatorStateMachine stateMachine, ParameterEmote parameterEmote)
        {
            var clip = parameterEmote.states
                .Where(state => state.enabled)
                .Select(state => state.clip)
                .FirstOrDefault(c => c != null);
            if (clip == null) return;

            using (var positions = Positions().GetEnumerator())
            {
                positions.MoveNext();
                var state = stateMachine.AddState(parameterEmote.name, positions.Current);
                state.motion = clip;
                state.writeDefaultValues = false;

                state.timeParameterActive = true;
                state.timeParameter = parameterEmote.parameter;
                clip.SetLoopTimeRec(false);
                EditorUtility.SetDirty(clip);
            }

            stateMachine.defaultState = stateMachine.states.FirstOrDefault().state;
        }

        void BuildBlendTreeStateMachine(AnimatorStateMachine stateMachine, ParameterEmote parameterEmote)
        {
            var path = GeneratedAssetLocator.ParameterEmoteBlendTreePath(AnimationWizardBase.LayerName, parameterEmote.name);
            var blendTree = AnimationWizardBase.EmoteWizardRoot.EnsureAsset<BlendTree>(path);

            blendTree.blendParameter = parameterEmote.parameter;
            blendTree.blendType = BlendTreeType.Simple1D;
            blendTree.useAutomaticThresholds = false;
            var validStates = parameterEmote.states.Where(state => state.enabled);
            blendTree.children = validStates.Select(state => new ChildMotion
            {
                cycleOffset = 0,
                directBlendParameter = null,
                mirror = false,
                motion = state.clip,
                position = default,
                threshold = state.value,
                timeScale = 1
            }).ToArray();

            using (var positions = Positions().GetEnumerator())
            {
                positions.MoveNext();
                var state = stateMachine.AddState(parameterEmote.name, positions.Current);
                state.motion = blendTree;
                state.writeDefaultValues = false;
            }

            stateMachine.defaultState = stateMachine.states.FirstOrDefault().state;
        }

        public void BuildMixinLayerStateMachine(AnimatorStateMachine stateMachine, AnimationMixin mixin)
        {
            using (var positions = Positions().GetEnumerator())
            {
                positions.MoveNext();
                var state = stateMachine.AddState(mixin.name, positions.Current);
                state.motion = mixin.Motion;
                state.writeDefaultValues = false;

                if (mixin.kind == AnimationMixinKind.AnimationClip && mixin.normalizedTimeEnabled)
                {
                    state.timeParameterActive = true;
                    state.timeParameter = mixin.normalizedTime;
                    mixin.Motion.SetLoopTimeRec(false);
                    EditorUtility.SetDirty(mixin.Motion);
                }
            }
        }
        
        public void BuildParameters()
        {
            foreach (var parameter in (IEnumerable<ParameterItem>) ParametersWizard.parameterItems)
            {
                var parameterName = parameter.name;
                AnimatorControllerParameterType parameterType;
                switch (parameter.ValueKind)
                {
                    case ParameterValueKind.Int:
                        parameterType = AnimatorControllerParameterType.Int;
                        break;
                    case ParameterValueKind.Float:
                        parameterType = AnimatorControllerParameterType.Float;
                        break;
                    case ParameterValueKind.Bool:
                        parameterType = AnimatorControllerParameterType.Bool;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                AnimatorController.AddParameter(parameterName, parameterType);
            }
        }
    }
}