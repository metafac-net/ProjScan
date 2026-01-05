using System;
using System.Linq;

namespace MetaFac.ProjScan.Config
{
    public class ProjScanJson
    {
        public bool? StopAscending { get; set; }
        public bool? ExcludeFromScan { get; set; }
        public string? CompanyName { get; set; }
        public string? ProductName { get; set; }
        public bool? PackageRequireLicenseAcceptance { get; set; }
        public string? PackageLicenseFile { get; set; }
        public string? PackageLicenseExpression { get; set; }
        public string[]? ExemptionCodes { get; set; }

        public ProjScanJson() { }

        public ProjScanJson(ProjScanJson source, ProjScanJson? parent = null)
        {
            StopAscending = source.StopAscending;
            ExcludeFromScan = source.ExcludeFromScan;
            CompanyName = source.CompanyName;
            ProductName = source.ProductName;
            PackageRequireLicenseAcceptance = source.PackageRequireLicenseAcceptance;
            PackageLicenseFile = source.PackageLicenseFile;
            PackageLicenseExpression = source.PackageLicenseExpression;
            ExemptionCodes = source.ExemptionCodes ?? Array.Empty<string>();
            if (parent is not null)
            {
                StopAscending ??= parent.StopAscending;
                ExcludeFromScan ??= parent.ExcludeFromScan;
                CompanyName ??= parent.CompanyName;
                ProductName ??= parent.ProductName;
                PackageRequireLicenseAcceptance ??= parent.PackageRequireLicenseAcceptance;
                PackageLicenseFile ??= parent.PackageLicenseFile;
                PackageLicenseExpression ??= parent.PackageLicenseExpression;
                if (parent.ExemptionCodes is not null && parent.ExemptionCodes.Length > 0)
                {
                    ExemptionCodes = ExemptionCodes.Concat(parent.ExemptionCodes).Distinct().ToArray();
                }
                if (ExemptionCodes.Length == 0)
                {
                    ExemptionCodes = null;
                }
            }
        }
    }
}