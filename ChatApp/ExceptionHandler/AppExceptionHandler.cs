using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;

namespace ChatApp.ExceptionHandler
{
    public class AppExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if(exception is NotImplementedException)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "An error occurred",
                    Status = StatusCodes.Status501NotImplemented,
                    Detail = exception.Message
                };
                httpContext.Response.StatusCode = problemDetails.Status.Value;
                await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            }
            else if (exception is DbException)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "An error occurred while saving changes in database",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = exception.Message
                };
                httpContext.Response.StatusCode = problemDetails.Status.Value;
                await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            }
            else
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "An error occurred",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = exception.Message
                };
                httpContext.Response.StatusCode = problemDetails.Status.Value;
                await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            }
            return true;
        }
    }
}
