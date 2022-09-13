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
        [Tooltip("this is an [code]int[/code] variable, exported with metadata for the [i][b]custom inspector")]
        public int varInt = 4;

        [Export]
        [Tooltip("What a lovely thing <3")]
        public float varFloat = 12.0f;

        [Export]
        [Tooltip("probably not implemented yet")]
        public string varString = "temp";


    }
}