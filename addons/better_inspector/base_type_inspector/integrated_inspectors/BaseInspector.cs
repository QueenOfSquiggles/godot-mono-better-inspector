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
                if (scriptRef is GDScript)
                {
                    // easy search, ## is specific to the meta-data generation
                    return (scriptRef as GDScript).SourceCode.Contains("##");
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
                if (scriptRef is GDScript) return true; // Force GDscript to use integrated
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
                    if (scriptRef is CSharpScript) ParseCsharp(scriptRef as CSharpScript);
                    if (scriptRef is GDScript) ParseGdscript(scriptRef as GDScript);
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
            string[] lines = script.SourceCode.Split('\n');
            Array<string> commentLines = new Array<string>();
            // get the comment lines that precede the export declaration. Blocks need to be contiguous chains of "##" comments
            // attributes are parsed out from the @tagname tags, trying to emulate the Godot 4 style
            foreach (string line in lines)
            {
                if (line.Contains("##"))
                {
                    commentLines.Add(line);
                }

                if (line.Contains(GetEditedProperty()) && line.Contains("export"))
                {
                    // variable must be defined before it can be used. The first naming of it which also contains export keyword should be the export call
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
                for (int i = 0; i < commentLines.Count; i++)
                {
                    if(!commentLines[i].Contains('@')) // @ denotes a custom attribute
                    {
                        if (i > 0) tooltip += "\n";
                        tooltip += commentLines[i].Replace("##", "");
                    } else {
                        var meta = ParseGDMetaLine(commentLines[i]);
                        var demo = "";
                        foreach(var m in meta) demo += m + "\n";
                        switch(meta[0])
                        {
                            case "label":
                                var lbl = meta[1];
                                for(int j = 2; j < meta.Length; j++) lbl += meta[j];
                                new CustomLabel(lbl).Apply(this);
                                break;
                            case "bottom":
                                new BottomInspector().Apply(this);
                                break;
                            case "range":
                            //  min, max, step, rounded, limits
                                float min = 0.0f;
                                float max = 1.0f;
                                float step = 1.0f;
                                bool rounded = false;
                                EditorRange.RangeLimitOptions limits = EditorRange.RangeLimitOptions.CLAMP_BOTH;
                                {
                                    if (meta.Length >= 2) if (float.TryParse(meta[1], out var n_min)) min = n_min;
                                    if (meta.Length >= 3) if (float.TryParse(meta[2], out var n_max)) max = n_max;
                                    if (meta.Length >= 4) if (float.TryParse(meta[3], out var n_step)) step = n_step;
                                    if (meta.Length >= 5) if (bool.TryParse(meta[4], out var n_rounded)) rounded = n_rounded;
                                    if (meta.Length >= 6)
                                    {
                                        if (System.Enum.TryParse<EditorRange.RangeLimitOptions>(meta[5], true, out var n_limits)) limits = n_limits;
                                    }
                                }
                                new EditorRange(min, max, step, rounded, limits).Apply(this);
                                break;
                        }

                    }
                }
            }
        }
        private string[] ParseGDMetaLine(string line)
        {
            // returns an array where index 0 is the tag name, and all other elements are the parameters
            var tagStart = line.Find('@');
            var tagEnd = line.Find(" ", tagStart);
            if (tagEnd < 0) return new string[]{line.Substr(tagStart+1, line.Length - tagStart)}; // lonely tag, no params
            var tag = line.Substr(tagStart+1, tagEnd - (tagStart+1));
            if (line.Length <= (tagEnd)+1) return new string[]{line.Substr(tagStart+1, line.Length - tagStart)}; // lonely tag, no params
            var tokenLine = line.Substr(tagEnd+1, line.Length - (tagEnd+1));
            var tokens = tokenLine.Split(",");
            var arr = new string[tokens.Length+1];
            if (tokens.Length <= 0)
            {
                arr = new string[2];
                arr[0] = tag;
                arr[1] = tokenLine;
            }else{
                arr[0] = tag;
                for(int i = 0; i< tokens.Length; i++)
                {
                    arr[i+1] = tokens[i];
                }
            }
            return arr;
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

        public void SetLabelColour(Color colour)
        {
            GD.PushError($"Arbitrary Label Colours on {GetEditedObject().Get("Name")}:{GetEditedProperty()} is not valid for integrated editors. Use the customized variant for more options");
        }
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