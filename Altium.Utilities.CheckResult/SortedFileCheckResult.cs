namespace Altium.Utilities.CheckResult;

public record SortedFileCheckResult(
    bool OrderCheck,
    bool StatisticsCheck)
{
    public bool TotalSuccess => OrderCheck && StatisticsCheck;
}
