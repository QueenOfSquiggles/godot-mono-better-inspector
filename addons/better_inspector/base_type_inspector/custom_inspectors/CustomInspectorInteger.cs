using Godot;

namespace betterinspector.inspectors.custom
{
    public class CustomInspectorInteger : CustomInspectorBase
    {

        
        private EditorSpinSlider slider;
        private Label propertyLabel;
        private Button resetButton;

        public CustomInspectorInteger(Object gdObj, object csObj, string handledProperty) : base(gdObj, csObj, handledProperty)
        {}

        protected override void Rebuild(bool vertical){
            var children = GetChildren();
            foreach (Node c in children)
            {
                RemoveChild(c);
                c.QueueFree();
            }

            Container box = null;
            if (vertical)
            {
                box = new VBoxContainer();
            } else{
                box = new HBoxContainer();
            } 
            box.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

            propertyLabel = new Label();
            propertyLabel.Text = propertyName;
            propertyLabel.MouseFilter = MouseFilterEnum.Pass;
            propertyLabel.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            propertyLabel.SizeFlagsStretchRatio = 0.2f;
            box.AddChild(propertyLabel);

            resetButton = CreateResetButton();
            resetButton.Visible = !IsValueDefault();
            box.AddChild(resetButton);

            slider = new EditorSpinSlider();
            slider.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            slider.AllowGreater = true;
            slider.AllowLesser = true;
            box.AddChild(slider);

            AddChild(box);
            slider.Connect("value_changed", this, "OnSliderValueChanged");
            slider.Value = (int)GetCurrentValue();
        }

        public void OnSliderValueChanged(float value)
        { // We need a transitory function to ensure the values match the needed variable type
            SaveNewValue((int)value);
            resetButton.Visible = !IsValueDefault();
        }

        public override void ResetValue()
        {
            base.ResetValue();
            slider.Value = (int)gdObj.Get(propertyName);
            resetButton.Visible = false;
        }


        public override Range GetRangeElement()
        {
            return slider;
        }

        public override void SetEditorCheckable(bool isCheckable)
        {
        }

        public override void SetEditorDrawRed(bool isDrawRed)
        {
            if (isDrawRed)
                SetLabelColour(new Color(1f, 0f, 0f));
            else if (propertyLabel.HasColorOverride("font_color")) propertyLabel.RemoveColorOverride("font_color");
        }

        public override void SetEditorKeying(bool isKeying)
        {
        }

        public override void SetEditorReadOnly(bool isReadOnly)
        {
            slider.ReadOnly = true;
        }

        public override void SetLabelText(string label)
        {
            propertyLabel.Text = label;
        }

        public override void SetLabelColour(Color colour)
        {
            propertyLabel.AddColorOverride("font_color", colour);
        }

        public override bool IsValueDefault()
        {
            var baseVal = (int)(csObj as Node).Get(propertyName);
            var editVal = (int)(gdObj as Node).Get(propertyName);
            return baseVal == editVal;
        }
    }
}