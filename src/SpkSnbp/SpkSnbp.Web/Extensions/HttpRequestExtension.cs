namespace SpkSnbp.Web.Extensions;

public static class HttpRequestExtension
{
    public static string AbsoluteUri(this HttpRequest request) => new UriBuilder(
            request.Scheme, 
            request.Host.Host, 
            request.Host.Port ?? -1, 
            request.Path.ToString(), 
            request.QueryString.ToString()).Uri.AbsoluteUri;
}
