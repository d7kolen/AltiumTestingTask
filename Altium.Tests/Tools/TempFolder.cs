using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Altium.Tests;

class TempFolder
{
    public string Folder { get; private set; } = null!;

    public static TempFolder Create()
    {
        CleanTempFolder();

        var path = System.IO.Path.Combine(
            TempFolderLocation(),
            DateTime.UtcNow.ToString("yyyy-MM-dd_hh-mm-ss"),
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(path);

        return new TempFolder()
        {
            Folder = path
        };
    }

    private static object? _cleaned = new object();

    private static void CleanTempFolder()
    {
        var tCleaned = _cleaned;
        if (tCleaned == null)
            return;

        lock (tCleaned)
        {
            if (_cleaned == null)
                return;

            Directory.CreateDirectory(TempFolderLocation());

            foreach (var t in Directory.GetDirectories(TempFolderLocation()))
                DeleteOverdatedFolder(t);

            _cleaned = null;
        }
    }

    private static void DeleteOverdatedFolder(string t)
    {
        try
        {
            var tName = Path.GetFileName(t);
            if (
                DateTime.TryParseExact(tName, "yyyy-MM-dd_hh-mm-ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out var tDate)
                &&
                tDate.AddHours(1) < DateTime.UtcNow
                )
            {
                Directory.Delete(t, true);
            }
        }
        //the deletion is not critical - there is a test cleaning only
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static string TempFolderLocation()
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        return Path.Combine(assemblyDir, "temp");
    }

    public string SubFile(string name)
    {
        return Path.Combine(Folder, name);
    }
}