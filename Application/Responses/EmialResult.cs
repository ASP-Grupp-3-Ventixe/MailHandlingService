namespace Application.Responses;


public class EmailResult : ResponseResult
{
    public string? Result { get; set; }
}

public class EmailResult<T> : ResponseResult
{
    public T? Result { get; set; }
}

public class EmailListResult<T> : ResponseResult
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }    
    public int UnreadCount { get; set; }
    public int? SelectedIndex { get; set; }
    public string DisplayCount => TotalCount > 999 ? "999+" : TotalCount.ToString();
}

public class EmailNavigationResult<T> : ResponseResult
{
    public T? Item { get; set; } = default!; 
    public int CurrentPosition { get; set; } 
    public int TotalCount { get; set; }    
    public Guid? PreviousEmailId { get; set; } 
    public Guid? NextEmailId { get; set; }    
}

public class CountResult : ResponseResult
{
    public int Count { get; set; }
    public string DisplayCount => Count > 999 ? "999+" : Count.ToString();
}