using System.Text.Json.Serialization;

namespace SmartMeter.Server.Responses;

[JsonConverter(typeof(JsonStringEnumConverter<ApiErrorCode>))]
public enum ApiErrorCode
{
    [JsonStringEnumMemberName("bad_request")]
    BadRequest,

    [JsonStringEnumMemberName("unauthorized")]
    Unauthorized,

    [JsonStringEnumMemberName("forbidden")]
    Forbidden,

    [JsonStringEnumMemberName("not_found")]
    NotFound,

    [JsonStringEnumMemberName("method_not_allowed")]
    MethodNotAllowed,

    [JsonStringEnumMemberName("unsupported_media_type")]
    UnsupportedMediaType,

    [JsonStringEnumMemberName("client_error")]
    ClientError,

    [JsonStringEnumMemberName("service_unavailable")]
    ServiceUnavailable,

    [JsonStringEnumMemberName("internal_error")]
    InternalError,
}
