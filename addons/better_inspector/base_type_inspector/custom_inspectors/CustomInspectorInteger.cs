using Godot;

namespace betterinspector.inspectors.custom
{
    public class CustomInspectorInteger : CustomInspectorBase
    {

        private EditorSpinSlider slider;

        public CustomInspectorInteger(Object gdObj, object csObj, string handledProperty) : base(gdObj, csObj, handledProperty)
        {}

        protected override void Rebuild(){
            var hbox = new HBoxContainer();
            slider = new EditorSpinSlider();
            base.Rebuild(); // init label
        }


        public override Range GetRangeElement()
        {
            return slider;
        }

        public override void SetEditorCheckable(bool isCheckable)
        {
            throw new System.NotImplementedException();
        }

        public override void SetEditorDrawRed(bool isDrawRed)
        {
            throw new System.NotImplementedException();
        }

        public override void SetEditorKeying(bool isKeying)
        {
            throw new System.NotImplementedException();
        }

        public override void SetEditorReadOnly(bool isReadOnly)
        {
            throw new System.NotImplementedException();
        }

        public override void SetUseBottomEditor()
        {
            throw new System.NotImplementedException();
        }
    }
}