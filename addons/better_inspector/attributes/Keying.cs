using System;
using betterinspector.inspectors;
using Godot;

namespace betterinspector.attributes
{


    [AttributeUsage(AttributeTargets.Field)]
    public class Keying : ExportVariableAttribute
    {
        public readonly bool isKeying;

        public Keying(bool isKeying = true)
        {
            this.isKeying = isKeying;
        }

        public override void Apply(BaseInspector control)
        {
            control.Keying = isKeying;
        }
    }

}