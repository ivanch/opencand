using System.Text;
using Microsoft.Extensions.Logging;

namespace OpenCand.Parser.Services
{
    public class CsvFixerService
    {
        private readonly ILogger<CsvFixerService> logger;

        public CsvFixerService(
            ILogger<CsvFixerService> logger)
        {
            this.logger = logger;
        }

        public string FixCsvFile(string filePath)
        {
            var filename = Path.GetFileName(filePath);
            var path = Path.GetDirectoryName(filePath);

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                logger.LogError($"FixCsvFile - The file at '{filePath}' does not exist");
                return string.Empty;
            }

            if (string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(path))
            {
                logger.LogError($"FixCsvFile - The file path '{filePath}' is invalid");
                return string.Empty;
            }

            // Fixed file will have the same name but with "fix_" prefix
            var newFilePath = Path.Combine(path, $"fix_{filename}");
            if (File.Exists(newFilePath))
            {
                logger.LogWarning($"FixCsvFile - A fixed file already exists at '{newFilePath}'. It will be overwritten.");
            }

            logger.LogInformation($"FixCsvFile - Starting to fix CSV file at '{filePath}'");

            try
            {
                // Read the file
                var lines = File.ReadAllLines(filePath, encoding: Encoding.GetEncoding(1252));

                if (lines.Length == 0)
                {
                    logger.LogError($"FixCsvFile - The file at '{filePath}' is empty");
                    return string.Empty;
                }

                var newLines = new List<string>();

                var headerCount = lines[0].Split(';').Length;

                if (headerCount == 0)
                {
                    logger.LogError($"FixCsvFile - The first line of the file at '{filePath}' does not contain any headers");
                    return string.Empty;
                }

                logger.LogInformation($"FixCsvFile - Detected {headerCount} headers in the CSV file");

                for (int i = 0; i < lines.Length;)
                {
                    var line = lines[i];
                    var columns = line.Split(';');
                    var lineJump = 1;

                    while (columns.Length != headerCount)
                    {
                        if (columns.Length > headerCount)
                        {
                            logger.LogCritical($"FixCsvFile - Line {i + 1} has {columns.Length} columns, expected {headerCount}. Halting process.");
                            return string.Empty; // Critical error, cannot fix this line => needs manual intervention
                        }

                        logger.LogWarning($"FixCsvFile - Line {i + 1} has {columns.Length} columns, expected {headerCount}. Attempting to fix [i = {lineJump}]...");

                        // Likely the "original line" had some \n that were processed incorrectly
                        // Append lines[i + 1] to the current line and re-do the check

                        if (i + lineJump >= lines.Length)
                        {
                            logger.LogCritical($"FixCsvFile - Reached the end of the file while trying to fix line {i + 1}. Cannot continue.");
                            return string.Empty; // Cannot fix this line, reached the end of the file
                        }

                        // Append the next line to the current line
                        line += lines[i + lineJump];

                        // Re-split the line to check the number of columns again
                        columns = line.Split(';');

                        // increment lineJump
                        lineJump++;
                    }

                    newLines.Add(line);
                    i += lineJump;
                }

                // Write the fixed lines to the new filepath
                File.WriteAllLines(newFilePath, newLines, Encoding.UTF8);

                logger.LogInformation($"FixCsvFile - Successfully fixed CSV file at {newFilePath}");
                return newFilePath;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"FixCsvFile - Error fixing CSV file at {filePath}");
                return string.Empty;
            }
        }
    }
}
