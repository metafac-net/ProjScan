using System.Collections.Immutable;

namespace ProjScan
{
    internal record ProjectData(string Name, string FullPath)
    {
        private const string GeneratePackageOnBuild = nameof(GeneratePackageOnBuild);

        public ImmutableDictionary<string, string> OtherProps { get; init; } = ImmutableDictionary<string, string>.Empty;
        public ImmutableList<Message> Messages { get; init; } = ImmutableList<Message>.Empty;
        public bool IsPublished => OtherProps.ContainsKey(GeneratePackageOnBuild);
    }
}