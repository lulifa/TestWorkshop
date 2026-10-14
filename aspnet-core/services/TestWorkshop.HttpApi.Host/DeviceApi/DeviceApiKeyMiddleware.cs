using System.Security.Cryptography;
using System.Text;

namespace TestWorkshop;

/// <summary>
/// 下位机匿名接口的共享密钥校验。
/// 心跳 / 上传 / 实时数据这三个接口在下位机侧没有登录态，只能走匿名，
/// 这里统一用请求头里的共享密钥兜一层，校验不通过直接 401，不进入业务逻辑。
/// </summary>
public class DeviceApiKeyMiddleware : IMiddleware
{
    public const string HeaderName = "X-Device-Key";

    /// <summary>需要校验的下位机接口路径</summary>
    private static readonly string[] ProtectedPaths =
    {
        "/api/workshop/telemetry/online",
        "/api/workshop/telemetry/upload",
        "/api/workshop/realtime-data"
    };

    private readonly DeviceApiOptions _options;
    private readonly ILogger<DeviceApiKeyMiddleware> _logger;

    public DeviceApiKeyMiddleware(IOptions<DeviceApiOptions> options, ILogger<DeviceApiKeyMiddleware> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!_options.Enabled || !IsProtectedPath(context.Request.Path))
        {
            await next(context);
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            // 开了校验却没配密钥：一律拒绝，避免"以为开了其实在裸奔"
            _logger.LogError("DeviceApi 已启用但未配置 ApiKey，下位机请求全部拒绝。");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var provided)
            || !IsSameKey(provided.ToString(), _options.ApiKey))
        {
            _logger.LogWarning(
                "下位机接口密钥校验失败：{Method} {Path}，RemoteIp={RemoteIp}",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await next(context);
    }

    private static bool IsProtectedPath(PathString path)
    {
        return ProtectedPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsSameKey(string provided, string expected)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(provided),
            Encoding.UTF8.GetBytes(expected));
    }
}