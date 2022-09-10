#if TOOLS
#pragma warning disable IDE0017 
// removes the "SiMpLiFy YoUr InItIaLiZaTiOn!!!! D:<" warning, which isn't even in this C# version

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using betterinspector.attributes;
using Godot;

namespace betterinspector.inspectors
{
    [Tool]
    public class BaseTypesInspector : EditorInspectorPlugin
    {
        public override bool CanHandle(Object @object)
        {
            return true; // handle all objects, we are parsing base types
        }

        public override void ParseBegin(Object @object)
        {

            var scriptRef = @object.GetScript();
            if (scriptRef != null && scriptRef is CSharpScript){
                var obj = (scriptRef as CSharpScript).New();
                // we are inspecting some type (Node or Resource) that has a CSharp script, we can get some fun information with this!
            }
        }

        public override bool ParseProperty(Object @object, int type, string path, int hint, string hintText, int usage)
        {
            if (!BaseInspector.HasInspectorAttributes(@object, path)) return false;
            switch(type)
            {
                case (int)Variant.Type.Int:
                    AddPropertyEditor(path,  new BaseInspectorInteger());
                    return true;
                case (int)Variant.Type.Real:
                    AddPropertyEditor(path,  new BaseInspectorFloat());
                    return true;
            }
            return false;
        }
    }

    [Tool]
    public abstract class BaseInspector : EditorProperty
    {
        protected bool initialized = false;
        protected object editedObjectInstance = null;

        public string tooltip = "";
        public BaseInspector() {
            //HintTooltip = "stub"; // forces use of tooltip
        }

        private object cachedValue = null;

        public static bool HasInspectorAttributes(Object obj, string propertyName)
        {
            var scriptRef = obj.GetScript();
            object sysObj = null;
            if (scriptRef != null && scriptRef is CSharpScript) sysObj = (scriptRef as CSharpScript).New();
            if (sysObj == null) return false;
            var field = sysObj.GetType().GetField(propertyName);
            if (field != null)
            {
                var attribs = field.GetCustomAttributes(false);
                foreach(var a in attribs)
                {
                    if (a is ExportVariableAttribute) return true;
                }
            } 
            return false;
        }

        public override void UpdateProperty()
        {
            if (!initialized) {
                // create proper elements
                var scriptRef = GetEditedObject().GetScript();
                if (scriptRef != null && scriptRef is CSharpScript) editedObjectInstance = (scriptRef as CSharpScript).New();
                // cache value
                cachedValue = GetEditedObject().Get(GetEditedProperty());
                Create();
                // perform generic attribute applications
                ApplyInspectorAttributes();
                initialized = true;
                // need to clear the boldface and underline bbcode on the default inspector
                HintTooltip = HintTooltip + "[/b][/u]\n" + tooltip + "\n\n[b][u]";
            }
            var realValue = GetEditedObject().Get(GetEditedProperty());
            if (cachedValue != null && cachedValue != realValue)
            {
                 ValueChangedExternal(realValue);
                 cachedValue = realValue;
            }
        }

        


        private void ApplyInspectorAttributes()
        {
            if (editedObjectInstance == null) return;

            var field = editedObjectInstance.GetType().GetField(GetEditedProperty());
            if (field != null)
            {
                var attribs = field.GetCustomAttributes(false);
                foreach(var a in attribs)
                {
                    (a as ExportVariableAttribute)?.Apply(this);
                }
            }
        }

        protected void UpdateValue(object value)
        {
            EmitChanged(GetEditedProperty(), value);
        }

        protected abstract void Create();
        protected abstract void ValueChangedExternal(object n_value);

        public override Control _MakeCustomTooltip(string forText)
        {
            //FIXME How in the hell can I get this to actually run in-editor???
            GD.Print($"Creating a custom tooltip for the given tooltip text: \"{forText}\"");
            GD.PushWarning("Maybe this will show when making a custom tooltip");
            var vbox = new VBoxContainer();

            var propLabel = new Label();
            propLabel.Text = $"{GetEditedObject().Get("name")}:{GetEditedProperty()}";
            propLabel.RectMinSize = new Vector2(300, 50);
            vbox.AddChild(propLabel);

            var customTooltip = new RichTextLabel(); // lets tooltips use BB codes
            customTooltip.BbcodeEnabled = true;
            customTooltip.BbcodeText = "[wave]This is BBCode in the tooltip![/wave]";
            customTooltip.RectMinSize = new Vector2(300, 50);
            vbox.AddChild(customTooltip);

            vbox.RectMinSize = new Vector2(300, 300);
            return vbox;
        }
    }

    [Tool]
    public class BaseInspectorInteger : BaseInspector
    {
        public EditorSpinSlider varSpinBox = null;

        protected override void Create()
        {
            varSpinBox = new EditorSpinSlider();
            varSpinBox.Connect("value_changed", this, "SpinEdited");
            varSpinBox.Value = (int)GetEditedObject().Get(GetEditedProperty());

            AddChild(varSpinBox);
            AddFocusable(varSpinBox);
        }

        public void SpinEdited(float value){
            SpinEdited(value, true);
        }

        public void SpinEdited(float value, bool notifyEditor)
        {
            if(notifyEditor){
                GetEditedObject().Set(GetEditedProperty(), (int)value);
                UpdateValue((int)value);
            }
        }

        protected override void ValueChangedExternal(object n_value)
        {
            varSpinBox.Value = (int)n_value;
            SpinEdited((int)varSpinBox.Value);
        }

    }

    [Tool]
    public class BaseInspectorFloat : BaseInspector
    {
        //SpinBox varSpinBox = null;
        public Range varSlider = null;
        private LineEdit varLabel = null;


        protected override void Create()
        {
            var hbox = new HBoxContainer();

            varLabel = new LineEdit();
            var rect = varLabel.RectMinSize;
            rect.x = 64; // this is hardcoded...not good but IDK how to make it more customizable without making the system extremely obtuse
            varLabel.RectMinSize = rect;
            varLabel.Align = LineEdit.AlignEnum.Left;
            varLabel.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            varLabel.SizeFlagsStretchRatio = 0.1f;
            varLabel.Editable = false;
            hbox.AddChild(varLabel);
            

            varSlider = new HSlider();
            (varSlider as HSlider).TicksOnBorders = true;
            (varSlider as HSlider).TickCount = 5;
            varSlider.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            varSlider.HintTooltip = "A slider OwO";
            hbox.AddChild(varSlider);
            
            var propVal = (float)GetEditedObject().Get(GetEditedProperty());
            varSlider.Connect("value_changed", this, "ValueChanged");
            varSlider.Value = propVal;
            ValueChanged(varSlider.Value, false);

            AddChild(hbox);
            AddFocusable(hbox);
            //SetBottomEditor(hbox);
        }

        public void ValueChanged(double n_value)
        {
            ValueChanged(n_value, true);
        }

        public void ValueChanged(double n_value, bool notifyEditor)
        {
            if (varLabel != null)
            {
                varLabel.Text = n_value.ToString("F2"); // 2 decimal value
                varLabel.HintTooltip = n_value.ToString("G"); // full representation in tooltip
            }
            
            if(notifyEditor){
                GetEditedObject().Set(GetEditedProperty(), (float)n_value);
                UpdateValue((float)n_value);
            } 
                
        }


        protected override void ValueChangedExternal(object n_value)
        {
            varSlider.Value = (float)n_value;
            ValueChanged((float)n_value);
        }

    }
}
#pragma warning restore IDE0017
#endif