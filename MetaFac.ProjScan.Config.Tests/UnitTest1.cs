using Shouldly;
using System.Text.Json;
using Xunit;

namespace MetaFac.ProjScan.Config.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Serialize()
        {
            var cfg = new ProjScanJson()
            {
                StopAscending = true,
                CompanyName = "TestCo"
            };
            var jso = new JsonSerializerOptions() { WriteIndented = true };
            string text = JsonSerializer.Serialize<ProjScanJson>(cfg, jso);
            string[] lines = text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            lines.ShouldBeEquivalentTo(new string[] {
                "{",
                "  \"StopAscending\": true,",
                "  \"ExcludeFromScan\": null,",
                "  \"CompanyName\": \"TestCo\",",
                "  \"ProductName\": null,",
                "  \"PackageRequireLicenseAcceptance\": null,",
                "  \"PackageLicenseFile\": null,",
                "  \"PackageLicenseExpression\": null,",
                "  \"ExemptionCodes\": null",
                "}",
            });
        }

        [Fact]
        public void Deserialize()
        {
            string text = "{'CompanyName':'TestCo'}".Replace('\'', '"');

            ProjScanJson cfg = JsonSerializer.Deserialize<ProjScanJson>(text)!;

            cfg.ShouldNotBeNull();
            cfg.StopAscending.ShouldBeNull();
            cfg.CompanyName.ShouldBe("TestCo");
            cfg.ProductName.ShouldBeNull();
        }

        [Fact]
        public void Override()
        {
            var lowest = new ProjScanJson()
            {
            };

            var middle = new ProjScanJson()
            {
                ProductName = "Product1"
            };

            var highest = new ProjScanJson()
            {
                StopAscending = true,
                CompanyName = "TestCo",
                ProductName = "**error**"
            };

            var result = new ProjScanJson(lowest, middle);
            result.StopAscending.ShouldBeNull();
            result.ProductName.ShouldBe("Product1");
            result.CompanyName.ShouldBeNull();

            result = new ProjScanJson(result, highest);
            result.StopAscending.ShouldNotBeNull();
            result.StopAscending.Value.ShouldBeTrue();
            result.ProductName.ShouldBe("Product1");
            result.CompanyName.ShouldBe("TestCo");

        }

    }
}