using System;
using System.Collections.Generic;
using System.Linq;
using EmoteWizard.Base;
using EmoteWizard.DataObjects;
using EmoteWizard.DataObjects.Internal;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace EmoteWizard
{
    [DisallowMultipleComponent]
    public class ParametersWizard : EmoteWizardBase
    {
        [SerializeField] public VRCExpressionParameters outputAsset;
        [SerializeField] public bool vrcDefaultParameters = true;
        [SerializeField] public List<ParameterItem> parameterItems;

        public IEnumerable<ParameterItem> CustomParameterItems => parameterItems.Where(parameter => !parameter.defaultParameter);

        public void TryRefreshParameters()
        {
            var expressionWizard = GetComponent<ExpressionWizard>();
            if (expressionWizard == null)
            {
                Debug.LogWarning("ExpressionWizard not found. Parameters are unchanged.");
                return;
            }
            DoRefreshParameters(expressionWizard);
        }

        public void ForceRefreshParameters()
        {
            var expressionWizard = GetComponent<ExpressionWizard>();
            if (expressionWizard == null)
            {
                throw new Exception("ExpressionWizard not found. Parameters are unchanged.");
            }
            DoRefreshParameters(expressionWizard);
        }

        void DoRefreshParameters(ExpressionWizard expressionWizard)
        {
            var customOnly = !vrcDefaultParameters;
            var vrcDefaultParametersStub = ParameterItem.VrcDefaultParameters;

            var builder = new ExpressionParameterBuilder();

            builder.Import(vrcDefaultParametersStub); // create VRC default parameters entry

            if (parameterItems != null) builder.Import(parameterItems);

            foreach (var expressionItem in expressionWizard.expressionItems)
            {
                if (!string.IsNullOrEmpty(expressionItem.parameter))
                {
                    builder.FindOrCreate(expressionItem.parameter).AddUsage(expressionItem.value);
                }
                if (!expressionItem.IsPuppet) continue;
                foreach (var subParameter in expressionItem.subParameters.Where(subParameter => !string.IsNullOrEmpty(subParameter)))
                {
                    builder.FindOrCreate(subParameter).AddPuppetUsage();
                }
            }

            builder.Import(vrcDefaultParametersStub); // override VRC default parameters with default values

            parameterItems = builder.ParameterItems.Where(parameter => !customOnly || !parameter.defaultParameter).ToList();
        }

        public VRCExpressionParameters.Parameter[] ToParameters()
        {
            TryRefreshParameters(); 
            return parameterItems.Select(parameter => parameter.ToParameter()).ToArray();
        }
    }
}