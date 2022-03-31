namespace SpringBootApiClientGenerator.AntlrParser.Models;

public class ApiModel
{
    public string Name { get; set; }
    public List<(string, string)> Fields { get; set; }
}