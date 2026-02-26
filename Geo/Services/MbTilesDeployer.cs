using System.Reflection;

namespace Geo.Services;

public static class MbTilesDeployer
{
    public static string MbTilesLocation { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Geo");

    public static void CopyEmbeddedResourceToFileIfNeeded(string fileName)
    {
        var assembly = typeof(MbTilesDeployer).GetTypeInfo().Assembly;
        assembly.CopyEmbeddedResourceToFile(@"Geo.Resources.Raw.", MbTilesLocation, fileName);
    }

    private static void CopyEmbeddedResourceToFile(
        this Assembly assembly,
        string embeddedResourcesPath,
        string folder,
        string resourceFile)
    {
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var destPath = Path.Combine(folder, resourceFile);

        using var image = assembly.GetManifestResourceStream(embeddedResourcesPath + resourceFile)
            ?? throw new ArgumentException("EmbeddedResource not found");

        if (File.Exists(destPath))
        {
            var fileInfo = new FileInfo(destPath);
            if (fileInfo.Length == image.Length)
            {
                return;
            }
        }

        using var dest = File.Create(destPath);
        image.CopyTo(dest);
    }
}
