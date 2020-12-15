using Microsoft.TemplateEngine.Edge.Template;
using Microsoft.TemplateEngine.IDE.IntegrationTests.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.TemplateEngine.IDE.IntegrationTests
{
    public class CreateTests 
    {
        [Fact]
        internal async Task BasicTest()
        {
            var bootstrapper = BootstrapperFactory.GetBootstrapper(additionalVirtualLocations: new string[] { "test" });
            bootstrapper.InstallTestTemplate("TemplateWithSourceName");
            var template = bootstrapper.ListTemplates(true, WellKnownSearchFilters.NameFilter("TestAssets.TemplateWithSourceName"));

            var result = await bootstrapper.CreateAsync(template.First().Info, "test", "test", new Dictionary<string, string>(), false, "").ConfigureAwait(false);

            Assert.Equal(0, result.PrimaryOutputs.Count);
            Assert.Equal(0, result.PostActions.Count);
        }


    }
}
