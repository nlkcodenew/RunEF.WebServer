using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace RunEF.WebServer.Api.Middleware;

public class ClientAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public ClientAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Kiểm tra nếu request đến từ RunEF-Client
        if (IsRunEFClientRequest(context))
        {
            // Xác thực client dựa trên API key và computer code
            if (ValidateClientRequest(context))
            {
                // Thêm thông tin client vào context
                context.Items["IsRunEFClient"] = true;
                context.Items["ComputerCode"] = GetComputerCode(context);
                context.Items["ClientIP"] = GetClientIP(context);
            }
            else
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized client request");
                return;
            }
        }

        await _next(context);
    }

    private bool IsRunEFClientRequest(HttpContext context)
    {
        // Kiểm tra User-Agent hoặc custom header
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var apiKey = context.Request.Headers["X-API-Key"].ToString();
        
        return userAgent.Contains("RunEF-Client") || !string.IsNullOrEmpty(apiKey);
    }

    private bool ValidateClientRequest(HttpContext context)
    {
        var apiKey = context.Request.Headers["X-API-Key"].ToString();
        var apiSecret = context.Request.Headers["X-API-Secret"].ToString();
        var computerCode = context.Request.Headers["X-Computer-Code"].ToString();

        // Kiểm tra các header bắt buộc
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret) || string.IsNullOrEmpty(computerCode))
        {
            return false;
        }

        // Validate API key format (có thể mở rộng thêm)
        if (!apiKey.StartsWith("RUNEF_API_KEY_") || !apiSecret.StartsWith("RUNEF_SECRET_"))
        {
            return false;
        }

        return true;
    }

    private string GetComputerCode(HttpContext context)
    {
        return context.Request.Headers["X-Computer-Code"].ToString();
    }

    private string GetClientIP(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}
