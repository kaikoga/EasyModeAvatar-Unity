using Silksprite.EmoteWizard.Extensions;
using Silksprite.EmoteWizard.Internal.ConditionBuilders;
using Silksprite.EmoteWizard.Internal.LayerBuilders.Base;
using UnityEditor.Animations;
using UnityEngine;

namespace Silksprite.EmoteWizard.Internal.LayerBuilders
{
    public class StaticLayerBuilder : LayerBuilderBase
    {
        readonly string _stateName;
        readonly Motion _clip;

        public StaticLayerBuilder(AnimationControllerBuilder builder, AnimatorControllerLayer layer, string stateName, Motion clip) : base(builder, layer)
        {
            _stateName = stateName;
            _clip = clip;
        }

        protected override void Process()
        {
            var state = AddStateWithoutTransition(_stateName, _clip);
            var condition = new ConditionBuilder().AlwaysTrue();
            AddAnyStateTransition(state, condition);
        }
    }
}