namespace AiDevs2_szkolenie.Tasks;

public record TaskModel(
    int Code,
    string Msg,
    string Question,
    object Input,
    string Hint,
    List<string> Blog);
