using System;
using betterinspector.inspectors;
using betterinspector.inspectors.integrated;
using Godot;

namespace betterinspector.attributes
{


    [AttributeUsage(AttributeTargets.Field)]
    public class EditorReadOnly : ExportVariableAttribute
    {
        public readonly bool isReadOnly;

        public EditorReadOnly(bool isReadOnly = true)
        {
            this.isReadOnly = isReadOnly;
        }

        public override void Apply(IBetterPropertyEditor control)
        {
            control.SetEditorReadOnly(isReadOnly);
        }
    }

}