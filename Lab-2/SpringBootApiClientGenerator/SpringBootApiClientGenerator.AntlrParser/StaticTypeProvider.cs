namespace SpringBootApiClientGenerator.AntlrParser;

public static class StaticTypeProvider
{
    private static readonly List<string> _existingTypes = new List<string>(){"void","int","long","string","bool","char"};
    private static readonly Queue<string> _typesToGenerate = new Queue<string>();

    public static string RegisterType(string typeName)
    {
        if (typeName.EndsWith("[]"))
        {
            if (_existingTypes.Contains(typeName[..^2])
                || _typesToGenerate.Contains(typeName[..^2]))
                return typeName;
            _typesToGenerate.Enqueue(typeName[..^2]);
            return typeName;
        }
            

        if (_existingTypes.Contains(typeName))
            return typeName;

        if (_existingTypes.Contains(typeName.ToLower()))
            return typeName.ToLower();

        if (!_typesToGenerate.Contains(typeName))
            _typesToGenerate.Enqueue(typeName);

        return typeName;
    }

    public static string GetTypeToGenerate() => _typesToGenerate.Dequeue();

    public static IReadOnlyCollection<string> TypesToGenerate => _typesToGenerate;
}