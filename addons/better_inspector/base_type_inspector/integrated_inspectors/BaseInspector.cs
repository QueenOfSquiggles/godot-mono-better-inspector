
using System.Linq;
using betterinspector.attributes;
using Godot;
using Godot.Collections;

namespace betterinspector.inspectors.integrated{
    [Tool]
    public abstract class BaseInspector : EditorProperty, IBetterPropertyEditor
    {
        protected bool initialized = false;
        //protected object editedObjectInstance = null;

        public string tooltip = "";
        public BaseInspector()
        {
            //HintTooltip = "stub"; // forces use of tooltip
        }

        private object cachedValue = null;

        public static bool HasInspectorAttributes(Object obj, string propertyName)
        {
            Reference scriptRef = obj.GetScript();
            object sysObj = null;
            if (scriptRef != null)
            {
                if (scriptRef is CSharpScript)
                {
                    sysObj = (scriptRef as CSharpScript).New();
                }
            }
            if (sysObj == null)
            {
                return false;
            }

            System.Reflection.FieldInfo field = sysObj.GetType().GetField(propertyName);
            if (field != null)
            {
                object[] attribs = field.GetCustomAttributes(false);
                foreach (object a in attribs)
                {
                    if (a is ExportVariableAttribute)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool UsesIntegrated(Object obj, string propertyName)
        {
            Reference scriptRef = obj.GetScript();
            object sysObj = null;
            if (scriptRef != null)
            {
                if (scriptRef is CSharpScript) sysObj = (scriptRef as CSharpScript).New();
            }
            if (sysObj == null)
            {
                return false;
            }

            System.Reflection.FieldInfo field = sysObj.GetType().GetField(propertyName);
            if (field != null)
            {
                object[] attribs = field.GetCustomAttributes(false);
                foreach (object a in attribs)
                {
                    if (a is UseIntegrated) return true;
                }
            }
            return false;
        }

        public override void UpdateProperty()
        {
            if (!initialized)
            {
                // cache value
                cachedValue = GetEditedObject().Get(GetEditedProperty());
                Create();
                // Parse metadata
                Reference scriptRef = GetEditedObject().GetScript();
                if (scriptRef != null)
                {
                    if (scriptRef is CSharpScript)
                    {
                        ParseCsharp(scriptRef as CSharpScript);
                    }

                    if (scriptRef is GDScript)
                    {
                        ParseGdscript(scriptRef as GDScript);
                    }
                }
                // Finish intialization
                initialized = true;
                // FIXME - improve janky tooltip solution!
                // need to clear the boldface and underline bbcode on the default inspector
                HintTooltip = HintTooltip + "[/b][/u]\n" + tooltip + "\n\n[b][u]";
            }
            object realValue = GetEditedObject().Get(GetEditedProperty());
            if (cachedValue != null && cachedValue != realValue)
            {
                ValueChangedExternal(realValue);
                cachedValue = realValue;
            }
        }

        private void ParseCsharp(CSharpScript script)
        {
            object csharpObj = script.New();
            if (csharpObj == null)
            {
                return;
            }

            System.Reflection.FieldInfo field = csharpObj.GetType().GetField(GetEditedProperty());
            if (field != null)
            {
                object[] attribs = field.GetCustomAttributes(false);
                foreach (object a in attribs)
                {
                    (a as ExportVariableAttribute)?.Apply(this);
                }
            }
        }

        private void ParseGdscript(GDScript script)
        {
            // TODO how do we want to parse GDScript metadata
            string[] lines = script.SourceCode.Split('\n');
            Array<string> commentLines = new Array<string>();
            // get the comment lines that precede the export declaration. Blocks need to be contiguous chains of "##" comments
            // attributes are parsed out from the @tagname tags, trying to emulate the Godot 4 style
            foreach (string line in lines)
            {
                //GD.Print($"[Parsing({GetEditedProperty()})] > {line}");
                if (line.Contains("##"))
                {
                    commentLines.Add(line);
                    GD.Print($"CommentLine '{line}'");
                }

                if (line.Contains(GetEditedProperty()) && line.Contains("export"))
                {
                    // variable must be defined before it can be used. The first naming of it which also contains export keyword should be the export call
                    GD.Print($"ExportLine '{line}'");
                    break; // we actually don't need this line, but it's the point at which we can stop caching blocks of comments
                }

                if (!line.Contains("##"))
                {
                    commentLines.Clear();
                }
            }
            if (commentLines.Count > 0)
            {
                // parse out comment attributes
                tooltip = commentLines[0];
                for (int i = 1; i < commentLines.Count; i++)
                {
                    tooltip += "\n" + commentLines[i];
                }
            }
            GD.Print("Property Comments:\n", commentLines);
        }

        protected void UpdateValue(object value)
        {
            EmitChanged(GetEditedProperty(), value);
        }

        protected abstract void Create();
        protected abstract void ValueChangedExternal(object n_value);

        public override Control _MakeCustomTooltip(string forText)
        {
            //FIXME This method does not run due to an engine bug. I'm submitting a bug report for this use case.
            VBoxContainer vbox = new VBoxContainer();

            Label propLabel = new Label();
            propLabel.Text = $"{GetEditedObject().Get("name")}:{GetEditedProperty()}";
            propLabel.RectMinSize = new Vector2(300, 50);
            vbox.AddChild(propLabel);

            RichTextLabel customTooltip = new RichTextLabel(); // lets tooltips use BB codes
            customTooltip.BbcodeEnabled = true;
            customTooltip.BbcodeText = forText;
            customTooltip.RectMinSize = new Vector2(300, 50);
            vbox.AddChild(customTooltip);

            vbox.RectMinSize = new Vector2(300, 300);
            return vbox;
        }

        public abstract Range GetRangeElement();
        public void SetEditorCheckable(bool isCheckable) => Checkable = isCheckable;
        public void SetEditorDrawRed(bool isDrawRed) => DrawRed = isDrawRed;
        public void SetEditorKeying(bool isKeying) => Keying = isKeying;
        public void SetEditorReadOnly(bool isReadOnly) => ReadOnly = isReadOnly;
        public void SetEditorTooltipText(string tooltip) => this.tooltip = tooltip;
        public void SetLabelText(string label) => this.Label = label;
        public void SetUseBottomEditor() => this.SetBottomEditor(GetChild(0) as Control);
    }

    [Tool]
    public class BaseInspectorInteger : BaseInspector
    {
        public EditorSpinSlider varSpinBox = null;

        protected override void Create()
        {
            varSpinBox = new EditorSpinSlider();
            _ = varSpinBox.Connect("value_changed", this, "SpinEdited");
            varSpinBox.Value = (int)GetEditedObject().Get(GetEditedProperty());

            AddChild(varSpinBox);
            AddFocusable(varSpinBox);
        }

        public void SpinEdited(float value)
        {
            SpinEdited(value, true);
        }

        public void SpinEdited(float value, bool notifyEditor)
        {
            if (notifyEditor)
            {
                GetEditedObject().Set(GetEditedProperty(), (int)value);
                UpdateValue((int)value);
            }
        }

        protected override void ValueChangedExternal(object n_value)
        {
            varSpinBox.Value = (int)n_value;
            SpinEdited((int)varSpinBox.Value);
        }

        public override Range GetRangeElement()
        {
            return varSpinBox;
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
            HBoxContainer hbox = new HBoxContainer();

            varLabel = new LineEdit();
            Vector2 rect = varLabel.RectMinSize;
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

            float propVal = (float)GetEditedObject().Get(GetEditedProperty());
            _ = varSlider.Connect("value_changed", this, "ValueChanged");
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

            if (notifyEditor)
            {
                GetEditedObject().Set(GetEditedProperty(), (float)n_value);
                UpdateValue((float)n_value);
            }

        }


        protected override void ValueChangedExternal(object n_value)
        {
            varSlider.Value = (float)n_value;
            ValueChanged((float)n_value);
        }

        public override Range GetRangeElement()
        {
            return varSlider;
        }
    }
}