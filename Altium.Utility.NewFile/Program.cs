using Altium.Core;

var config = new ConfigLoader().Load();

using var logger = new LoggerFactory().CreateLogger(config["LogFolder"]);

logger.Information("Start");

var filePath = config["ResultFile"];
if (File.Exists(filePath))
    File.Delete(filePath);

var countRows = int.Parse(config["CountRows"]!.Replace(" ", ""));

using var writer = new FileWriter(filePath);
writer.WriteRandomRows(countRows, logger);

logger.Information("Finish");