using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SpringBootApiClientGenerator.AntlrParser.Models;

namespace SpringBootApiClientGenerator.Generator;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

public class ApiClientGenerator
{
    public static string Generate(string namespaceName, List<ApiModel> models, List<ApiController> controllers)
    {
        var generatedModels = GenerateApiModels(models);
        var modelsString = string.Join('\n', generatedModels.Select(m => m.NormalizeWhitespace().ToString()));

        var clientsString = string.Join('\n', GenerateApiClients(controllers));

        return $@"using System.Net.Http.Json;

                namespace {namespaceName};

                {modelsString}

                {clientsString}
                ";
    }

    private static List<ClassDeclarationSyntax> GenerateApiModels(List<ApiModel> models)
    {
        List<ClassDeclarationSyntax> generatedModels = new List<ClassDeclarationSyntax>();
        
        foreach (var model in models)
        {
            var members = new List<MemberDeclarationSyntax>();

            foreach (var field in model.Fields)
            {
                members.Add(
                    PropertyDeclaration(
                            IdentifierName(field.Item1),
                            Identifier(field.Item2)
                        )
                        .WithModifiers(
                            TokenList(
                                Token(SyntaxKind.PublicKeyword)))
                        .WithAccessorList(
                            AccessorList(
                                List<AccessorDeclarationSyntax>(
                                    new AccessorDeclarationSyntax[]
                                    {
                                        AccessorDeclaration(
                                                SyntaxKind.GetAccessorDeclaration)
                                            .WithSemicolonToken(
                                                Token(SyntaxKind.SemicolonToken)),
                                        AccessorDeclaration(
                                                SyntaxKind.SetAccessorDeclaration)
                                            .WithSemicolonToken(
                                                Token(SyntaxKind.SemicolonToken))
                                    }))));
            }

            var generatedModel = ClassDeclaration(model.Name)
                                 .WithModifiers(
                                     TokenList(
                                         Token(SyntaxKind.PublicKeyword)))
                                 .WithMembers(new SyntaxList<MemberDeclarationSyntax>(members))
                                 .NormalizeWhitespace();

            generatedModels.Add(generatedModel);
        }

        return generatedModels;
    }

    private static List<string> GenerateApiClients(List<ApiController> apiControllers)
    {
        List<string> clients = new List<string>();
        
        foreach (var apiController in apiControllers)
        {
            var members = new List<MemberDeclarationSyntax>();

            foreach (var endpoint in apiController.Methods)
            {
                if (endpoint.ReturnType == "void")
                    members.Add(GenerateVoidMethod(endpoint));
                else
                {
                    members.Add(GenerateNotVoidMethod(endpoint));
                }
            }

            var membersString = string.Join('\n', members.Select(m => m.NormalizeWhitespace()));

            var client = $@"public class {apiController.Name} {{
                            private readonly System.Net.Http.HttpClient _httpClient;
                            
                            public {apiController.Name}(HttpClient httpClient) {{
                                _httpClient = httpClient;
                            }}

                            {membersString}

                            private string ConvertToString(object? value, System.Globalization.CultureInfo cultureInfo)
                            {{
                                if (value is null)
                                {{
                                    return """";
                                }}

                                if (value is System.Enum)
                                {{
                                    var name = System.Enum.GetName(value.GetType(), value);
                                    if (name is null)
                                    {{
                                        var field = System.Reflection.IntrospectionExtensions.GetTypeInfo(value.GetType()).GetDeclaredField(name);
                                        if (field is null)
                                        {{
                                            var attribute = System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(System.Runtime.Serialization.EnumMemberAttribute)) 
                                                as System.Runtime.Serialization.EnumMemberAttribute;
                                            if (attribute is null)
                                            {{
                                                return attribute.Value is null ? attribute.Value : name;
                                            }}
                                        }}

                                        var converted = System.Convert.ToString(System.Convert.ChangeType(value, System.Enum.GetUnderlyingType(value.GetType()), cultureInfo));
                                        return converted is null ? string.Empty : converted;
                                    }}
                                }}
                                else if (value is bool b) 
                                {{
                                    return System.Convert.ToString(b, cultureInfo).ToLowerInvariant();
                                }}
                                else if (value is byte[] bytes)
                                {{
                                    return System.Convert.ToBase64String(bytes);
                                }}
                                else if (value.GetType().IsArray)
                                {{
                                    var array = System.Linq.Enumerable.OfType<object>((System.Array) value);
                                    return string.Join("","", System.Linq.Enumerable.Select(array, o => ConvertToString(o, cultureInfo)));
                                }}

                                var result = System.Convert.ToString(value, cultureInfo);
                                return result ?? """";
                            }}
                        }}";
            
            clients.Add(client);
        }

        return clients;
    }

