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
        void SetLabelText(string label);
        void SetUseBottomEditor();
    }
}