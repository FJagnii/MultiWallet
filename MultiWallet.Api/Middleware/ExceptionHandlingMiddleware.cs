using Microsoft.AspNetCore.Mvc;
using MultiWallet.Api.Exceptions;

namespace MultiWallet.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception e)
        {
            await HandleExceptionAsync(context, e);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Złapano wyjątek w API");

        var (problem, statusCode) = exception switch
        {
            WalletRepositoryException => ("Błąd danych portfeli", StatusCodes.Status500InternalServerError),
            ExchangeRatesRepositoryException => ("Błąd danych walut NBP", StatusCodes.Status500InternalServerError),
            WalletLockTimeoutException => ("Limit oczekiwania na portfel", StatusCodes.Status423Locked),
            _ => ("Nieoczekiwany błąd", StatusCodes.Status500InternalServerError)
        };

        var problemDetails = new ProblemDetails
        {
            Title = problem,
            Status = statusCode,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}