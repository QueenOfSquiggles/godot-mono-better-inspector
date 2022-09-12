using System;
using betterinspector.inspectors;
using betterinspector.inspectors.integrated;
using Godot;

namespace betterinspector.attributes
{


    [AttributeUsage(AttributeTargets.Field)]
    // Does nothing but act as a tag to force the custom inspector to be used.
    public class UseIntegrated : ExportVariableAttribute
    {
        public override void Apply(IBetterPropertyEditor control){}
    }

}