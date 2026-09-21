using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Application.Features.Auth;
using TerryCorner.Application.Features.Categories;
using TerryCorner.Application.Features.Customers;
using TerryCorner.Application.Features.Orders;
using TerryCorner.Application.Features.PaymentReceipts;
using TerryCorner.Application.Features.Products;
using TerryCorner.Application.Features.Users;

namespace TerryCorner.Api.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = Map(exception);

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            // Full technical detail goes to the server log only; the client gets a safe message.
            logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(exception, "Handled exception on {Path}: {Title}", httpContext.Request.Path, title);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private static (int StatusCode, string Title, string Detail) Map(Exception exception) => exception switch
    {
        ValidationException => (
            StatusCodes.Status400BadRequest,
            "One or more validation errors occurred.",
            "Please correct the highlighted fields and try again."),

        InvalidCredentialsException => (
            StatusCodes.Status401Unauthorized,
            "Invalid credentials.",
            "The email or password you entered is incorrect."),

        InvalidRefreshTokenException => (
            StatusCodes.Status401Unauthorized,
            "Session expired.",
            "Please sign in again."),

        OrderValidationException orderEx => (
            StatusCodes.Status400BadRequest,
            "Your order could not be placed.",
            orderEx.Message),

        DuplicateReceiptException dupEx => (
            StatusCodes.Status409Conflict,
            "Receipt already submitted.",
            dupEx.Message),

        ReceiptAlreadyReviewedException reviewedEx => (
            StatusCodes.Status409Conflict,
            "Already reviewed.",
            reviewedEx.Message),

        FileValidationException fileEx => (
            StatusCodes.Status400BadRequest,
            "We couldn't upload your receipt.",
            fileEx.Message),

        CategoryHasProductsException catEx => (
            StatusCodes.Status409Conflict,
            "Category still in use.",
            catEx.Message),

        ProductHasOrderHistoryException prodEx => (
            StatusCodes.Status409Conflict,
            "Product has order history.",
            prodEx.Message),

        NoCustomerProfileException profileEx => (
            StatusCodes.Status404NotFound,
            "No profile found.",
            profileEx.Message),

        CannotRestrictSelfException selfEx => (
            StatusCodes.Status400BadRequest,
            "Not allowed.",
            selfEx.Message),

        CannotChangeOwnRoleException roleEx => (
            StatusCodes.Status400BadRequest,
            "Not allowed.",
            roleEx.Message),

        UserNotFoundException userEx => (
            StatusCodes.Status404NotFound,
            "User not found.",
            userEx.Message),

        UnauthorizedAccessException => (
            StatusCodes.Status403Forbidden,
            "Forbidden.",
            "You don't have permission to perform this action."),

        KeyNotFoundException => (
            StatusCodes.Status404NotFound,
            "Not found.",
            "The requested resource could not be found."),

        _ => (
            StatusCodes.Status500InternalServerError,
            "An unexpected error occurred.",
            "Something went wrong. Please try again."),
    };
}
