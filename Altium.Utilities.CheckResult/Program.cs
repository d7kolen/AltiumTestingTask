using Altium.Core.Infrastructure;
using Altium.Utilities.CheckResult;

var config = new ConfigLoader().Load();

using var logger = new LoggerFactory().CreateLogger(config["LogFolder"]!);

logger.Information("Start");

var resultFile = config["ResultFile"]!;
var originalFile = config["OriginalFile"]!;

var checker = new SortedFileChecker(logger);

try
{
    var result = checker.Check(originalFile, resultFile);

    if (result.OrderCheck)
        logger.Information("File is sorted.");
    else
        logger.Warning("File is NOT sorted.");

    if (result.StatisticsCheck)
        logger.Information("Number statistics are matching.");
    else
        logger.Warning(
            "Number statistics are NOT matching.");

    logger.Information("Finish with {success}", result.TotalSuccess);
}
catch (Exception e)
{
    logger.Error(e, "Error");
}