using System.Collections.Immutable;

namespace ProjScan
{
    internal record ProjectData(string Name, string FullPath)
    {
        private const string GeneratePackageOnBuild = nameof(GeneratePackageOnBuild);
        private const string OutputType = nameof(OutputType);

        public ImmutableDictionary<string, string> OtherProps { get; init; } = ImmutableDictionary<string, string>.Empty;
        public ImmutableList<Message> Messages { get; init; } = ImmutableList<Message>.Empty;
        public bool IsPublished => OtherProps.ContainsKey(GeneratePackageOnBuild);
        public bool OutputIsExe => OtherProps.ContainsKey(OutputType) && string.Equals(OtherProps[OutputType], "exe", System.StringComparison.InvariantCultureIgnoreCase);
    }
}