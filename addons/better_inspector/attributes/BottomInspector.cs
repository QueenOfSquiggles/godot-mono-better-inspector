using System;
using betterinspector.inspectors;
using Godot;

namespace betterinspector.attributes
{


    [AttributeUsage(AttributeTargets.Field)]
    public class BottomInspector : ExportVariableAttribute
    {
        public override void Apply(BaseInspector control)
        {
            // TODO this isn't the safest option, but should work 99% of the time.
            control.SetBottomEditor(control.GetChild(0) as Control);
        }
    }

}