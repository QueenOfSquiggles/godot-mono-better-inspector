using System;
using betterinspector.inspectors;
using Godot;

namespace betterinspector.attributes
{


    [AttributeUsage(AttributeTargets.Field)]
    public class Checkable : ExportVariableAttribute
    {
        public readonly bool isCheckable;

        public Checkable(bool isCheckable = true)
        {
            this.isCheckable = isCheckable;
        }

        public override void Apply(BaseInspector control)
        {
            control.Checkable = isCheckable;
        }
    }

}