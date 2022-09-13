using System;
using betterinspector.inspectors;
using betterinspector.inspectors.integrated;
using Godot;

namespace betterinspector.attributes
{


    [AttributeUsage(AttributeTargets.Field)]
    public class LabelColour : ExportVariableAttribute
    {
        private Color colour = new Color();

        public LabelColour(string hexCode)
        {
            colour = new Color(hexCode);
        }

        public LabelColour(Color colour)
        {
            this.colour = colour;
        }
        

        public override void Apply(IBetterPropertyEditor control)
        {
            control.SetLabelColour(colour);
        }
    }

}