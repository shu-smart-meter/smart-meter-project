using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;

using SmartMeter.Api.Responses;

namespace SmartMeter.Api.Filters;

// always-run so results from short circuiting filters (auth, 415 etc) get wrapped too
// why: https://learn.microsoft.com/aspnet/core/mvc/controllers/filters#ialwaysrunresultfilter-and-iasyncalwaysrunresultfilter
public sealed class ApiResponseResultFilter : IAlwaysRunResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is not ObjectResult result || IsEnvelope(result.Value))
        {
            return;
        }

        var status = result.StatusCode ?? StatusCodes.Status200OK;
        object envelope = status >= StatusCodes.Status400BadRequest
            ? ApiResponse.Failure(status, ErrorMessage(status, result.Value))
            : ApiResponse.Success(status, result.Value);

        result.Value = envelope;
        result.DeclaredType = envelope.GetType();
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private static bool IsEnvelope(object? value)
    {
        var type = value?.GetType();

        return type is { IsGenericType: true } && type.GetGenericTypeDefinition() == typeof(ApiResponse<>);
    }

    // [ApiController] turns errors into problem details before this runs, so pull the message back out of them
    private static string ErrorMessage(int status, object? value)
    {
        return value switch
        {
            ValidationProblemDetails validation when validation.Errors.Count > 0
                => validation.Errors.First().Value.First(),
            ProblemDetails problem => problem.Detail ?? problem.Title ?? ReasonPhrases.GetReasonPhrase(status),
            string text => text,
            _ => ReasonPhrases.GetReasonPhrase(status),
        };
    }
}
