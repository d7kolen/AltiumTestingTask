using System.IO;
using System.Text;
using System.Threading.Tasks;
using Altium.Core.Row;

namespace Altium.Tests.Tools;

interface IRowFileTest
{
}

static class RowFileHelper
{
    public static async Task AppendLineToFile(this IRowFileTest test, string file, string line)
    {
        await using var writer = new StreamWriter(file, true, Encoding.UTF8);
        writer.WriteLine(line);
    }

    public static string StringValueAsString(this RowDto row)
    {
        return new string(row.StringValue.Span);
    }
}