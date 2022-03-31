using System.Text.RegularExpressions;
using Antlr4.Runtime;
using SpringBootApiClientGenerator.AntlrParser;
using SpringBootApiClientGenerator.AntlrParser.Models;
using SpringBootApiClientGenerator.Generator;

var math = Regex.Match(Directory.GetCurrentDirectory(), @"[\\\/a-zA-Z\-_0-9:]+bibletoon\\Lab-2\\");

string basePath = math.Value;

string serverPath = Path.Combine(basePath, @"Server\spring-boot-api-example\src\main\java\com\tomgregory\");
string codePath = Path.Combine(basePath, @"SpringBootApiClientGenerator\SpringBootApiClientGenerator");

List<string> files = Directory.EnumerateFiles(serverPath, "*.java", SearchOption.AllDirectories).ToList();

List<string> controllerPaths = files.Where(f => f.EndsWith("Controller.java")).ToList();

List<ApiController> controllers = new List<ApiController>();

foreach (var controllerPath in controllerPaths)
{
    string controllerText = File.ReadAllText(controllerPath);

    AntlrInputStream inputStream = new AntlrInputStream(controllerText);
    JavaLexer javaLexer = new JavaLexer(inputStream);
    CommonTokenStream commonTokenStream = new CommonTokenStream(javaLexer);
    JavaParser javaParser = new JavaParser(commonTokenStream);

    SpringControllerVisitor visitor = new SpringControllerVisitor();

    var controller = visitor.Visit(javaParser.compilationUnit());
    controllers.Add(controller);
}

var models = new List<ApiModel>();

while (StaticTypeProvider.TypesToGenerate.Any())
{
    var typeName = StaticTypeProvider.GetTypeToGenerate();
    var fileName = files.First(f => f.EndsWith($"{typeName}.java"));
    
    string modelText = File.ReadAllText(fileName);

    AntlrInputStream inputStream = new AntlrInputStream(modelText);
    JavaLexer javaLexer = new JavaLexer(inputStream);
    CommonTokenStream commonTokenStream = new CommonTokenStream(javaLexer);
    JavaParser javaParser = new JavaParser(commonTokenStream);

    ClassVisitor visitor = new ClassVisitor();
    models.Add(visitor.Visit(javaParser.compilationUnit()));
}

var result = ApiClientGenerator.Generate("apiClient", models, controllers);

File.WriteAllText(Path.Combine(codePath, "ApiClient.cs"), result);