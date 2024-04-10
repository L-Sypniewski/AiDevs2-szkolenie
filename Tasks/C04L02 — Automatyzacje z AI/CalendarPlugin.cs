using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace AiDevs2_szkolenie.Tasks;

public class CalendarPlugin
{
    [KernelFunction,
     Description(
         "Given a task to do, add a calendar entry.")]
    public ToolModel GetCalendar(
        [Description("Provided task that has date or time information.")] string task,
        [Description("Date or time of the task in format dd-MM-YYYY")] string date)
    {
        return new ToolModel { Tool = Tool.Calendar, Desc = task, Date = DateTime.ParseExact(date, "dd-MM-yyyy", null) };
    }
}
