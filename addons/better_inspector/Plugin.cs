#if TOOLS
using betterinspector.attributes;
using betterinspector.inspectors;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[Tool]
public class Plugin : EditorPlugin
{


    private BaseTypesInspector inspectBaseTypes;

    public override void _EnterTree()
    {
        // init
        inspectBaseTypes = new BaseTypesInspector();

        // register
        this.AddInspectorPlugin(inspectBaseTypes);
    }

    public override void _ExitTree()
    {
        // clear registry
        this.RemoveInspectorPlugin(inspectBaseTypes);
    }

    private static IEnumerable<Type> GetCustomRegisteredTypes()
    {
        var assembly = Assembly.GetAssembly(typeof(Plugin));
        return assembly.GetTypes().Where(t => !t.IsAbstract
            && Attribute.IsDefined(t, typeof(ExportVariableAttribute)) 
            && (t.IsSubclassOf(typeof(Node)) || t.IsSubclassOf(typeof(Resource)))
            );
    }

}
#endif