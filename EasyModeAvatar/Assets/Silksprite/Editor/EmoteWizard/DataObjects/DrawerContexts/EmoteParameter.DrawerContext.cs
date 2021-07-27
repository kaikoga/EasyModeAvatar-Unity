using JetBrains.Annotations;
using Silksprite.EmoteWizard.Base.DrawerContexts;

namespace Silksprite.EmoteWizard.DataObjects.DrawerContexts
{
    [UsedImplicitly]
    public class EmoteParameterDrawerContext : EmoteWizardDrawerContextBase<EmoteParameter, EmoteParameterDrawerContext>
    {
        public readonly ParametersWizard ParametersWizard;
        public readonly bool IsEditing;

        public EmoteParameterDrawerContext() : base(null) { }
        public EmoteParameterDrawerContext(EmoteWizardRoot emoteWizardRoot, ParametersWizard parametersWizard, bool isEditing) : base(emoteWizardRoot)
        {
            ParametersWizard = parametersWizard;
            IsEditing = isEditing;
        }
    }
}