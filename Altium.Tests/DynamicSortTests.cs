using Altium.Core;
using FluentAssertions;
using NUnit.Framework;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

namespace Altium.Tests;

[TestFixture]
public class DynamicSortTests
{
    #region Init

    RowDtoAlphabet _alphabet;

    [SetUp]
    public void Init()
    {
        _alphabet = new RowDtoAlphabet();
    }

    #endregion

    [Test]
    public async Task ReadRow()
    {
        var r0 = new RowDtoSorting(new RowDto("1. abc", _alphabet));
        var r1 = new RowDtoSorting(new RowDto("1. abc", _alphabet), r0);
        var root = new RowDtoSorting(new RowDto("1. abc", _alphabet), r1);

    }
}