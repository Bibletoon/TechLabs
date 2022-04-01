using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SpringBootApiClientGenerator.AntlrParser.Models;
using SpringBootApiClientGenerator.AntlrParser.Tools;

namespace SpringBootApiClientGenerator.AntlrParser;

public class SpringControllerVisitor : JavaParserBaseVisitor<ApiController>
{
    private static readonly Regex MethodAnnotationRegex =
        new Regex(@"@(?<annotationType>\w+)Mapping\(.*value\s*=\s*""(?<path>[a-zA-Z0-9\/_-{}]*)"",*\s*(method\s*=\s*RequestMethod\.(?<RequestMethod>[a-zA-Z]+))?.*\)", RegexOptions.Compiled);

    public override ApiController VisitTypeDeclaration(JavaParser.TypeDeclarationContext context)
    {
        var annotations = context.children.OfType<JavaParser.ClassOrInterfaceModifierContext>().SelectMany(c => c.children)
               .OfType<JavaParser.AnnotationContext>();

        if (annotations.All(a => a.GetText() != "@RestController"))
            return null;

        string controllerName = context.children.OfType<JavaParser.ClassDeclarationContext>().Single().GetChild(1)
                                       .GetText();
        
        string controllerRoute = (annotations.FirstOrDefault(a => a.GetChild(1).GetText() == "RequestMapping")
                                             ?.children.OfType<JavaParser.ElementValuePairsContext>())?.Single(p => p.Start.Text == "value").Stop.Text.Replace("\"","") ?? string.Empty;

        var methods = context.children.OfType<JavaParser.ClassDeclarationContext>().Single().children
                             .OfType<JavaParser.ClassBodyContext>().Single().children
                             .OfType<JavaParser.ClassBodyDeclarationContext>().SelectMany(c => c.children)
                             .OfType<JavaParser.MemberDeclarationContext>().SelectMany(c => c.children)
                             .OfType<JavaParser.MethodDeclarationContext>();

        var apiMethods = methods.Select(ParseApiMethod).Where(m => m is not null).ToList();
        
        var controller = new ApiController()
        {
            Name = controllerName,
            Route = controllerRoute,
            Methods = apiMethods,
        };
        
        return controller;
    }

    public ApiMethod? ParseApiMethod(JavaParser.MethodDeclarationContext context)
    {
        if (context.Parent.Parent is not JavaParser.ClassBodyDeclarationContext p)
            return null;

        var methodAttributes = p.children.OfType<JavaParser.ModifierContext>().Where(mc => mc.Start.Text == "@");

        (HttpMethod httpMethod, string path) = ParseApiMethodAnnotation(methodAttributes);

        string returnType = context.GetChild(0).GetText();
        string name = context.GetChild(1).GetText();
        
        var methodParameters = context.children.OfType<JavaParser.FormalParametersContext>().Single().children
                                      .OfType<JavaParser.FormalParameterListContext>();

        List<ApiMethodParameter> parameters;

        if (!methodParameters.Any())
            parameters = new List<ApiMethodParameter>();
        else
        {
            parameters = methodParameters.Single().children
                                         .OfType<JavaParser.FormalParameterContext>()
                                         .Select(ParseApiMethodParameters)
                                         .ToList();
        }

        returnType = StaticTypeProvider.RegisterType(returnType);
        
        return new ApiMethod()
        {
            Name = name,
            Path = path,
            HttpMethod = httpMethod,
            ReturnType = returnType,
            Parameters = parameters
        };
    }

    public (HttpMethod, string) ParseApiMethodAnnotation(IEnumerable<JavaParser.ModifierContext> contexts)
    {
        var c = contexts.First(c => c.GetChild(0).GetChild(0).GetChild(1).GetText().EndsWith("Mapping")).GetChild(0).GetChild(0);

        var match = MethodAnnotationRegex.Match(c.GetText());

        HttpMethod method = match.Groups["annotationType"].Value == "Request" 
            ? match.Groups["RequestMethod"].Value.ToHttpMethod() 
            : match.Groups["annotationType"].Value.ToHttpMethod();

        string path = match.Groups["path"].Value;    
        
        return (method, path);
    }

    public ApiMethodParameter ParseApiMethodParameters(JavaParser.FormalParameterContext context)
    {
        var annotations = context.children.OfType<JavaParser.VariableModifierContext>()
                                 .SelectMany(c => c.children)
                                 .OfType<JavaParser.AnnotationContext>();

        string parameterName = context.children.OfType<JavaParser.VariableDeclaratorIdContext>().Single().GetText();
        string parameterType = context.children.OfType<JavaParser.TypeTypeContext>().Single().GetText();
        ApiMethodParameterType methodParameterType = ApiMethodParameterType.Query;

        foreach (var annotation in annotations)
        {
            Match match = Regex.Match(annotation.GetText(), @"@PathVariable(\(.*value\s*=\s*""(?<customName>\w*)"".*\))?");
            if (match.Success)
            {
                methodParameterType = ApiMethodParameterType.Path;

                if (!string.IsNullOrWhiteSpace(match.Groups["customName"].Value))
                    parameterName = match.Groups["customName"].Value;
                break;
            }

            match = Regex.Match(annotation.GetText(), @"@RequestBody");

            if (match.Success)
            {
                methodParameterType = ApiMethodParameterType.Body;
                break;
            }

            match = Regex.Match(annotation.GetText(), @"@RequestParam");

            if (match.Success)
            {
                methodParameterType = ApiMethodParameterType.Query;
                break;
            }
        }
        
        parameterType = StaticTypeProvider.RegisterType(parameterType);
        
        return new ApiMethodParameter()
        {
            Name = parameterName,
            Type = parameterType,
            ParameterType = methodParameterType,
        };
    }
}