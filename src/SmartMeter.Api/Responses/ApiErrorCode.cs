using System.Text.Json.Serialization;

namespace SmartMeter.Api.Responses;

[JsonConverter(typeof(JsonStringEnumConverter<ApiErrorCode>))]
public enum ApiErrorCode
{
    [JsonStringEnumMemberName("internal_error")]
    InternalError,
}
