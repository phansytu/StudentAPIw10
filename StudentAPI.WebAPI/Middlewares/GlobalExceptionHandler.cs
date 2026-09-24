using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StudentAPI.Application.Common.Exceptions;
namespace StudentAPI.WebAPI.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var correlationId = httpContext.Items["CorrelationId"]?.ToString() ?? httpContext.TraceIdentifier;
            if (exception is ValidationException validationException)
            {
                _logger.LogWarning(
                    exception,
                    "Validation thất bại tại {Path}",
                    httpContext.Request.Path
                );

                var errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                var validationProblem = new HttpValidationProblemDetails(errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Dữ liệu không hợp lệ",
                    Detail = "Vui lòng kiểm tra lại dữ liệu gửi lên.",
                    Instance = httpContext.Request.Path,
                    Extensions = { ["correlationId"] = correlationId }
                };

                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                await httpContext.Response.WriteAsJsonAsync(validationProblem, cancellationToken);
                return true;
            }


            var (statusCode, title) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Không tìm thấy tài nguyên"),
                BadRequestException => (StatusCodes.Status400BadRequest, "Yêu cầu không hợp lệ"),
                BusinessException => (StatusCodes.Status409Conflict, "Vi phạm quy tắc nghiệp vụ"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Không có quyền truy cập"),
                _ => (StatusCodes.Status500InternalServerError, "Lỗi hệ thống")
            };

            if (statusCode >= 500)
            {
                _logger.LogError(exception, "Lỗi hệ thống tại {Path}. CorrelationId={CorrelationId}",
                    httpContext.Request.Path, correlationId);
            }
            else
            {
                _logger.LogWarning("Lỗi request tại {Path}: {Message}. CorrelationId={CorrelationId}",
                    httpContext.Request.Path, exception.Message, correlationId);
            }

            var response = new ProblemDetails
            {
                Status = statusCode,

                Title = title,

                Detail = exception.Message,

                Instance = httpContext.Request.Path,
                Extensions = { ["correlationId"] = correlationId }
            };

            httpContext.Response.StatusCode =
                statusCode;

            await httpContext.Response.WriteAsJsonAsync(
                response,
                cancellationToken
            );

            return true;
        }
    }
}