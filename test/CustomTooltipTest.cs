using Godot;
using System;

public class CustomTooltipTest : PanelContainer
{
    public override Control _MakeCustomTooltip(string forText)
    {
        var label = new RichTextLabel();
        label.BbcodeEnabled = true;
        label.BbcodeText = $"[i][wave]{forText}[/wave][/i]";
        label.RectMinSize = new Vector2(200, 50);
        return label;
    }
}
