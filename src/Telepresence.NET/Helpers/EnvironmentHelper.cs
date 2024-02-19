namespace Telepresence.NET.Helpers;

public static class EnvironmentHelper
{
    public static bool TryGetEnvironmentVariable<T>(string key, out T? variable)
    {
        variable = default;

        try
        {
            var environmentVariable = Environment.GetEnvironmentVariable(key);

            if (string.IsNullOrWhiteSpace(environmentVariable))
                return false;

            variable = (T)Convert.ChangeType(environmentVariable, typeof(T));

            return true;
        }
        catch
        {
            return false;
        }
    }
}
