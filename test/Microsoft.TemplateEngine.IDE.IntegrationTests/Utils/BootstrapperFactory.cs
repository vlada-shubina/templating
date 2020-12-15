using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Edge;
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

namespace Microsoft.TemplateEngine.IDE.IntegrationTests.Utils
{
    internal static class BootstrapperFactory
    {
        private const string HostIdentifier = "IDE.IntegrationTests";
        private const string HostVersion = "v1.0.0";

        internal static Bootstrapper GetBootstrapper(bool installAllTemplates = false, IEnumerable<string> additionalVirtualLocations = null)
        {
            string home = "%USERPROFILE%";
            if (Path.DirectorySeparatorChar == '/')
            {
                home = "%HOME%";
            }

            ITemplateEngineHost host = CreateHost();
            string profileDir = Environment.ExpandEnvironmentVariables(home);

            string hivePath = Path.Combine(profileDir, ".idetests");
            host.VirtualizeDirectory(hivePath);

            if (additionalVirtualLocations != null)
            {
                foreach (string virtualLocation in additionalVirtualLocations)
                {
                    host.VirtualizeDirectory(virtualLocation);
                }
            }

            if (installAllTemplates)
            {
                return new Bootstrapper(host, InstallAllTemplatesOnFirstRun, true);
            }
            return new Bootstrapper(host, null, true);
        }



        private static ITemplateEngineHost CreateHost()
        {
            var preferences = new Dictionary<string, string>
            {
                { "prefs:language", "C#" }
            };

            var builtIns = new AssemblyComponentCatalog(new[]
            {
                typeof(RunnableProjectGenerator).GetTypeInfo().Assembly,            // for assembly: Microsoft.TemplateEngine.Orchestrator.RunnableProjects
                typeof(AssemblyComponentCatalog).GetTypeInfo().Assembly,            // for assembly: Microsoft.TemplateEngine.Edge
            });

            return new DefaultTemplateEngineHost(HostIdentifier, HostVersion, CultureInfo.CurrentCulture.Name, preferences, builtIns, new[] { "idetests" });
        }

        private static void InstallAllTemplatesOnFirstRun(IEngineEnvironmentSettings environmentSettings, IInstaller installer)
        {
            string codebase = typeof(BootstrapperFactory).GetTypeInfo().Assembly.Location;
            Uri cb = new Uri(codebase);
            string asmPath = cb.LocalPath;
            string dir = Path.GetDirectoryName(asmPath);

            string packages = Path.Combine(dir, "..", "..", "..", "..", "..", "artifacts", "packages") + Path.DirectorySeparatorChar + "*";
            string templates = Path.Combine(dir, "..", "..", "..", "..", "..", "template_feed") + Path.DirectorySeparatorChar;
            string testTemplates = Path.Combine(dir, "..", "..", "..", "..", "..", "test", "Microsoft.TemplateEngine.TestTemplates", "test_templates") + Path.DirectorySeparatorChar;
            installer.InstallPackages(new[] { packages });
            installer.InstallPackages(new[] { templates, testTemplates });
        }
    }
}
