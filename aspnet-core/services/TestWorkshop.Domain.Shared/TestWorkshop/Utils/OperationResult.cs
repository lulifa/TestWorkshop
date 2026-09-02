namespace TestWorkshop;

public class OperationResult<T>
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public T Data { get; set; }

    public static OperationResult<T> SuccessResult(T data)
    {
        return new OperationResult<T> { Success = true, Data = data };
    }

    public static OperationResult<T> Fail(string errorMessage)
    {
        return new OperationResult<T> { Success = false, ErrorMessage = errorMessage };
    }
}

public class OperationResult : OperationResult<object>
{
    public static OperationResult SuccessResult()
    {
        return new OperationResult { Success = true };
    }
    public static new OperationResult Fail(string errorMessage)
    {
        return new OperationResult { Success = false, ErrorMessage = errorMessage };
    }
}

public class OperationOption<T>
{
    public OperationOption()
    {

    }
    public OperationOption(T key, string value)
    {
        Key = key;
        Value = value;
    }

    public T Key { get; set; }

    public string Value { get; set; }
}

public class OperationOption : OperationOption<string>
{
    public OperationOption() { }
    public OperationOption(string key, string value) : base(key, value) { }
}

public class OperationOptionInt : OperationOption<int>
{
    public OperationOptionInt() { }
    public OperationOptionInt(int key, string value) : base(key, value) { }

    public OperationOptionInt(int key, string value, object type) : base(key, value)
    {
        this.type = type;
    }

    // 当type为null时，序列化时忽略
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object type { get; set; }

}
