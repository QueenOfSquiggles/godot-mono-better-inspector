using System;
using betterinspector.inspectors;
using betterinspector.inspectors.integrated;
using Godot;

namespace betterinspector.attributes
{


    [AttributeUsage(AttributeTargets.Field)]
    public class Tooltip : ExportVariableAttribute
    {
        public readonly string tooltip = "";

        public Tooltip(string tooltip = "No Tooltip Provided")
        {
            this.tooltip = tooltip;
        }
        public Tooltip(params string[] tooltipLines)
        {
            if (tooltipLines.Length == 0){
                tooltip = "A fatal error has ocurred"; // This should literally never be called
                // if you are debugging this edge case...how in the hell did it get here???
                return;
            }

            this.tooltip = tooltipLines[0];
            for(int i = 1; i < tooltipLines.Length; i++)
            {
                tooltip += " \n" + tooltipLines[i];
            }
        }

        public override void Apply(IBetterPropertyEditor control)
        {
            control.SetEditorTooltipText(tooltip);
        }
    }

}