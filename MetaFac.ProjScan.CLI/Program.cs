using MetaFac.ProjScan.Config;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ProjScan
{
    internal readonly struct ItemSupportPeriod
    {
        public readonly string Name;
        public readonly DateTime Start;
        public readonly DateTime End;

        public ItemSupportPeriod(string name, DateTime start, DateTime end) : this()
        {
            Name = name;
            Start = start;
            End = end;
        }
    }

    public class Program
    {
        private static IEnumerable<KeyValuePair<string, string>> GetPropertyGroupValues(string fullPath)
        {
            using var fs = new FileStream(fullPath, FileMode.Open);
            XDocument xdoc = XDocument.Load(fs);
            foreach (var el1 in xdoc.XPathSelectElements("./Project/PropertyGroup"))
            {
                foreach (var el2 in el1.Elements())
                {
                    yield return new KeyValuePair<string, string>(el2.Name.LocalName, el2.Value);
                    // attributes
                    //foreach (var attr in el2.Attributes())
                    //{
                    //    switch (attr.Name.LocalName)
                    //    {
                    //        case "name":
                    //            assmName = attr.Value;
                    //            break;
                    //        case "publicKeyToken":
                    //            assmToken = attr.Value;
                    //            break;
                    //    }
                    //}
                }
            }
        }

        static ProjectData GetProjectData(string fullPath)
        {
            string projectName = Path.GetFileNameWithoutExtension(fullPath);
            Dictionary<string, string> otherProperties = new Dictionary<string, string>();
            List<Message> messages = new List<Message>();
            foreach (var kvp in GetPropertyGroupValues(fullPath))
            {
                if (!otherProperties.TryAdd(kvp.Key, kvp.Value))
                {
                    messages.Add(Message.NewWarning($"Duplicate property found: {kvp.Key}={kvp.Value}"));
                }
            }
            return new ProjectData(projectName, fullPath)
            {
                OtherProps = ImmutableDictionary<string, string>.Empty.AddRange(otherProperties),
                Messages = ImmutableList<Message>.Empty.AddRange(messages),
            };
        }

        static int Body(string[] args)
        {
            AnsiConsole.WriteLine("ProjScan");
            var options = new Options(args);
            if (!options.Parsed) return -1;

            string pathRoot = Path.GetFullPath(options.PathRoot);

            AnsiConsole.WriteLine($"Unattended: {options.Unattended}");
            AnsiConsole.WriteLine($"PathRoot  : {pathRoot}");

            string[] files = Directory.GetFiles(pathRoot, "*.csproj", SearchOption.AllDirectories)
                .ToArray();

            //------------------------------ data --------------------------------------------------

            var dotNetSupportPeriods = new ItemSupportPeriod[]
            {
                new ItemSupportPeriod("net5", new DateTime(2020, 11, 10), new DateTime(2022, 5, 14)),
                new ItemSupportPeriod("net6", new DateTime(2021, 11, 10), new DateTime(2024, 11, 12)),
                new ItemSupportPeriod("net7", new DateTime(2022, 11, 10), new DateTime(2024, 5, 14)),
                new ItemSupportPeriod("net8", new DateTime(2023, 11, 10), new DateTime(2026, 11, 10)),
                new ItemSupportPeriod("net9", new DateTime(2024, 11, 10), new DateTime(2026, 5, 14)),
            };

            //------------------------------ scan --------------------------------------------------

            AnsiConsole.WriteLine($"Scanning {files.Length} project files...");

            Dictionary<string, ProjectData> projects = new Dictionary<string, ProjectData>();

            foreach (var fullPath in files)
            {
                string? path = Path.GetDirectoryName(fullPath);
                string? projectName = Path.GetFileName(path);
                if (projectName is null) continue;

                ProjectData projectData = GetProjectData(fullPath);
                projects[fullPath] = projectData;
            }

            //------------------------------ analyse --------------------------------------------------

            ImmutableList<Message> CheckProperty(ProjectData project, string propertyName, Op op, string? expected = null,
                StringComparison stringComparison = StringComparison.Ordinal)
            {
                ImmutableList<Message> result = ImmutableList<Message>.Empty;

                    bool exists = project.OtherProps.TryGetValue(propertyName, out string? actual);

                    var comparee = actual?.Trim();
                    var comparand = expected?.Trim();

                switch (op)
                {
                    case Op.Exists:
                        if (!exists)
                        {
                            string warning = $"Expected '{propertyName}' to be present, but it was not found.";
                            result = result.Add(Message.NewWarning(warning));
                        }
                        break;
                    case Op.NotExists:
                        if (exists)
                        {
                            string warning = $"Expected '{propertyName}' not to be present, but found '{actual}'.";
                            result = result.Add(Message.NewWarning(warning));
                        }
                        break;
                    case Op.Equals:
                        if (string.Compare(comparee, comparand, stringComparison) != 0)
                        {
                            string warning = $"Expected '{propertyName}' to be '{expected}', but found '{actual}'.";
                            result = result.Add(Message.NewWarning(warning));
                        }
                        break;
                    case Op.NotEquals:
                        if (string.Compare(comparee, comparand, stringComparison) == 0)
                        {
                            string warning = $"Expected '{propertyName}' not to be '{expected}', but found '{actual}'.";
                            result = result.Add(Message.NewWarning(warning));
                        }
                        break;
                    case Op.Contains:
                        if (comparee is not null && comparand is not null && !comparee.Contains(comparand, stringComparison))
                        {
                            string warning = $"Expected '{propertyName}' to contain '{expected}', but '{actual}' does not.";
                            result = result.Add(Message.NewWarning(warning));
                        }
                        break;
                    case Op.NotContains:
                        if (comparee is not null && comparand is not null && comparee.Contains(comparand, stringComparison))
                        {
                            string warning = $"Expected '{propertyName}' to not contain '{expected}', but '{actual}' does.";
                            result = result.Add(Message.NewWarning(warning));
                        }
                        break;
                    case Op.StartsWith:
                        if (comparee is not null && comparand is not null && !comparee.StartsWith(comparand, stringComparison))
                        {
                            string warning = $"Expected '{propertyName}' to start with '{expected}', but '{actual}' does not.";
                            result = result.Add(Message.NewWarning(warning));
                        }
                        break;
                    case Op.EndsWith:
                        if (comparee is not null && comparand is not null && !comparee.EndsWith(comparand, stringComparison))
                        {
                            string warning = $"Expected '{propertyName}' to end with '{expected}', but '{actual}' does not.";
                            result = result.Add(Message.NewWarning(warning));
                        }
                        break;
                    default:
                        break;
                }

                return result;
            }

            int publicProjectCount = 0;
            foreach (var project in projects.Values.ToArray())
            {
                ProjScanJson cfg = GetProjScanCfg(project);

                if (cfg.ExcludeFromScan ?? false) continue;

                string companyName = cfg.CompanyName ?? "UnknownCompany";
                string? productName = cfg.ProductName;

                string projectId = project.FullPath;
                string[] pathParts = projectId.Split('/', '\\');
                // github defaults
                string adoOrgName = pathParts[^4];
                string adoProjName = pathParts[^3];
                //string adoRepoName = pathParts[^3];
                var messages = ImmutableList<Message>.Empty.ToBuilder();
                messages.AddRange(CheckProperty(project, "LangVersion", Op.Equals, "latest"));
                messages.AddRange(CheckProperty(project, "Nullable", Op.Equals, "enable"));
                messages.AddRange(CheckProperty(project, "WarningsAsErrors", Op.Exists));
                messages.AddRange(CheckProperty(project, "WarningsAsErrors", Op.Contains, "nullable"));
                if (project.IsPublished)
                {
                    publicProjectCount++;

                    messages.AddRange(CheckProperty(project, "TargetFramework", Op.NotExists));
                    messages.AddRange(CheckProperty(project, "TargetFrameworks", Op.Exists));

                    var now = DateTime.Now;
                    foreach (var period in dotNetSupportPeriods)
                    {
                        // expiry
                        if (now > period.Start && now < period.End)
                        {
                            messages.AddRange(CheckProperty(project, "TargetFrameworks", Op.Contains, period.Name));
                        }
                        // expiry
                        if (now > period.End)
                        {
                            messages.AddRange(CheckProperty(project, "TargetFrameworks", Op.NotContains, period.Name));
                        }
                    }

                    messages.AddRange(CheckProperty(project, "Description", Op.Exists));
                    messages.AddRange(CheckProperty(project, "Product", productName is null ? Op.Exists : Op.Equals, productName));
                    messages.AddRange(CheckProperty(project, "Copyright", Op.StartsWith, $"Copyright (c) "));
                    messages.AddRange(CheckProperty(project, "Copyright", Op.EndsWith, $"-{DateTime.Now.Year:D4} {companyName}"));
                    messages.AddRange(CheckProperty(project, "Company", Op.Equals, companyName));
                    messages.AddRange(CheckProperty(project, "Authors", Op.Equals, $"{companyName} Contributors"));
                    if (cfg.PackageRequireLicenseAcceptance is not null)
                    {
                        messages.AddRange(CheckProperty(project, "PackageRequireLicenseAcceptance", Op.Equals, cfg.PackageRequireLicenseAcceptance.ToString()));
                    }
                    if (cfg.PackageLicenseFile is not null)
                    {
                        messages.AddRange(CheckProperty(project, "PackageLicenseFile", Op.Equals, cfg.PackageLicenseFile));
                    }
                    if (cfg.PackageLicenseExpression is not null)
                    {
                        messages.AddRange(CheckProperty(project, "PackageLicenseExpression", Op.Equals, cfg.PackageLicenseExpression));
                    }
                    messages.AddRange(CheckProperty(project, "SignAssembly", Op.Equals, "true", StringComparison.OrdinalIgnoreCase));
                    messages.AddRange(CheckProperty(project, "AssemblyOriginatorKeyFile", Op.Equals, @"..\SigningKey.snk", StringComparison.OrdinalIgnoreCase));
                    messages.AddRange(CheckProperty(project, "IncludeSymbols", Op.Equals, "true", StringComparison.OrdinalIgnoreCase));
                    messages.AddRange(CheckProperty(project, "SymbolPackageFormat", Op.Equals, "snupkg"));
                    messages.AddRange(CheckProperty(project, "PackageReadmeFile", Op.Equals, "readme.md", StringComparison.OrdinalIgnoreCase));
                    messages.AddRange(CheckProperty(project, "PackageProjectUrl", Op.Equals, $"https://github.com/{adoOrgName}/{adoProjName}", StringComparison.OrdinalIgnoreCase));
                    messages.AddRange(CheckProperty(project, "RepositoryUrl", Op.Equals, $"https://github.com/{adoOrgName}/{adoProjName}", StringComparison.OrdinalIgnoreCase));
                }
                var newProject = project with { Messages = messages.ToImmutable() };

                projects[projectId] = newProject;
            }

            var tree = new Tree("Results")
                .Guide(TreeGuide.Line);

            tree.AddNode($"Root: {pathRoot}");

            int warningCount = 0;
            foreach (var project in projects.Values.OrderBy(p => p.FullPath))
            {
                if (project.Messages.Count > 0)
                {
                    var projnode = tree.AddNode($"[{Color.Cyan1}]{project.Name}[/]");
                    string subPath = Path.GetRelativePath(pathRoot, project.FullPath);
                    string relPath = Path.GetDirectoryName(subPath) ?? "";
                    string filename = Path.GetFileName(subPath) ?? "";
                    projnode.AddNode($"Folder  : {relPath}");
                    projnode.AddNode($"ProjFile: {filename}");
                    foreach (var message in project.Messages)
                    {
                        if (message.Warning)
                        {
                            warningCount++;
                            projnode.AddNode($"[{Color.Yellow}]Warning : {message.Text}[/]");
                        }
                        else
                            projnode.AddNode($"Message : {message.Text}");
                    }
                }
            }

            AnsiConsole.Write(tree);
            AnsiConsole.WriteLine($"Found {publicProjectCount} published projects, {warningCount} warnings issued.");
            int exitCode = warningCount > 0 ? 1 : 0;
            return exitCode;
        }

        private static ProjScanJson GetProjScanCfg(ProjectData project)
        {
            ProjScanJson result = new ProjScanJson();
            string? path = Path.GetDirectoryName(project.FullPath);
            while (path is not null)
            {
                string cfgFile = Path.Combine(path, "projscan.json");
                if (File.Exists(cfgFile))
                {
                    using FileStream fs = File.OpenRead(cfgFile);
                    ProjScanJson? cfg = JsonSerializer.Deserialize<ProjScanJson>(fs);
                    result = new ProjScanJson(result, cfg);
                    if (result.StopAscending ?? false)
                        return result;
                }

                // try parent
                path = Path.GetDirectoryName(path);
            }

            return result;
        }

        static void Main(string[] args)
        {
            int exitCode = 0;
            try
            {
                string? response = null;
                do
                {
                    AnsiConsole.Clear();
                    exitCode = Body(args);
                    AnsiConsole.WriteLine("Enter R to rescan, anything else to exit...");
                    response = Console.ReadLine();

                } while (exitCode >= 0 && response is not null && response.Trim().ToUpper() == "R");
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
                exitCode = -2;
            }
            Environment.Exit(exitCode);
            AnsiConsole.WriteLine($"ExitCode: {exitCode}");
        }
    }
}