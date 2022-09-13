using System;
using betterinspector.inspectors;
using betterinspector.inspectors.integrated;
using Godot;

namespace betterinspector.attributes
{


    [AttributeUsage(AttributeTargets.Field)]
    public class BottomInspector : ExportVariableAttribute
    {
        public override void Apply(IBetterPropertyEditor control)
        {
            control.SetUseBottomEditor();
        }
    }

}