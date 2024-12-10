using Altium.Core;

//await new FileWriter(@"c:\Temp\Altium\input.txt").WriteRandomRowsAsync(100_000);

await new Sorter(@"c:\Temp\Altium\result.txt", @"c:\Temp\Altium\temp").SortAsync(@"c:\Temp\Altium\input.txt");