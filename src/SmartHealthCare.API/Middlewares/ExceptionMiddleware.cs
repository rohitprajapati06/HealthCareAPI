using FluentValidation;
using SmartHealthCare.API.Models;
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
                logger.LogError(ex,"Unhandled Exception for {Method} {Path}",context.Request.Method , context.Request.Path);
                await HandleExceptionAsync(context,ex );
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            ApiResponse response = new();

            switch (ex)
            {
                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Success = false;
                    response.Message = ex.Message;
                    break;

                case ValidationException validationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Success = false;
                    response.Message = "Validation Failed";
                    response.Errors = validationException.Errors.Select(x => x.ErrorMessage).ToList();
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Success = false;
                    response.Message = ex.Message;
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Success = false;
                    response.Message = ex.Message;
                    break;
            }

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
