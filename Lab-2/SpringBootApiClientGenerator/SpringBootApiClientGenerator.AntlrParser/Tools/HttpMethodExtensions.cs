namespace SpringBootApiClientGenerator.AntlrParser.Tools;

public static class HttpMethodExtensions
{
    public static HttpMethod ToHttpMethod(this string str) =>
        str.ToUpper() switch
        {
            "GET" => HttpMethod.Get,
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            "PATCH" => HttpMethod.Patch,
            "DELETE" => HttpMethod.Delete
        };
}