using System.Diagnostics;
using betterinspector.attributes;
using Godot;

namespace betterinspector.inspectors.custom
{
    public abstract class CustomInspectorBase : HBoxContainer, IBetterPropertyEditor
    {

        protected Object gdObj;
        protected object csObj;
        protected string propertyName;

        protected Texture iconReset = null;

        public CustomInspectorBase(Object gdObj, object csObj, string handledProperty)
        {
            this.gdObj = gdObj;
            this.csObj = csObj;
            propertyName = handledProperty;
            if (Plugin.instance != null) Theme = Plugin.instance.customInspectorTheme;
            MouseFilter = MouseFilterEnum.Stop;
        }

        public override void _Ready()
        {
            iconReset = Plugin.GetIcon("Reload");

            Rebuild(false);
            ParseCsharp();
        }

        protected object GetCurrentValue()
        {
            return gdObj.Get(propertyName);
        }

        protected virtual void SaveNewValue(object value){
            gdObj.Set(propertyName, value);
            GD.Print($"{gdObj.Get("name")}:{propertyName} was set to {value}");
        }

        protected abstract void Rebuild(bool vertical);

        protected void ParseCsharp()
        {
            if (csObj == null) GD.PushError("csObj ref was found null!!!!");
            System.Reflection.FieldInfo field = csObj.GetType().GetField(propertyName);
            if (field != null)
            {
                object[] attribs = field.GetCustomAttributes(true);
                foreach (object a in attribs)
                {
                    (a as ExportVariableAttribute)?.Apply(this);
                }
            }
        }

/*
*/

        protected virtual Button CreateResetButton()
        {
            var btn = new Button();
            btn.Connect("pressed", this, "ResetValue");
            btn.Icon = iconReset;
            return btn;
        }

        public virtual void ResetValue()
        {
            var baseVal = (csObj as Node).Get(propertyName);
            SaveNewValue(baseVal);
        }

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

        public abstract bool IsValueDefault();


        public abstract Range GetRangeElement();
        public abstract void SetEditorCheckable(bool isCheckable);
        public abstract void SetEditorDrawRed(bool isDrawRed);
        public abstract void SetEditorKeying(bool isKeying);
        public abstract void SetEditorReadOnly(bool isReadOnly);
        public abstract void SetLabelText(string label);

        public virtual void SetEditorTooltipText(string tooltip)
        {
            this.HintTooltip = tooltip;
        }
        
        public virtual void SetUseBottomEditor()
        {
            //Rebuild(true);
            //ParseCsharp();
        }

        public abstract void SetLabelColour(Color colour);
    }
}