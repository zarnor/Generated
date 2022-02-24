using System;

namespace Generated.Builders;

internal static class BooleanExtensions
{
    public static void If(this bool value, Action trueAction, Action falseAction)
    {
        if (value)
        {
            trueAction();
        }
        else
        {
            falseAction();
        }
    }
}
