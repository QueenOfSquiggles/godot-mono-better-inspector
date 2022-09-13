using betterinspector.attributes;
using Godot;
using System;

namespace squiggles.demo
{
    public class ExampleNode : Node
    {
        //  - - - - - - - - - - - - -
        //  Exported Vars
        //  - - - - - - - - - - - - -

        [Export]
        [CustomLabel("Customized Integer!")]
        [Tooltip("this is an [code]int[/code] variable, exported with metadata for the [i][b]custom inspector")]
        public int varInt = 4;

        [BottomInspector]
        [DrawRed]
        [Tooltip("What a lovely thing <3")]
        [Export]
        public int varIntTheSecond = 5;

        [LabelColour("#FF0000")]
        [Category("Colours", "#FFFFFF")]
        [Export]
        public int colourA = 4;
        [LabelColour("#00FF00")]
        [Export]
        public int colourB = 4;
        [LabelColour("#0000FF")]
        [Export]
        public int colourC = 4;
        [LabelColour("#00FFFF")]
        [Export]
        public int colourD = 4;

        [Category("Spatial Props", "#FF0000", "Spatial")]
        [Export]
        public int spatialPropX = 5;

        [Export]
        [ForceUseBetterInspector]
        public int spatialPropY = 5;

        [Export]
        [ForceUseBetterInspector]
        public int spatialPropZ = 5;



    }
}