namespace Application.Responses;

public class LabelResult : ResponseResult
{
    public string? Result { get; set; }
}

public class LabelResult<T> : ResponseResult
{
    public T? Result { get; set; }
}