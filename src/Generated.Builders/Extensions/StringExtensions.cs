namespace Generated.Builders;

internal static class StringExtensions
{
    public static string ToSingular(this string name)
    {
        if (name.EndsWith("s"))
        {
            return name.Substring(0, name.Length - "s".Length);
        }

        return name;
    }

    public static string Capitalize(this string name)
    {
        return name.Substring(0, 1).ToUpper() + name.Substring(1, name.Length - 1);
    }
}
