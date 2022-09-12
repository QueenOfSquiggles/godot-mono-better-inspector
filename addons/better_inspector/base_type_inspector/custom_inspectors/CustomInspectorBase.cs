using betterinspector.attributes;
using Godot;

namespace betterinspector.inspectors.custom
{
    public abstract class CustomInspectorBase : HBoxContainer, IBetterPropertyEditor
    {

        protected Object gdObj;
        protected object csObj;
        protected string propertyName;

        protected Label propertyNameLabel = null;

        public CustomInspectorBase(Object gdObj, object csObj, string handledProperty)
        {
            this.gdObj = gdObj;
            this.csObj = csObj;
            propertyName = handledProperty;
            if (Plugin.instance != null) Theme = Plugin.instance.customInspectorTheme;
        }

        public override void _Ready()
        {
            Rebuild();
            var panelBox = new StyleBoxFlat();
            panelBox.BgColor = new Color(0f,0f,0f);
            panelBox.SetBorderWidthAll(2);
            panelBox.BorderColor = new Color(1f, 0f, 1f);
            Theme.SetStylebox("panel", "TooltipPanel", panelBox);
        }

        protected object GetCurrentValue()
        {
            return gdObj.Get(propertyName);
        }

        protected virtual void SaveNewValue(){

        }

        protected virtual void Rebuild(){
            propertyNameLabel = new Label();
            propertyNameLabel.Text = propertyName;
            AddChild(propertyNameLabel);
            ParseCsharp();
        }

        private void ParseCsharp()
        {
            System.Reflection.FieldInfo field = csObj.GetType().GetField(propertyName);
            if (field != null)
            {
                object[] attribs = field.GetCustomAttributes(false);
                foreach (object a in attribs)
                {
                    (a as ExportVariableAttribute)?.Apply(this);
                }
            }
        }

/*
*/
        public override Control _MakeCustomTooltip(string forText)
        {
            var nameSpace = csObj.GetType().Namespace;
            var headerText = $"[center][b]{(nameSpace.Empty()? "__" : nameSpace)}:{gdObj.Get("name")}:{propertyName}[/b][/center]";
            var label = new RichTextLabel();
            label.BbcodeEnabled = true;
            label.BbcodeText = headerText + "\n" + forText;
            label.RectMinSize = new Vector2(450, 100);
            label.SizeFlagsVertical = (int)SizeFlags.ExpandFill;
            label.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            label.FitContentHeight = true;
            if (Plugin.instance != null) label.Theme = Plugin.instance.customInspectorTheme;
            return label;
        }


        public abstract Range GetRangeElement();
        public abstract void SetEditorCheckable(bool isCheckable);
        public abstract void SetEditorDrawRed(bool isDrawRed);
        public abstract void SetEditorKeying(bool isKeying);
        public abstract void SetEditorReadOnly(bool isReadOnly);

        public virtual void SetEditorTooltipText(string tooltip)
        {
            this.HintTooltip = tooltip;
        }
        
        public virtual void SetLabelText(string label)
        {
            propertyNameLabel.Text = label;
        }
        public abstract void SetUseBottomEditor();
    }
}