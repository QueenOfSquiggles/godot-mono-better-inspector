using System;
using betterinspector.inspectors;
using betterinspector.inspectors.integrated;
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

        public override void Apply(IBetterPropertyEditor control)
        {
            control.SetEditorKeying(isKeying);
        }
    }

}