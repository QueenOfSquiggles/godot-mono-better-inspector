using System;
using betterinspector.inspectors;
using Godot;

namespace betterinspector.attributes
{


    [AttributeUsage(AttributeTargets.Field)]
    // Does nothing but act as a tag to force the custom inspector to be used.
    public class ForceUseBetterInspector : ExportVariableAttribute
    {
        public override void Apply(BaseInspector control){}
    }

}