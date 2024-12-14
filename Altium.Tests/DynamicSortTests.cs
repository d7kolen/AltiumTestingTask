using Altium.Core;
using FluentAssertions;
using NUnit.Framework;

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
    public void SortTest()
    {
        var r0 = new RowDtoSorting(new RowDto("1. abc", _alphabet));
        var r1 = new RowDtoSorting(new RowDto("3. abc", _alphabet), r0);
        var root = new RowDtoSorting(new RowDto("2. abc", _alphabet), r1);

        root = RowDtoSorting.Sort(root, new RowDtoComparer());

        root.Data.Number.Should().Be(1);
        root.Bigger.Data.Number.Should().Be(2);
        root.Bigger.Bigger.Data.Number.Should().Be(3);
        root.Bigger.Bigger.Bigger.Should().BeNull();
    }

    [Test]
    public void SortTest_1()
    {
        var r0 = new RowDtoSorting(new RowDto("1. abc", _alphabet));
        var r1 = new RowDtoSorting(new RowDto("2. abc", _alphabet), r0);
        var root = new RowDtoSorting(new RowDto("3. abc", _alphabet), r1);

        root = RowDtoSorting.Sort(root, new RowDtoComparer());

        root.Data.Number.Should().Be(1);
        root.Bigger.Data.Number.Should().Be(2);
        root.Bigger.Bigger.Data.Number.Should().Be(3);
        root.Bigger.Bigger.Bigger.Should().BeNull();
    }

    [Test]
    public void SortTest_2()
    {
        var r0 = new RowDtoSorting(new RowDto("3. abc", _alphabet));
        var r1 = new RowDtoSorting(new RowDto("2. abc", _alphabet), r0);
        var root = new RowDtoSorting(new RowDto("1. abc", _alphabet), r1);

        root = RowDtoSorting.Sort(root, new RowDtoComparer());

        root.Data.Number.Should().Be(1);
        root.Bigger.Data.Number.Should().Be(2);
        root.Bigger.Bigger.Data.Number.Should().Be(3);
        root.Bigger.Bigger.Bigger.Should().BeNull();
    }

    [Test]
    public void SortTest_3()
    {
        var r0 = new RowDtoSorting(new RowDto("4. abc", _alphabet));
        var r1 = new RowDtoSorting(new RowDto("3. abc", _alphabet), r0);
        var r2 = new RowDtoSorting(new RowDto("2. abc", _alphabet), r1);
        var root = new RowDtoSorting(new RowDto("1. abc", _alphabet), r2);

        root = RowDtoSorting.Sort(root, new RowDtoComparer());

        root.Data.Number.Should().Be(1);
        root.Bigger.Data.Number.Should().Be(2);
        root.Bigger.Bigger.Data.Number.Should().Be(3);
        root.Bigger.Bigger.Bigger.Data.Number.Should().Be(4);
        root.Bigger.Bigger.Bigger.Bigger.Should().BeNull();
    }
}