    private static MemberDeclarationSyntax GenerateVoidMethod(ApiMethod endpoint)
    {
        var parameters = new List<SyntaxNodeOrToken>();

        foreach (var parameter in endpoint.Parameters)
        {
            if (parameters.Any())
                parameters.Add(Token(SyntaxKind.CommaToken));
            
            parameters.Add(Parameter(
                                   Identifier(parameter.Name))
                               .WithType(
                                   IdentifierName(parameter.Type)));
        }

        var body = new List<StatementSyntax>()
        {
            LocalDeclarationStatement(
                           VariableDeclaration(
                                   IdentifierName(
                                       Identifier(
                                           TriviaList(),
                                           SyntaxKind.VarKeyword,
                                           "var",
                                           "var",
                                           TriviaList())))
                               .WithVariables(
                                   SingletonSeparatedList<VariableDeclaratorSyntax>(
                                       VariableDeclarator(
                                               Identifier("urlBuilder"))
                                           .WithInitializer(
                                               EqualsValueClause(
                                                   ObjectCreationExpression(
                                                           QualifiedName(
                                                               QualifiedName(
                                                                   IdentifierName("System"),
                                                                   IdentifierName("Text")),
                                                               IdentifierName("StringBuilder")))
                                                       .WithArgumentList(
                                                           ArgumentList())))))),
                       ExpressionStatement(
                           InvocationExpression(
                                   MemberAccessExpression(
                                       SyntaxKind.SimpleMemberAccessExpression,
                                       InvocationExpression(
                                               MemberAccessExpression(
                                                   SyntaxKind.SimpleMemberAccessExpression,
                                                   IdentifierName("urlBuilder"),
                                                   IdentifierName("Append")))
                                           .WithArgumentList(
                                               ArgumentList(
                                                   SingletonSeparatedList<ArgumentSyntax>(
                                                       Argument(
                                                           InterpolatedStringExpression(
                                                                   Token(SyntaxKind.InterpolatedStringStartToken))
                                                               .WithContents(
                                                                   SingletonList<InterpolatedStringContentSyntax>(
                                                                       InterpolatedStringText()
                                                                           .WithTextToken(
                                                                               Token(
                                                                                   TriviaList(),
                                                                                   SyntaxKind.InterpolatedStringTextToken,
                                                                                   endpoint.Path,
                                                                                   endpoint.Path,
                                                                                   TriviaList())))))))),
                                       IdentifierName("Append")))
                               .WithArgumentList(
                                   ArgumentList(
                                       SingletonSeparatedList<ArgumentSyntax>(
                                           Argument(
                                               LiteralExpression(
                                                   SyntaxKind.CharacterLiteralExpression,
                                                   Literal('?')))))))
        };

        foreach (var param in endpoint.Parameters.Where(p => p.ParameterType == ApiMethodParameterType.Query))
        {
            body.Add(GenerateQueryParamAdd(param));
        }
        
        body.AddRange(new StatementSyntax[]
        {
            LocalDeclarationStatement(
                               VariableDeclaration(
                                       IdentifierName(
                                           Identifier(
                                               TriviaList(),
                                               SyntaxKind.VarKeyword,
                                               "var",
                                               "var",
                                               TriviaList())))
                                   .WithVariables(
                                       SingletonSeparatedList<VariableDeclaratorSyntax>(
                                           VariableDeclarator(
                                                   Identifier("request"))
                                               .WithInitializer(
                                                   EqualsValueClause(
                                                       ObjectCreationExpression(
                                                               QualifiedName(
                                                                   QualifiedName(
                                                                       QualifiedName(
                                                                           IdentifierName("System"),
                                                                           IdentifierName("Net")),
                                                                       IdentifierName("Http")),
                                                                   IdentifierName("HttpRequestMessage")))
                                                           .WithArgumentList(
                                                               ArgumentList()))))))
                           .WithUsingKeyword(
                               Token(SyntaxKind.UsingKeyword)),
                       ExpressionStatement(
                           AssignmentExpression(
                               SyntaxKind.SimpleAssignmentExpression,
                               MemberAccessExpression(
                                   SyntaxKind.SimpleMemberAccessExpression,
                                   IdentifierName("request"),
                                   IdentifierName("Method")),
                               ObjectCreationExpression(
                                       IdentifierName("HttpMethod"))
                                   .WithArgumentList(
                                       ArgumentList(
                                           SingletonSeparatedList<ArgumentSyntax>(
                                               Argument(
                                                   LiteralExpression(
                                                       SyntaxKind.StringLiteralExpression,
                                                       Literal(endpoint.HttpMethod.ToString().ToUpper())))))))),
                       ExpressionStatement(
                           InvocationExpression(
                                   MemberAccessExpression(
                                       SyntaxKind.SimpleMemberAccessExpression,
                                       MemberAccessExpression(
                                           SyntaxKind.SimpleMemberAccessExpression,
                                           MemberAccessExpression(
                                               SyntaxKind.SimpleMemberAccessExpression,
                                               IdentifierName("request"),
                                               IdentifierName("Headers")),
                                           IdentifierName("Accept")),
                                       IdentifierName("Add")))
                               .WithArgumentList(
                                   ArgumentList(
                                       SingletonSeparatedList<ArgumentSyntax>(
                                           Argument(
                                               InvocationExpression(
                                                       MemberAccessExpression(
                                                           SyntaxKind.SimpleMemberAccessExpression,
                                                           MemberAccessExpression(
                                                               SyntaxKind.SimpleMemberAccessExpression,
                                                               MemberAccessExpression(
                                                                   SyntaxKind.SimpleMemberAccessExpression,
                                                                   MemberAccessExpression(
                                                                       SyntaxKind.SimpleMemberAccessExpression,
                                                                       MemberAccessExpression(
                                                                           SyntaxKind.SimpleMemberAccessExpression,
                                                                           IdentifierName("System"),
                                                                           IdentifierName("Net")),
                                                                       IdentifierName("Http")),
                                                                   IdentifierName("Headers")),
                                                               IdentifierName("MediaTypeWithQualityHeaderValue")),
                                                           IdentifierName("Parse")))
                                                   .WithArgumentList(
                                                       ArgumentList(
                                                           SingletonSeparatedList<ArgumentSyntax>(
                                                               Argument(
                                                                   LiteralExpression(
                                                                       SyntaxKind.StringLiteralExpression,
                                                                       Literal("application/json"))))))))))),
                       ExpressionStatement(
                           AssignmentExpression(
                               SyntaxKind.SimpleAssignmentExpression,
                               MemberAccessExpression(
                                   SyntaxKind.SimpleMemberAccessExpression,
                                   IdentifierName("request"),
                                   IdentifierName("RequestUri")),
                               ObjectCreationExpression(
                                       QualifiedName(
                                           IdentifierName("System"),
                                           IdentifierName("Uri")))
                                   .WithArgumentList(
                                       ArgumentList(
                                           SeparatedList<ArgumentSyntax>(
                                               new SyntaxNodeOrToken[]
                                               {
                                                   Argument(
                                                       InvocationExpression(
                                                           MemberAccessExpression(
                                                               SyntaxKind.SimpleMemberAccessExpression,
                                                               IdentifierName("urlBuilder"),
                                                               IdentifierName("ToString")))),
                                                   Token(SyntaxKind.CommaToken),
                                                   Argument(
                                                       MemberAccessExpression(
                                                           SyntaxKind.SimpleMemberAccessExpression,
                                                           MemberAccessExpression(
                                                               SyntaxKind.SimpleMemberAccessExpression,
                                                               IdentifierName("System"),
                                                               IdentifierName("UriKind")),
                                                           IdentifierName("RelativeOrAbsolute")))
                                               }))))),
                       ExpressionStatement(
                           AwaitExpression(
                               InvocationExpression(
                                       MemberAccessExpression(
                                           SyntaxKind.SimpleMemberAccessExpression,
                                           IdentifierName("_httpClient"),
                                           IdentifierName("SendAsync")))
                                   .WithArgumentList(
                                       ArgumentList(
                                           SeparatedList<ArgumentSyntax>(
                                               new SyntaxNodeOrToken[]
                                               {
                                                   Argument(
                                                       IdentifierName("request")),
                                                   Token(SyntaxKind.CommaToken),
                                                   Argument(
                                                       MemberAccessExpression(
                                                           SyntaxKind.SimpleMemberAccessExpression,
                                                           IdentifierName("HttpCompletionOption"),
                                                           IdentifierName("ResponseContentRead")))
                                               })))))
        });

        var bodyParam = endpoint.Parameters.FirstOrDefault(p => p.ParameterType == ApiMethodParameterType.Body);

        if (bodyParam is not null)
        {
            body.Insert(body.Count-2, ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("request"),
                                    IdentifierName("Content")),
                                InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("JsonContent"),
                                            IdentifierName("Create")))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList<ArgumentSyntax>(
                                                Argument(
                                                    IdentifierName("BODY_ARGUMENT"))))))));
        }
        
        return MethodDeclaration(
                   IdentifierName("Task"),
                   Identifier(endpoint.Name))
               .WithModifiers(
                   TokenList(
                       new[]
                       {
                           Token(SyntaxKind.PublicKeyword),
                           Token(SyntaxKind.AsyncKeyword)
                       }))
               .WithParameterList(
                   ParameterList(
                       SeparatedList<ParameterSyntax>(parameters)))
               .WithBody(
                   Block(body));
    }

    private static MemberDeclarationSyntax GenerateNotVoidMethod(ApiMethod endpoint)
    {
        var parameters = new List<SyntaxNodeOrToken>();

        foreach (var parameter in endpoint.Parameters)
        {
            if (parameters.Any())
                parameters.Add(Token(SyntaxKind.CommaToken));
            
            parameters.Add(Parameter(
                                   Identifier(parameter.Name))
                               .WithType(
                                   IdentifierName(parameter.Type)));
        }

        var body = new List<StatementSyntax>()
        {
            LocalDeclarationStatement(
                           VariableDeclaration(
                                   IdentifierName(
                                       Identifier(
                                           TriviaList(),
                                           SyntaxKind.VarKeyword,
                                           "var",
                                           "var",
                                           TriviaList())))
                               .WithVariables(
                                   SingletonSeparatedList<VariableDeclaratorSyntax>(
                                       VariableDeclarator(
                                               Identifier("urlBuilder"))
                                           .WithInitializer(
                                               EqualsValueClause(
                                                   ObjectCreationExpression(
                                                           QualifiedName(
                                                               QualifiedName(
                                                                   IdentifierName("System"),
                                                                   IdentifierName("Text")),
                                                               IdentifierName("StringBuilder")))
                                                       .WithArgumentList(
                                                           ArgumentList())))))),
                       ExpressionStatement(
                           InvocationExpression(
                                   MemberAccessExpression(
                                       SyntaxKind.SimpleMemberAccessExpression,
                                       InvocationExpression(
                                               MemberAccessExpression(
                                                   SyntaxKind.SimpleMemberAccessExpression,
                                                   IdentifierName("urlBuilder"),
                                                   IdentifierName("Append")))
                                           .WithArgumentList(
                                               ArgumentList(
                                                   SingletonSeparatedList<ArgumentSyntax>(
                                                       Argument(
                                                           InterpolatedStringExpression(
                                                                   Token(SyntaxKind.InterpolatedStringStartToken))
                                                               .WithContents(
                                                                   SingletonList<InterpolatedStringContentSyntax>(
                                                                       InterpolatedStringText()
                                                                           .WithTextToken(
                                                                               Token(
                                                                                   TriviaList(),
                                                                                   SyntaxKind.InterpolatedStringTextToken,
                                                                                   endpoint.Path,
                                                                                   endpoint.Path,
                                                                                   TriviaList())))))))),
                                       IdentifierName("Append")))
                               .WithArgumentList(
                                   ArgumentList(
                                       SingletonSeparatedList<ArgumentSyntax>(
                                           Argument(
                                               LiteralExpression(
                                                   SyntaxKind.CharacterLiteralExpression,
                                                   Literal('?')))))))
        };

        foreach (var param in endpoint.Parameters.Where(p => p.ParameterType == ApiMethodParameterType.Query))
        {
            body.Add(GenerateQueryParamAdd(param));
        }
        
        body.AddRange(new StatementSyntax[]
        {
            LocalDeclarationStatement(
                               VariableDeclaration(
                                       IdentifierName(
                                           Identifier(
                                               TriviaList(),
                                               SyntaxKind.VarKeyword,
                                               "var",
                                               "var",
                                               TriviaList())))
                                   .WithVariables(
                                       SingletonSeparatedList<VariableDeclaratorSyntax>(
                                           VariableDeclarator(
                                                   Identifier("request"))
                                               .WithInitializer(
                                                   EqualsValueClause(
                                                       ObjectCreationExpression(
                                                               QualifiedName(
                                                                   QualifiedName(
                                                                       QualifiedName(
                                                                           IdentifierName("System"),
                                                                           IdentifierName("Net")),
                                                                       IdentifierName("Http")),
                                                                   IdentifierName("HttpRequestMessage")))
                                                           .WithArgumentList(
                                                               ArgumentList()))))))
                           .WithUsingKeyword(
                               Token(SyntaxKind.UsingKeyword)),
                       ExpressionStatement(
                           AssignmentExpression(
                               SyntaxKind.SimpleAssignmentExpression,
                               MemberAccessExpression(
                                   SyntaxKind.SimpleMemberAccessExpression,
                                   IdentifierName("request"),
                                   IdentifierName("Method")),
                               ObjectCreationExpression(
                                       IdentifierName("HttpMethod"))
                                   .WithArgumentList(
                                       ArgumentList(
                                           SingletonSeparatedList<ArgumentSyntax>(
                                               Argument(
                                                   LiteralExpression(
                                                       SyntaxKind.StringLiteralExpression,
                                                       Literal(endpoint.HttpMethod.ToString().ToUpper())))))))),
                       ExpressionStatement(
                           InvocationExpression(
                                   MemberAccessExpression(
                                       SyntaxKind.SimpleMemberAccessExpression,
                                       MemberAccessExpression(
                                           SyntaxKind.SimpleMemberAccessExpression,
                                           MemberAccessExpression(
                                               SyntaxKind.SimpleMemberAccessExpression,
                                               IdentifierName("request"),
                                               IdentifierName("Headers")),
                                           IdentifierName("Accept")),
                                       IdentifierName("Add")))
                               .WithArgumentList(
                                   ArgumentList(
                                       SingletonSeparatedList<ArgumentSyntax>(
                                           Argument(
                                               InvocationExpression(
                                                       MemberAccessExpression(
                                                           SyntaxKind.SimpleMemberAccessExpression,
                                                           MemberAccessExpression(
                                                               SyntaxKind.SimpleMemberAccessExpression,
                                                               MemberAccessExpression(
                                                                   SyntaxKind.SimpleMemberAccessExpression,
                                                                   MemberAccessExpression(
                                                                       SyntaxKind.SimpleMemberAccessExpression,
                                                                       MemberAccessExpression(
                                                                           SyntaxKind.SimpleMemberAccessExpression,
                                                                           IdentifierName("System"),
                                                                           IdentifierName("Net")),
                                                                       IdentifierName("Http")),
                                                                   IdentifierName("Headers")),
                                                               IdentifierName("MediaTypeWithQualityHeaderValue")),
                                                           IdentifierName("Parse")))
                                                   .WithArgumentList(
                                                       ArgumentList(
                                                           SingletonSeparatedList<ArgumentSyntax>(
                                                               Argument(
                                                                   LiteralExpression(
                                                                       SyntaxKind.StringLiteralExpression,
                                                                       Literal("application/json"))))))))))),
                       ExpressionStatement(
                           AssignmentExpression(
                               SyntaxKind.SimpleAssignmentExpression,
                               MemberAccessExpression(
                                   SyntaxKind.SimpleMemberAccessExpression,
                                   IdentifierName("request"),
                                   IdentifierName("RequestUri")),
                               ObjectCreationExpression(
                                       QualifiedName(
                                           IdentifierName("System"),
                                           IdentifierName("Uri")))
                                   .WithArgumentList(
                                       ArgumentList(
                                           SeparatedList<ArgumentSyntax>(
                                               new SyntaxNodeOrToken[]
                                               {
                                                   Argument(
                                                       InvocationExpression(
                                                           MemberAccessExpression(
                                                               SyntaxKind.SimpleMemberAccessExpression,
                                                               IdentifierName("urlBuilder"),
                                                               IdentifierName("ToString")))),
                                                   Token(SyntaxKind.CommaToken),
                                                   Argument(
                                                       MemberAccessExpression(
                                                           SyntaxKind.SimpleMemberAccessExpression,
                                                           MemberAccessExpression(
                                                               SyntaxKind.SimpleMemberAccessExpression,
                                                               IdentifierName("System"),
                                                               IdentifierName("UriKind")),
                                                           IdentifierName("RelativeOrAbsolute")))
                                               }))))),
                       LocalDeclarationStatement(
                            VariableDeclaration(
                                IdentifierName(
                                    Identifier(
                                        TriviaList(),
                                        SyntaxKind.VarKeyword,
                                        "var",
                                        "var",
                                        TriviaList())))
                            .WithVariables(
                                SingletonSeparatedList<VariableDeclaratorSyntax>(
                                    VariableDeclarator(
                                        Identifier("response"))
                                    .WithInitializer(
                                        EqualsValueClause(
                                            AwaitExpression(
                                                InvocationExpression(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName("_httpClient"),
                                                        IdentifierName("SendAsync")))
                                                .WithArgumentList(
                                                    ArgumentList(
                                                        SeparatedList<ArgumentSyntax>(
                                                            new SyntaxNodeOrToken[]{
                                                                Argument(
                                                                    IdentifierName("request")),
                                                                Token(SyntaxKind.CommaToken),
                                                                Argument(
                                                                    MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        IdentifierName("HttpCompletionOption"),
                                                                        IdentifierName("ResponseContentRead")))}))))))))),
                        ReturnStatement(
                            AwaitExpression(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("response"),
                                            IdentifierName("Content")),
                                        GenericName(
                                            Identifier("ReadFromJsonAsync"))
                                        .WithTypeArgumentList(
                                            TypeArgumentList(
                                                SingletonSeparatedList<TypeSyntax>(
                                                    IdentifierName(endpoint.ReturnType))))))))
        });

        var bodyParam = endpoint.Parameters.FirstOrDefault(p => p.ParameterType == ApiMethodParameterType.Body);

        if (bodyParam is not null)
        {
            body.Insert(body.Count-3, ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("request"),
                                    IdentifierName("Content")),
                                InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("JsonContent"),
                                            IdentifierName("Create")))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList<ArgumentSyntax>(
                                                Argument(
                                                    IdentifierName(bodyParam.Name))))))));
        }
        
        return MethodDeclaration(
                   GenericName(
                   Identifier("Task"))
                       .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName(endpoint.ReturnType)
                                    )
                                )
                           ),
                   Identifier(endpoint.Name))
               .WithModifiers(
                   TokenList(
                       new[]
                       {
                           Token(SyntaxKind.PublicKeyword),
                           Token(SyntaxKind.AsyncKeyword)
                       }))
               .WithParameterList(
                   ParameterList(
                       SeparatedList<ParameterSyntax>(parameters)))
               .WithBody(
                   Block(body));
    }
    
    private static ExpressionStatementSyntax GenerateQueryParamAdd(ApiMethodParameter methodParameter)
    {
        return ExpressionStatement(
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    InvocationExpression(
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                InvocationExpression(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("urlBuilder"),
                                                            IdentifierName("Append")))
                                                    .WithArgumentList(
                                                        ArgumentList(
                                                            SingletonSeparatedList<ArgumentSyntax>(
                                                                Argument(
                                                                    InvocationExpression(
                                                                            MemberAccessExpression(
                                                                                SyntaxKind
                                                                                    .SimpleMemberAccessExpression,
                                                                                MemberAccessExpression(
                                                                                    SyntaxKind
                                                                                        .SimpleMemberAccessExpression,
                                                                                    IdentifierName(
                                                                                        "System"),
                                                                                    IdentifierName("Uri")),
                                                                                IdentifierName(
                                                                                    "EscapeDataString")))
                                                                        .WithArgumentList(
                                                                            ArgumentList(
                                                                                SingletonSeparatedList<
                                                                                    ArgumentSyntax>(
                                                                                    Argument(
                                                                                        LiteralExpression(
                                                                                            SyntaxKind
                                                                                                .StringLiteralExpression,
                                                                                            Literal(
                                                                                                methodParameter.Name)))))))))),
                                                IdentifierName("Append")))
                                        .WithArgumentList(
                                            ArgumentList(
                                                SingletonSeparatedList<ArgumentSyntax>(
                                                    Argument(
                                                        LiteralExpression(
                                                            SyntaxKind.CharacterLiteralExpression,
                                                            Literal('=')))))),
                                    IdentifierName("Append")))
                            .WithArgumentList(
                                ArgumentList(
                                    SingletonSeparatedList<ArgumentSyntax>(
                                        Argument(
                                            InvocationExpression(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("System"),
                                                            IdentifierName("Uri")),
                                                        IdentifierName("EscapeDataString")))
                                                .WithArgumentList(
                                                    ArgumentList(
                                                        SingletonSeparatedList<ArgumentSyntax>(
                                                            Argument(
                                                                InvocationExpression(
                                                                        IdentifierName("ConvertToString"))
                                                                    .WithArgumentList(
                                                                        ArgumentList(
                                                                            SeparatedList<ArgumentSyntax>(
                                                                                new SyntaxNodeOrToken[]
                                                                                {
                                                                                    Argument(
                                                                                        IdentifierName(methodParameter.Name)),
                                                                                    Token(
                                                                                        SyntaxKind
                                                                                            .CommaToken),
                                                                                    Argument(
                                                                                        MemberAccessExpression(
                                                                                            SyntaxKind
                                                                                                .SimpleMemberAccessExpression,
                                                                                            MemberAccessExpression(
                                                                                                SyntaxKind
                                                                                                    .SimpleMemberAccessExpression,
                                                                                                MemberAccessExpression(
                                                                                                    SyntaxKind
                                                                                                        .SimpleMemberAccessExpression,
                                                                                                    IdentifierName(
                                                                                                        "System"),
                                                                                                    IdentifierName(
                                                                                                        "Globalization")),
                                                                                                IdentifierName(
                                                                                                    "CultureInfo")),
                                                                                            IdentifierName(
                                                                                                "InvariantCulture")))
                                                                                }))))))))))),
                        IdentifierName("Append")))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList<ArgumentSyntax>(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.CharacterLiteralExpression,
                                    Literal('&')))))));
    }
}