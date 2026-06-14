using FluentValidation;
using System.Net;
using System.Text.Json;

namespace SmartHealthCare.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate requestDelegate;
        private readonly ILogger<ExceptionMiddleware> logger;

        public ExceptionMiddleware(RequestDelegate requestDelegate , ILogger<ExceptionMiddleware> logger)
        {
            this.requestDelegate = requestDelegate;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await requestDelegate(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                logger.LogError(ex,"Unhandled Exception for {Method} {Path}",context.Request.Method , context.Request.Path);
                await HandleExceptionAsync(context,ex );
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            var responsemessage = new
            {
                Message = ex.Message,
            };

            switch (ex)
            {
                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                case ValidationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            await context.Response.WriteAsync(JsonSerializer.Serialize(responsemessage));
        }
    }
}
