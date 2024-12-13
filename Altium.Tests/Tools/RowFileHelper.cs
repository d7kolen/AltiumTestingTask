using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Altium.Tests;

interface IRowFileTest { }

static class RowFileHelper
{
    public static async Task AppendLineToFile(this IRowFileTest test, string file, string line)
    {
        await using (var writer = new StreamWriter(file, true, Encoding.UTF8))
            writer.WriteLine(line);
    }

    public static async IAsyncEnumerable<List<T>> ToAsyncEnumerable<T>(this IEnumerable<T> list)
    {
        await Task.Yield();

        foreach (var t in list)
            yield return new List<T>() { t };
    }
}