using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace SkChat.Skills;

public class CalcSkill
{
    [KernelFunction("add")]
    [Description("Add two numbers.")]
    public static double Add(
        [Description("first number")]  double x,
        [Description("second number")] double y)
    {
        Console.WriteLine($"Calc Skill Invoked! Add called with x={x}, y={y}");
        return x + y;
    }

    [Description("Work out a given percentage of a value.")]
    [KernelFunction("percentage")]
    public static double Percentage(
        [Description("value")]   double value,
        [Description("percent")] double percent)
    {
        Console.WriteLine($"Calc Skill Invoked! Percentage called with value={value}, percent={percent}");
        return value * percent / 100.0;
    }

    [Description("Get the equare root of a numbers.")]
    [KernelFunction("sqrt")]
    public static double Sqrt(
        [Description("number")] double n)
    {
        Console.WriteLine($"Calc Skill Invoked! Sqrt called with n={n}");
        return Math.Sqrt(n);
    }
}