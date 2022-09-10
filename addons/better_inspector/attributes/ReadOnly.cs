using System;
using betterinspector.inspectors;
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

        public override void Apply(BaseInspector control)
        {
            control.ReadOnly = isReadOnly;
        }
    }

}