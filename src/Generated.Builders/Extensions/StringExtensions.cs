namespace Generated.Builders;

internal static class StringExtensions
{
    public static string Capitalize(this string name)
    {
        return name.Substring(0, 1).ToUpper() + name.Substring(1, name.Length - 1);
    }
}
