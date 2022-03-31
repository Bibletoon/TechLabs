namespace SpringBootApiClientGenerator.AntlrParser.Models;

public class ApiController
{
    public string Name { get; set; }
    public string Route { get; set; }
    public List<ApiMethod> Methods { get; set; }
}