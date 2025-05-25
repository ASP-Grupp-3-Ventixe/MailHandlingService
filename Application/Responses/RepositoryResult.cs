namespace Application.Responses;

public class RepositoryResult : ResponseResult
{
}

public class RepositoryResult<T> : ResponseResult
{
    public T? Result { get; set; }
}