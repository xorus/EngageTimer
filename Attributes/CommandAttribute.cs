using System;
using JetBrains.Annotations;

namespace EngageTimer.Attributes;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
    public CommandAttribute(string command)
    {
        Command = command;
    }

    public string Command { get; }
}