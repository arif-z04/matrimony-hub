namespace MatrimonyHub.Application.Common;

public class ServiceResult
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();

    public static ServiceResult Success(string message = "") =>
        new() { Succeeded = true, Message = message };

    public static ServiceResult Failure(string error) =>
        new() { Succeeded = false, Message = error, Errors = new List<string> { error } };

    public static ServiceResult Failure(List<string> errors, string message = "") =>
        new() { Succeeded = false, Message = message, Errors = errors };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }

    public static ServiceResult<T> Success(T data, string message = "") =>
        new() { Succeeded = true, Data = data, Message = message };

    public new static ServiceResult<T> Failure(string error) =>
        new() { Succeeded = false, Message = error, Errors = new List<string> { error } };

    public new static ServiceResult<T> Failure(List<string> errors, string message = "") =>
        new() { Succeeded = false, Message = message, Errors = errors };
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 1));
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
