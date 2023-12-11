namespace Telepresence.NET;

public static class Constants
{
    public static class Exceptions
    {
        public const string AlphaNumericWithHyphens = "Value must consist of only letters, numbers and hyphens.";
        public const string AlphaNumericWithUnderscores = "Value must consist of only letters, numbers and underscores.";
        public const string AlphaNumericWithHyphensUnderscores = "Value must consist of only letters, numbers, hyphens and underscores.";
        public const string AlphaNumericWithHyphensUnderscoresDots = "Value must consist of only letters, numbers, hyphens, underscores and periods.";
        public const string CantDetermineName = "Cannot determine name from input or convention.";
        public const string CantExceed64Characters = "Cannot exceed 64 characters.";
        public const string InvalidNumberOfWorkloadsDefined = "Only 1 - 32 workloads can be defined at once.";
        public const string InvalidNumberOfInterceptsDefined = "Only 1 - 16 intercepts can be defined at once.";
        public const string InvalidNumberOfHandlersDefined = "Only 1 - 64 handlers can be defined at once.";
        public const string NotValidPort = "Port numbers can only be between 1 - 65535.";
        public const string NotAnIpAddress = "Not a valid IP address.";
        public const string GlobalMutuallyExclusive = "Cannot set Global while Paths or Headers are also set.";
        public const string MutuallyExclusiveHandlers = "Handlers are mutually exclusive, you must set exactly one of [docker, script, external].";
        public const string UnableToStartIntercept = "Unable to start the intercept.";
        public const string NoWorkloadFound = "Unable to find a workload to intercept.";
        public const string NoWorkloadInterceptFound = "Unable to find a workload intercept to start.";
    }
}