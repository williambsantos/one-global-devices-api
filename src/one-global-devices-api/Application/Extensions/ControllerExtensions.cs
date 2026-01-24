using Microsoft.AspNetCore.Mvc;

namespace OneGlobalDevicesApi.Application.Extensions
{
    /// <summary>
    /// Extension methods for working with RFC 9457 Problem Details in ASP.NET Core controllers.
    /// </summary>
    public static class ControllerExtensions
    {
        public static ObjectResult BadRequestProblem(this ControllerBase controller,
            string detail, string? instance = null, Dictionary<string, object?>? extensions = null
        )
            => controller.ProblemDetails(
                StatusCodes.Status400BadRequest,
                detail, instance, extensions
            );

        public static ObjectResult NotFoundProblem(this ControllerBase controller,
            string detail, string? instance = null, Dictionary<string, object?>? extensions = null
        )
            => controller.ProblemDetails(
                StatusCodes.Status404NotFound,
                detail, instance, extensions
            );

        public static ObjectResult ProblemDetails(this ControllerBase controller, int statusCode, string detail, string? instance = null, Dictionary<string, object?>? extensions = null)
        {
            instance ??= GetProblemDetailsDefaultInstance(controller);
            var title = GetProblemDetailsTitle(statusCode);
            var type = GetProblemDetailsType(statusCode);

            return controller.Problem(
                detail: detail,
                instance: instance,
                statusCode: statusCode,
                title: title,
                type: type,
                extensions: extensions
            );
        }

        private static string? GetProblemDetailsDefaultInstance(ControllerBase controller)
            => controller.HttpContext?.Request?.Path.ToString();

        private static string GetProblemDetailsType(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                StatusCodes.Status500InternalServerError => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc9110#section-15.5.8",
                StatusCodes.Status422UnprocessableEntity => "https://tools.ietf.org/html/rfc4918#section-11.2",
                _ => "about:blank",
            };
        }

        private static string GetProblemDetailsTitle(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "Bad Request",
                StatusCodes.Status404NotFound => "Not Found",
                StatusCodes.Status500InternalServerError => "Internal Server Error",
                StatusCodes.Status409Conflict => "Conflict",
                StatusCodes.Status422UnprocessableEntity => "Unprocessable Entity",
                _ => "Unknown",
            };
        }
    }
}
