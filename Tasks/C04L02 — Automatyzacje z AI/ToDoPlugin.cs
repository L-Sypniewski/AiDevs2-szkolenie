using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace AiDevs2_szkolenie.Tasks;

public class ToDoPlugin
{
    [KernelFunction,
     Description("Given a task to do, add a todo item.")]
    public ToolModel GetTodo([Description("Provided task without date or time information.")] string task)
    {
        return new ToolModel { Tool = Tool.ToDo, Desc = task };
    }
}
