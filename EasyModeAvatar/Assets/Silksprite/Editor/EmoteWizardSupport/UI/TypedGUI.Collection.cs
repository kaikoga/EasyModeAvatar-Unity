using System.Collections.Generic;
using Silksprite.EmoteWizardSupport.Collections.Generic;
using Silksprite.EmoteWizardSupport.Extensions;
using Silksprite.EmoteWizardSupport.Utils;
using UnityEngine;

namespace Silksprite.EmoteWizardSupport.UI
{
    public static partial class TypedGUI
    {
        public static List<T> ListField<T>(Rect position, string label, ref List<T> value, ITypedDrawer<T> drawer)
        where T : new()
        {
            if (!Foldout(position.UISliceV(0), value, label)) return value;

            var arraySize = value.Count;
            DelayedIntField(position.UISliceV(1), "Size", ref arraySize);
            ListUtils.ResizeAndPopulate(ref value, arraySize, v => new T());

            var y = 2;
            foreach (var item in value)
            {
                drawer.OnGUI(position.UISliceV(y++), item, new GUIContent(" "));
            }

            return value;
        }
    }
}