using System;

namespace EngageTimer.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
    public CommandAttribute(string command)
    {
        Command = command;
    }

    public string Command { get; }
}