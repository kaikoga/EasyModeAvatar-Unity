using Silksprite.EmoteWizard.Base;
using UnityEngine;

namespace Silksprite.EmoteWizard
{
    [DisallowMultipleComponent]
    public class GestureWizard : AnimationWizardBase
    {
        [SerializeField] public AvatarMask defaultAvatarMask;

        [SerializeField] public Motion globalClip;
        [SerializeField] public Motion ambienceClip;

        public override string LayerName => "Gesture";
    }
}