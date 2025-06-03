using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using OpenCand.ETL.Contracts;

namespace OpenCand.Parser.Services
{
    public class CsvParserService<CsvObj> : IDisposable
    {
        private readonly ILogger<CsvParserService<CsvObj>> logger;
        private readonly CsvFixerService csvFixerService;
        private readonly IParserService<CsvObj> parserService;

        private readonly CsvConfiguration parserConfig;

        // Progress tracking fields
        private long processedCount;
        private long totalCount;
        private string currentTask = string.Empty;
        private Timer? progressTimer;
        private readonly object progressLock = new object();

        public CsvParserService(
            ILogger<CsvParserService<CsvObj>> logger,
            IParserService<CsvObj> parserService,
            CsvFixerService csvFixerService)
        {
            this.logger = logger;
            this.csvFixerService = csvFixerService;
            this.parserService = parserService;

            parserConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                PrepareHeaderForMatch = args => args.Header.ToLower(),
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim,
                Encoding = System.Text.Encoding.UTF8
            };
        }

        public async Task ParseFolderAsync(string filePath)
        {
            logger.LogInformation($"ParseFolderAsync - Starting to parse '{filePath}'");

            filePath = csvFixerService.FixCsvFile(filePath);

            // Fix the CSV file if necessary
            if (string.IsNullOrEmpty(filePath))
            {
                logger.LogError($"ParseFolderAsync - Failed to fix CSV file at '{filePath}'");
                throw new InvalidOperationException($"Failed to fix CSV file at '{filePath}'");
            }

            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, parserConfig);
                var po = new ParallelOptions
                {
                    MaxDegreeOfParallelism = 40
                };

                //csv.Context.RegisterClassMap<ClassMap<CsvObj>>(); // optional for advanced mapping, not needed
                var records = csv.GetRecords<CsvObj>().ToList();

                StartProgressTracking($"Parsing {nameof(CsvObj)} - {Path.GetFileName(filePath)}", records.Count);

                await Parallel.ForEachAsync(records, po, async (record, ct) =>
                {
                    try
                    {
                        await parserService.ParseObject(record);

                        // Increment progress
                        IncrementProgress();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"ParseFolderAsync - Error processing:");
                        IncrementProgress();
                    }
                });

                StopProgressTracking();

                logger.LogInformation($"ParseFolderAsync - Finished parsing from {filePath}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ParseFolderAsync - Error parsing file {filePath}");
                throw;
            }
        }

        // Progress tracking methods
        private void StartProgressTracking(string taskName, long total)
        {
            lock (progressLock)
            {
                currentTask = taskName;
                processedCount = 0;
                totalCount = total;

                progressTimer?.Dispose();
                progressTimer = new Timer(LogProgress, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

                logger.LogInformation("Progress - Task: {Task}, Total: {Total}", currentTask, totalCount);
            }
        }

        private void IncrementProgress()
        {
            Interlocked.Increment(ref processedCount);
        }

        private void StopProgressTracking()
        {
            lock (progressLock)
            {
                progressTimer?.Dispose();
                progressTimer = null;

                // Log final progress
                var percentage = totalCount > 0 ? (double)processedCount / totalCount * 100 : 0;
                logger.LogInformation("Progress - Task: {Task}, Processed: {Processed}, Total: {Total}, Progress: {Percentage:F2}%",
                    currentTask, processedCount, totalCount, percentage);
            }
        }

        private void LogProgress(object? state)
        {
            lock (progressLock)
            {
                if (string.IsNullOrEmpty(currentTask)) return;

                var percentage = totalCount > 0 ? (double)processedCount / totalCount * 100 : 0;
                logger.LogInformation("Progress - Task: {Task}, Processed: {Processed}, Total: {Total}, Progress: {Percentage:F2}%",
                    currentTask, processedCount, totalCount, percentage);
            }
        }

        public void Dispose()
        {
            progressTimer?.Dispose();
        }
    }
}
