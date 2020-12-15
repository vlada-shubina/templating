using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Edge;
using Microsoft.TemplateEngine.Edge.Template;
using Microsoft.TemplateEngine.IDE.IntegrationTests.Utils;
using Microsoft.TemplateEngine.Orchestrator.RunnableProjects;
using Microsoft.TemplateEngine.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.TemplateEngine.IDE.IntegrationTests
{
    public class BasicExample
    {
        [Fact]
        internal async Task BasicExampleTest()
        {
#if DEBUG
            string configuration = "Debug";
#else
            string configuration = "Release";
#endif
            //preparing host
            //host properties
            const string HostIdentifier = "BasicExampleHost";
            const string HostVersion = "v1.0.0";

            //example host preferences, can be empty
            var preferences = new Dictionary<string, string>
            {
                { "prefs:language", "C#" }
            };

            //registering components from Orchestrator.RunnableProjects (generator) and  Microsoft.TemplateEngine.Edge (mount point implementations)
            var builtIns = new AssemblyComponentCatalog(new[]
            {
                typeof(RunnableProjectGenerator).GetTypeInfo().Assembly,            // for assembly: Microsoft.TemplateEngine.Orchestrator.RunnableProjects
                typeof(AssemblyComponentCatalog).GetTypeInfo().Assembly,            // for assembly: Microsoft.TemplateEngine.Edge
            });

            //creating default host
            ITemplateEngineHost host = new DefaultTemplateEngineHost(HostIdentifier, HostVersion, CultureInfo.CurrentCulture.Name, preferences, builtIns, null);

            // creating bootstrapper with created host
            // if any actions are needed pass predicate to execute actions as second parameter
            // usually it is used to install default set of templates
            // use true for virtualizeConfiguration if settings should be stored in memory only
            Bootstrapper bootstrapper = new Bootstrapper(host, null, virtualizeConfiguration:true);

            //install template to execute
            //also supports NuGet packages
            //do not support installation from NuGet feed
            string codebase = typeof(BootstrapperExtensions).GetTypeInfo().Assembly.Location;
            string dir = Path.GetDirectoryName(codebase);
            string path = Path.Combine(dir, "..", "..", "..", "..", "..", "test", "Microsoft.TemplateEngine.TestTemplates", "test_templates", "TemplateWithSourceName") + Path.DirectorySeparatorChar;
            bootstrapper.Install(path);
            path = Path.Combine(dir, "..", "..", "..", "..", "..", "artifacts", "packages", configuration, "Shipping", "Microsoft.DotNet.Common.ProjectTemplates.5.0.6.0.0-dev.nupkg");
            bootstrapper.Install(path);

            //get the template to execute
            var template = bootstrapper.ListTemplates(true, WellKnownSearchFilters.NameFilter("TestAssets.TemplateWithSourceName")).First().Info;
            var consoleTemplate = bootstrapper.ListTemplates(true, WellKnownSearchFilters.NameFilter("console")).First().Info;

            //dry run template
            //parameters
            //template - template to execute 
            //name - template source name
            //outputPath - output path for template execution
            //parameters - collection of input parameters to template
            //baseline - baseline to use (if implemented in template) (optional)
            var dryRunResult = await bootstrapper.GetCreationEffectsAsync(template, "test", "test", new Dictionary<string, string>(), "").ConfigureAwait(false);
            var dryRunConsoleResult = await bootstrapper.GetCreationEffectsAsync(consoleTemplate, "consoleTest", "consoleTest", new Dictionary<string, string>(), "").ConfigureAwait(false);
            //result contains primary outputs, post actions and files to be created / updated

            //execute template
            //parameters
            //template - template to execute
            //name - template source name
            //outputPath - output path for template execution
            //parameters - collection of input parameters to template
            //skiUpdateCheck - allow overwrite existing files (force creation)
            //baseline - baseline to use (if implemented in template) (optional)
            var executionResult = await bootstrapper.CreateAsync(template, "test", "test", new Dictionary<string, string>(), false, "").ConfigureAwait(false);
            var executionConsoleResult = await bootstrapper.CreateAsync(template, "consoleTest", "consoleTest", new Dictionary<string, string>(), false, "").ConfigureAwait(false);
            //result contains primary outputs and post actions
            //note that post actions are not executed - host should execute them additionally
        }
    }
}
