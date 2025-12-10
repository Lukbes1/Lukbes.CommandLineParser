namespace Lukbes.CommandLineParser;

public static class TypeExtensions
{
    public static string GetFriendlyTypeName(this Type type)
    {
        if (type.IsGenericType)
        {
            string typeName = type.Name;
            int backtickIndex = typeName.IndexOf('`');
            if (backtickIndex > 0)
            {
                typeName = typeName[..backtickIndex];
            }

            string genericArgs = string.Join(", ", type.GetGenericArguments()
                .Select(GetFriendlyTypeName));

            return $"{typeName}<{genericArgs}>";
        }

        return type.Name;
    }
}