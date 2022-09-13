using System;
using betterinspector.inspectors;
using betterinspector.inspectors.integrated;
using Godot;

namespace betterinspector.attributes
{


    [AttributeUsage(AttributeTargets.Field)]
    public class Category : ExportVariableAttribute
    {
        public string name = "";
        public Color fontColor = new Color();
        public string iconName = "Collapse";

        public Category(string name, string fontColor = "#FFFFFF", string iconName = "Collapse")
        {
            this.name = name;
            this.fontColor = new Color(fontColor);
            this.iconName = iconName;
        }

        public override void Apply(IBetterPropertyEditor control){}
    }

}