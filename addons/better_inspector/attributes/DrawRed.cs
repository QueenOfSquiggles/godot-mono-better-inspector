using System;
using betterinspector.inspectors;
using Godot;

namespace betterinspector.attributes
{


    [AttributeUsage(AttributeTargets.Field)]
    public class DrawRed : ExportVariableAttribute
    {
        public readonly bool isDrawRed;

        public DrawRed(bool isDrawRed = true)
        {
            this.isDrawRed = isDrawRed;
        }

        public override void Apply(BaseInspector control)
        {
            control.DrawRed = isDrawRed;
        }
    }

}