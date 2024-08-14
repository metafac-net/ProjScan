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
                if (StopAscending is null) StopAscending = parent.StopAscending;
                if (ExcludeFromScan is null) ExcludeFromScan = parent.ExcludeFromScan;
                if (CompanyName is null) CompanyName = parent.CompanyName;
                if (ProductName is null) ProductName = parent.ProductName;
                if (PackageRequireLicenseAcceptance is null) PackageRequireLicenseAcceptance = parent.PackageRequireLicenseAcceptance;
                if (PackageLicenseFile is null) PackageLicenseFile = parent.PackageLicenseFile;
                if (PackageLicenseExpression is null) PackageLicenseExpression = parent.PackageLicenseExpression;
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