using SpringBootApiClientGenerator.AntlrParser.Models;

namespace SpringBootApiClientGenerator.AntlrParser;

public class ClassVisitor : JavaParserBaseVisitor<ApiModel>
{
    public override ApiModel VisitTypeDeclaration(JavaParser.TypeDeclarationContext context)
    {
        var className = context.children.OfType<JavaParser.ClassDeclarationContext>().Single().children
                               .OfType<JavaParser.IdentifierContext>().Single().GetText();
        
        var members = context.children.OfType<JavaParser.ClassDeclarationContext>()
                             .Single().children
                             .OfType<JavaParser.ClassBodyContext>().Single().children
                             .OfType<JavaParser.ClassBodyDeclarationContext>()
                             .SelectMany(c => c.children)
                             .OfType<JavaParser.MemberDeclarationContext>()
                             .SelectMany(c => c.children)
                             .OfType<JavaParser.FieldDeclarationContext>();

        List<(string, string)> models = new List<(string, string)>();
        
        foreach (var fieldDeclaration in members)
        {
            string type = fieldDeclaration.children.OfType<JavaParser.TypeTypeContext>().Single().GetText();
            string name = fieldDeclaration.children.OfType<JavaParser.VariableDeclaratorsContext>().Single().GetText();
            type = StaticTypeProvider.RegisterType(type);
            models.Add((type, name));
        }
        
        return new ApiModel()
        {
            Name = className,
            Fields = models.ToList()
        };
    }
}