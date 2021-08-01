using JetBrains.Annotations;
using Silksprite.EmoteWizard.Base.DrawerContexts;
using Silksprite.EmoteWizard.DataObjects.DrawerStates;

namespace Silksprite.EmoteWizard.DataObjects.DrawerContexts
{
    [UsedImplicitly]
    public class EmoteDrawerContext : EmoteWizardDrawerContextBase<Emote, EmoteDrawerContext>
    {
        public readonly ParametersWizard ParametersWizard;
        public readonly bool AdvancedAnimations;
        public readonly EmoteDrawerState State;

        public EmoteDrawerContext() : base(null) { }
        public EmoteDrawerContext(EmoteWizardRoot emoteWizardRoot, ParametersWizard parametersWizard, bool advancedAnimations, EmoteDrawerState state) : base(emoteWizardRoot)
        {
            ParametersWizard = parametersWizard;
            AdvancedAnimations = advancedAnimations;
            State = state;
        }

        public EmoteConditionDrawerContext EmoteConditionDrawerContext()
        {
            return new EmoteConditionDrawerContext(EmoteWizardRoot, ParametersWizard);
        }
        
        public EmoteControlDrawerContext EmoteParameterDrawerContext()
        {
            return new EmoteControlDrawerContext(EmoteWizardRoot, ParametersWizard, true, State.EditControls);
        }
    }
}