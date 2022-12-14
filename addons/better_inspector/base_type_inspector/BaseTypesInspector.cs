#if TOOLS
#pragma warning disable IDE0017 
// removes the "SiMpLiFy YoUr InItIaLiZaTiOn!!!! D:<" warning, which isn't even in this C# version

using System.Linq;
using System.Reflection;
using betterinspector.attributes;
using betterinspector.inspectors.custom;
using betterinspector.inspectors.integrated;
using Godot;
using Godot.Collections;

namespace betterinspector.inspectors
{
    [Tool]
    public class BaseTypesInspector : EditorInspectorPlugin
    {

        private readonly Array<string> customProperties = new Array<string>();

        public override bool CanHandle(Object @object)
        {
            return true; // handle all objects, we are parsing base types
        }

        public override void ParseBegin(Object @object)
        {
            customProperties.Clear(); // erase previous
            foreach(var prop in @object.GetPropertyList())
            {
                if (!((prop as Dictionary)["name"] is string name) || name.Empty()) continue;
                if (customProperties.Contains(name)) continue; // prevent duplicates
                if (!BaseInspector.HasInspectorAttributes(@object, name)) continue;
                if (!BaseInspector.UsesIntegrated(@object, name)) customProperties.Add(name);
            }

            if (customProperties.Count <= 0) return; // no custom props
            
            Reference scriptRef = @object.GetScript();
            if (scriptRef != null && scriptRef is CSharpScript)
            {
                var obj = (scriptRef as CSharpScript).New();
                if (obj == null)
                {
                    GD.PushError("Creating an instance from CSharpScript was found null!");
                    return;
                } 
                BuildCustomProperties(obj, @object);
            }
        }

        private void BuildCustomProperties(object csObj, Object gdObject)
        {
            var vbox = new VBoxContainer();
            var clrEntries = new Array<string>();
            var heading = new RichTextLabel();
            heading.BbcodeEnabled = true;
            var nameSpace = csObj.GetType().Namespace;
            heading.BbcodeText = $"[b]{gdObject.Get("name")}[/b]";
            heading.FitContentHeight = true;
            heading.HintTooltip = $"{(nameSpace.Empty() ? "__" : nameSpace)}:{csObj.GetType().Name}";
            if (Plugin.instance != null) heading.Theme = Plugin.instance.customInspectorTheme;
            vbox.AddChild(heading);
            vbox.AddChild(new HSeparator());

            PanelContainer currentCategory = null;
            Container target = vbox;

            foreach (var prop in customProperties)
            {
                var propObj = gdObject.Get(prop);
                var cat = csObj.GetType().GetField(prop).GetCustomAttribute<Category>();
                if (cat != null)
                {
                    currentCategory = new PanelContainer();
                    currentCategory.Name = $"Cat_{cat.name}_";
                    var catBox = new VBoxContainer();
                    currentCategory.AddChild(catBox);
                    target = catBox;
                    var btnToggle = new Button();
                    btnToggle.Text = cat.name;
                    var icon = Plugin.GetIcon(cat.iconName);
                    if (icon != null) btnToggle.Icon = icon;
                    if (cat.fontColor != null)
                    {
                        btnToggle.AddColorOverride("font_color", cat.fontColor);
                        btnToggle.AddColorOverride("font_color_focus", cat.fontColor);
                        btnToggle.AddColorOverride("font_color_hover", cat.fontColor);
                    }

                    btnToggle.Connect("pressed", this, "ToggleElementVisible", new Array{currentCategory});
                    ToggleElementVisible(currentCategory);
                    vbox.AddChild(btnToggle);
                    vbox.AddChild(currentCategory);
                }
                switch (propObj.GetType().FullName)
                {
                    case "System.Int32":
                        target.AddChild(new CustomInspectorInteger(gdObject, csObj, prop));
                        break;
                    case "System.Single":
                        target.AddChild(new CustomInspectorFloat(gdObject, csObj, prop));
                        break;
                    default:
                        GD.PushWarning($"Custom properties of type {propObj.GetType().FullName} is not currently supported by custom inspectors, defaulting to integrated solution. Found on property '{prop}' please use [UseIntegrated] to force use of the integrated system");
                        clrEntries.Add(prop);
                        break;
                }
            }
            AddCustomControl(vbox);
            foreach(var prop in clrEntries) customProperties.Remove(prop);
        }

        public void ToggleElementVisible(Control control)
        {
            control.Visible = !control.Visible;
        }

        public override bool ParseProperty(Object @object, int type, string path, int hint, string hintText, int usage)
        {
            if (customProperties.Contains(path)) return true; // handled by custom systems
            if (!BaseInspector.HasInspectorAttributes(@object, path)) return false;

            // we are using custom attributes at this point, and we want integrated (in the standard style)
            // IDK if this is a helpful use case, which is why custom systems are the default
            switch (type)
            {
                case (int)Variant.Type.Int:
                    AddPropertyEditor(path, new BaseInspectorInteger());
                    return true;
                case (int)Variant.Type.Real:
                    AddPropertyEditor(path, new BaseInspectorFloat());
                    return true;
                default:
                    break;
            }
            return false;
        }
    }

}
#pragma warning restore IDE0017
#endif