using System;
using betterinspector.inspectors;
using Godot;

namespace betterinspector.attributes
{


    [AttributeUsage(AttributeTargets.Field)]
    public class CustomLabel : ExportVariableAttribute
    {
        public readonly string label = "";

        public CustomLabel(string label = "")
        {
            this.label = label;
        }

        public override void Apply(BaseInspector control)
        {
            if (label != null && !label.Empty()) control.Label = label;
        }
    }

}