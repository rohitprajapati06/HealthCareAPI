using FluentValidation;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthCare.API.Models;
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
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.Success = false;
                    response.Message = ex.Message;
                    break;

                case ValidationException validationException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Success = false;
                    response.Message = "Validation Failed";
                    response.Errors = validationException.Errors.Select(x => x.ErrorMessage).ToList();
                    break;

                case NotFoundException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    response.Success = false;
                    response.Message = ex.Message;
                    break;

                case ConflictException:
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    response.Success = false;
                    response.Message = ex.Message;
                    break;

                case BadRequestException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Success = false;
                    response.Message = ex.Message;
                    break;


                case ForbiddenException:
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    response.Success = false;
                    response.Message = ex.Message;
                    break;


                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    response.Success = false;
                    response.Message = "An unexpected error occurred.";
                    break;
            }

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
