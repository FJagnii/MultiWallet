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
        var (problem, statusCode, isError) = exception switch
        {
            WalletRepositoryException => ("Błąd danych portfeli", StatusCodes.Status500InternalServerError, true),
            ExchangeRatesRepositoryException => ("Błąd danych walut NBP", StatusCodes.Status500InternalServerError, true),
           
            WalletLockTimeoutException => ("Limit oczekiwania na portfel", StatusCodes.Status423Locked, false),
            WalletNotFoundException => (exception.Message, StatusCodes.Status404NotFound, false),
            CurrencyNotFoundException => (exception.Message, StatusCodes.Status400BadRequest, false),
            CurrencyNotInWalletException => (exception.Message, StatusCodes.Status400BadRequest, false),
            NotEnoughFundsException => (exception.Message, StatusCodes.Status400BadRequest, false),
            
            _ => ("Nieoczekiwany błąd", StatusCodes.Status500InternalServerError, true)
        };

        if (isError)
        {
            _logger.LogError(exception, "Złapano wyjątek w API");
        }
        else
        {
            _logger.LogWarning($"Złapano wyjątek w API :{exception.Message}");
        }

        var problemDetails = new ProblemDetails
        {
            Title = problem,
            Status = statusCode,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}