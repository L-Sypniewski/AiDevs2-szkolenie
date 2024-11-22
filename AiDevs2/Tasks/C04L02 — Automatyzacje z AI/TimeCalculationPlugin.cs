using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace AiDevs2_szkolenie.Tasks;

public class TimeCalculationPlugin
{
    [KernelFunction,
     Description(
         "Extract a date offset from a given date. The offset can be in days, months, or years. The offset can be positive or negative. Offset is always in days in integer format.")]
    public DateTimeOffset GetDateTimeWithOffset(
        [Description("Offset in days")] int offset)
    {
        return DateTimeOffset.Now.AddDays(offset);
    }

    [KernelFunction, Description("Get the current date in dd-MM-yyyy format")]
    public string Date() =>
        // Example: Sunday, 12 January, 2025
        DateTimeOffset.Now.ToString("dd-MM-yyyy");
}
