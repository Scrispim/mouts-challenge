using System.Net;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, type, error, detail) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                "ValidationError",
                "Invalid input data",
                string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage))),
            DomainException domainEx => (
                HttpStatusCode.UnprocessableEntity,
                "DomainError",
                "Business rule violation",
                domainEx.Message),
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "ResourceNotFound",
                "Resource not found",
                exception.Message),
            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                "InvalidOperation",
                "Invalid operation",
                exception.Message),
            _ => (
                HttpStatusCode.InternalServerError,
                "InternalError",
                "An unexpected error occurred",
                exception.Message)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = JsonSerializer.Serialize(new
        {
            type,
            error,
            detail
        });

        return context.Response.WriteAsync(response);
    }
}