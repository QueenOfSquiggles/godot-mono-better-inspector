using betterinspector.attributes;
using Godot;
using System;

public class ExampleNode : Node
{
    //  - - - - - - - - - - - - -
    //  Exported Vars
    //  - - - - - - - - - - - - -

    [Export]
    [CustomLabel("More Expressive Label <3")]
    [Tooltip("And there's even more information here!")]
    [EditorRange(-15, 5)]
    [BottomInspector]
    public int varInt = 4;

    [Export]
    [EditorRange(0.0f, 10.0f)]
    [Tooltip("Line 1 - lorem ipsum dolor sit amet, this is a really fucking long line. How will it cope? Seething? Crying? Wailing? C'mon c'mon crash on me. I dare you!!!", "Line 2", "Line 3", "Line 4")]
    public float varFloat = 3.57f;

    [Export]
    public float useNormal = 1.0f;

    [Export][ForceUseBetterInspector]
    public float useBetterInspectorOnly = 2.3f;

    [Export]
    public string varString = "";



}
