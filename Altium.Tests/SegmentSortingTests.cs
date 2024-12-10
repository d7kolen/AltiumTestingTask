//using Altium.Core;
//using FluentAssertions;
//using NUnit.Framework;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Altium.Tests;

//[TestFixture]
//public class SegmentSortingTests : IRowFileTest
//{
//    [Test]
//    public async Task SegmentSorting()
//    {
//        var folder = TempFolder.Create();
//        var file = folder.SubFile("1.txt");

//        await this.AppendLineToFile(file, "5. abc");
//        await this.AppendLineToFile(file, "5. abc");
//        await this.AppendLineToFile(file, "5. abc");

//        new SegmentSorting()

//        var rows = new FileReader(file, 0).Read().ToList();

//        rows.Should().HaveCount(1);
//        rows[0].Number.Should().Be(5);
//        rows[0].StringValue.Should().Be("abc");
//    }
//}