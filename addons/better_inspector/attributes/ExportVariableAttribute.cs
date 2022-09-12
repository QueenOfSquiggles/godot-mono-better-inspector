using System;
using betterinspector.inspectors;
using Godot;

namespace betterinspector.attributes
{
    /*
    The parent of all base inspector properties
    Ensures that all attributes of this type have an apply function for easy processing.
    */
    public abstract class ExportVariableAttribute : Attribute
    {
        public abstract void Apply(IBetterPropertyEditor editor);
    }
}