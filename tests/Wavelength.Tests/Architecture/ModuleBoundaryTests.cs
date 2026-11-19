using System.Reflection;
using System.Text.RegularExpressions;
using Wavelength.Catalogue;
using Wavelength.Playback;
using Wavelength.Playlists;
using Wavelength.Royalties;
using Wavelength.Tests.Support;
using Xunit;

namespace Wavelength.Architecture.Tests;

public class ModuleBoundaryTests
{
    private static readonly Assembly[] Modules =
    [
        typeof(CatalogueModule).Assembly,
        typeof(PlaybackModule).Assembly,
        typeof(PlaylistsModule).Assembly,
        typeof(RoyaltiesModule).Assembly,
    ];

    private static readonly string[] ModuleNames = Modules.Select(m => m.GetName().Name!).ToArray();

    [Fact]
    public void No_module_assembly_references_another_module()
    {
        foreach (var module in Modules)
        {
            var name = module.GetName().Name!;
            var refs = module.GetReferencedAssemblies().Select(r => r.Name!).ToList();
            foreach (var other in ModuleNames.Where(n => n != name))
            {
                Assert.False(refs.Contains(other), $"{name} references {other}; talk through Wavelength.SharedKernel contracts instead");
            }
        }
    }

    [Fact]
    public void No_module_project_file_references_another_module_project()
    {
        var root = ApiHost.RepoRoot();
        foreach (var csproj in Directory.GetFiles(Path.Combine(root, "src", "Modules"), "*.csproj", SearchOption.AllDirectories))
        {
            var refs = Regex.Matches(File.ReadAllText(csproj), "ProjectReference Include=\"([^\"]+)\"").Select(m => Path.GetFileNameWithoutExtension(m.Groups[1].Value));
            Assert.All(refs, r => Assert.False(ModuleNames.Contains(r), $"{Path.GetFileName(csproj)} references module project {r}"));
        }
    }

    [Fact]
    public void Modules_only_expose_their_entry_point_and_contracts()
    {
        foreach (var module in Modules)
        {
            var root = module.GetName().Name!;
            var moduleClass = root.Replace("Wavelength.", string.Empty, StringComparison.Ordinal) + "Module";
            var leaks = module.GetExportedTypes()
                .Where(t => t.Namespace != root + ".Contracts" && !(t.Namespace == root && t.Name == moduleClass))
                .Select(t => t.FullName)
                .ToList();

            Assert.True(leaks.Count == 0, $"{root} exposes internals: {string.Join(", ", leaks)}");
        }
    }

    [Fact]
    public void Module_types_only_use_their_own_types_shared_kernel_and_framework()
    {
        foreach (var module in Modules)
        {
            var name = module.GetName().Name!;
            var foreign = module.GetTypes()
                .SelectMany(t => t.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Select(f => f.FieldType)
                    .Concat(t.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SelectMany(c => c.GetParameters()).Select(p => p.ParameterType)))
                .Select(t => t.Assembly.GetName().Name!)
                .Where(a => a != name && ModuleNames.Contains(a))
                .Distinct()
                .ToList();

            Assert.True(foreign.Count == 0, $"{name} uses types from {string.Join(", ", foreign)}");
        }
    }
}
