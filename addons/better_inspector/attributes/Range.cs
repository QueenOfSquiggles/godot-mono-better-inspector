using System;
using betterinspector.inspectors;
using Godot;

namespace betterinspector.attributes
{


    [AttributeUsage(AttributeTargets.Field)]
    public class EditorRange : ExportVariableAttribute
    {
        public enum RangeLimitOptions
        {
            ALLOW_GREATER, ALLOW_LESSER, NO_LIMITS, CLAMP_BOTH
        }
        
        public readonly float minVal;
        public readonly float maxVal;
        public readonly float step;
        public readonly bool rounded = false;
        public readonly RangeLimitOptions rangeLimits;



        public EditorRange(float minVal, float maxVal, float step = -1.0f, bool rounded = false, RangeLimitOptions rangeLimits = RangeLimitOptions.CLAMP_BOTH)
        {
            this.minVal = minVal;
            this.maxVal = maxVal;
            this.step = step;
            this.rounded = rounded;
            this.rangeLimits = rangeLimits;
        }

        public override void Apply(BaseInspector control)
        {
            Range rangeControl = null;
            if (control is BaseInspectorFloat) rangeControl = (control as BaseInspectorFloat).varSlider;
            if (control is BaseInspectorInteger) rangeControl = (control as BaseInspectorInteger).varSpinBox;

            if(rangeControl != null){
                // common base class is range, so I can let this attribute apply for integer and float values!
                rangeControl.MinValue = minVal;
                rangeControl.MaxValue = maxVal;
                rangeControl.Step = step;
                rangeControl.Rounded = rounded;
                rangeControl.Value = Mathf.Clamp((float)rangeControl.Value, minVal, maxVal);
                
                switch(rangeLimits)
                {
                    case RangeLimitOptions.CLAMP_BOTH:
                        rangeControl.AllowGreater = false;
                        rangeControl.AllowLesser = false;
                        break;
                    case RangeLimitOptions.ALLOW_GREATER:
                        rangeControl.AllowGreater = true;
                        rangeControl.AllowLesser = false;
                        break;
                    case RangeLimitOptions.ALLOW_LESSER:
                        rangeControl.AllowGreater = false;
                        rangeControl.AllowLesser = true;
                        break;
                    case RangeLimitOptions.NO_LIMITS:
                        rangeControl.AllowGreater = true;
                        rangeControl.AllowLesser = true;
                        break;
                }

            } else {
                GD.PushError($"Inspector Error: {control.GetEditedObject()}:{control.GetEditedProperty()} has attribute {nameof(Range)}, which is not allowed for non-float values");
            }
        }
    }

}