using Microsoft.AspNetCore.Mvc;

namespace OneGlobalDevicesApi.Application.Extensions
{
    /// <summary>
    /// Extension methods for working with RFC 9457 Problem Details in ASP.NET Core controllers.
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Gets the current request path, or null if HttpContext is not available (e.g., in unit tests).
        /// </summary>
        private static string? GetRequestPath(this ControllerBase controller)
        {
            return controller.HttpContext?.Request?.Path.ToString();
        }

        /// <summary>
        /// Creates an ObjectResult with the specified ProblemDetails and sets the appropriate status code.
        /// </summary>
        public static ObjectResult Problem(this ControllerBase controller, Domain.Common.ProblemDetails problemDetails)
        {
            return new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status,
                ContentTypes = { "application/problem+json" }
            };
        }

        /// <summary>
        /// Creates a Bad Request (400) response with RFC 9457 Problem Details.
        /// </summary>
        public static ObjectResult BadRequestProblem(this ControllerBase controller, string detail, string? instance = null, Dictionary<string, object>? extensions = null)
        {
            instance ??= controller.GetRequestPath();
            var problemDetails = Domain.Common.ProblemDetails.BadRequest(detail, instance, extensions);
            return controller.Problem(problemDetails);
        }

        /// <summary>
        /// Creates a Not Found (404) response with RFC 9457 Problem Details.
        /// </summary>
        public static ObjectResult NotFoundProblem(this ControllerBase controller, string detail, string? instance = null, Dictionary<string, object>? extensions = null)
        {
            instance ??= controller.GetRequestPath();
            var problemDetails = Domain.Common.ProblemDetails.NotFound(detail, instance, extensions);
            return controller.Problem(problemDetails);
        }

        /// <summary>
        /// Creates an Internal Server Error (500) response with RFC 9457 Problem Details.
        /// </summary>
        public static ObjectResult InternalServerErrorProblem(this ControllerBase controller, string detail, string? instance = null, Dictionary<string, object>? extensions = null)
        {
            instance ??= controller.GetRequestPath();
            var problemDetails = Domain.Common.ProblemDetails.InternalServerError(detail, instance, extensions);
            return controller.Problem(problemDetails);
        }

        /// <summary>
        /// Creates a Conflict (409) response with RFC 9457 Problem Details.
        /// </summary>
        public static ObjectResult ConflictProblem(this ControllerBase controller, string detail, string? instance = null, Dictionary<string, object>? extensions = null)
        {
            instance ??= controller.GetRequestPath();
            var problemDetails = Domain.Common.ProblemDetails.Conflict(detail, instance, extensions);
            return controller.Problem(problemDetails);
        }

        /// <summary>
        /// Creates an Unprocessable Entity (422) response with RFC 9457 Problem Details.
        /// </summary>
        public static ObjectResult UnprocessableEntityProblem(this ControllerBase controller, string detail, string? instance = null, Dictionary<string, object>? extensions = null)
        {
            instance ??= controller.GetRequestPath();
            var problemDetails = Domain.Common.ProblemDetails.UnprocessableEntity(detail, instance, extensions);
            return controller.Problem(problemDetails);
        }
    }
}
