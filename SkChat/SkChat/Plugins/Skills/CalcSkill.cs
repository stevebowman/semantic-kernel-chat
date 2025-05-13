using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace SkChat.Skills;

public class CalcSkill
{
    [KernelFunction("add")]
    public static double Add(
        [Description("first number")]  double x,
        [Description("second number")] double y)
    {
        Console.WriteLine($"Calc Skill Invoked! Add called with x={x}, y={y}");
        return x + y;
    }

    [KernelFunction("percentage")]
    public static double Percentage(
        [Description("value")]   double value,
        [Description("percent")] double percent)
    {
        Console.WriteLine($"Calc Skill Invoked! Percentage called with value={value}, percent={percent}");
        return value * percent / 100.0;
    }

    [KernelFunction("sqrt")]
    public static double Sqrt(
        [Description("number")] double n)
    {
        Console.WriteLine($"Calc Skill Invoked! Sqrt called with n={n}");
        return Math.Sqrt(n);
    }
}