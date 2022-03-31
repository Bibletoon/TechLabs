namespace SpringBootApiClientGenerator.AntlrParser.Models;

public class ApiMethod
{
    public string Name { get; set; }
    public HttpMethod HttpMethod { get; set; }
    public string Path { get; set; }
    public string ReturnType { get; set; }
    public List<ApiMethodParameter> Parameters { get; set; }
}