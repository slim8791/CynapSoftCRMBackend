using System.Net;

namespace Cynapharm_Mobile.Services;

public class ApiException : Exception
{
    public HttpStatusCode? StatusCode { get; }

    public ApiException(string message, Exception? inner = null, HttpStatusCode? statusCode = null)
        : base(message, inner) => StatusCode = statusCode;
}
