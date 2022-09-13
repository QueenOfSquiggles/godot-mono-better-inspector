using Godot;

namespace betterinspector.inspectors
{
    public interface IBetterPropertyEditor
    {
        Range GetRangeElement();
        void SetEditorCheckable(bool isCheckable);
        void SetEditorDrawRed(bool isDrawRed);
        void SetEditorKeying(bool isKeying);
        void SetEditorReadOnly(bool isReadOnly);
        void SetEditorTooltipText(string tooltip);
        void SetLabelColour(Color colour);
        void SetLabelText(string label);
        void SetUseBottomEditor();
    }
}