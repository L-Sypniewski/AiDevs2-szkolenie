namespace AiDevs2_szkolenie.Tasks;

public record TaskModel(
    int Code,
    string Msg,
    string Question,
    object Input,
    List<string> Blog);
