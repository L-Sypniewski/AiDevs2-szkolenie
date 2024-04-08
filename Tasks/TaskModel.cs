namespace AiDevs2_szkolenie.Tasks;

public record TaskModel(
    int Code,
    string Msg,
    string Question,
    object Input,
    string Hint,
    string? Data,
    string? Hint1,
    string? Hint2,
    List<string>? Blog);
