using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.UnityLinker;

namespace Fy.Services.Editor
{
    // Services are found by reflection, so IL2CPP's code stripper can't see them
    // being used and may delete them from the build. Unity calls this during the
    // build to ask for extra preservation rules. We list every service class and
    // tell the stripper to keep it. Editor-only, runs at build time.
    public sealed class ServicesLinkXmlGenerator : IUnityLinkerProcessor
    {
        public int callbackOrder => 0;

        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            IEnumerable<IGrouping<string, Type>> servicesByAssembly = TypeCache
                .GetTypesDerivedFrom<IService>()
                .Where(IsPreservableService)
                .GroupBy(type => type.Assembly.GetName().Name);

            StringBuilder builder = new();
            builder.AppendLine("<linker>");

            foreach (IGrouping<string, Type> assemblyGroup in servicesByAssembly)
            {
                builder.AppendLine($"  <assembly fullname=\"{assemblyGroup.Key}\">");

                foreach (Type service in assemblyGroup)
                {
                    builder.AppendLine($"    <type fullname=\"{service.FullName}\" preserve=\"all\"/>");
                }

                builder.AppendLine("  </assembly>");
            }

            builder.AppendLine("</linker>");

            string path = Path.GetFullPath(Path.Combine("Temp", "FyServicesLink.xml"));
            File.WriteAllText(path, builder.ToString());

            return path;
        }

        private static bool IsPreservableService(Type type)
        {
            return !type.IsAbstract
                && !type.IsInterface
                && !type.IsGenericTypeDefinition;
        }
    }
}
