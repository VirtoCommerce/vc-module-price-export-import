using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Models;
using VirtoCommerce.SimpleExportImportModule.Core.Services;
using VirtoCommerce.SimpleExportImportModule.Data.Models;

namespace VirtoCommerce.SimpleExportImportModule.Data.Services
{
    public sealed class CsvPriceDataValidator : ICsvPriceDataValidator
    {
        private readonly IBlobStorageProvider _blobStorageProvider;

        public CsvPriceDataValidator(IBlobStorageProvider blobStorageProvider)
        {
            _blobStorageProvider = blobStorageProvider;
        }
        public async Task<ImportDataValidationResult> ValidateAsync(string fileUrl)
        {
            var errorsList = new List<string>();

            var blobInfo = await _blobStorageProvider.GetBlobInfoAsync(fileUrl);

            if (blobInfo == null)
            {
                errorsList.Add(ModuleConstants.ValidationErrors.FileNotExisted);
            }
            else if (blobInfo.Size > ModuleConstants.FileMaxSize)
            {
                errorsList.Add(ModuleConstants.ValidationErrors.ExceedingFileMaxSize);
            }
            else
            {
                await using var stream = _blobStorageProvider.OpenRead(fileUrl);
                var csvConfiguration = new ImportConfiguration();
                using var streamReader = new StreamReader(stream);
                using var csvReader = new CsvReader(streamReader, csvConfiguration);

                await ValidateDelimiterAndDataExists(streamReader, csvConfiguration.Delimiter, errorsList);

                ValidateRequiredColumns(streamReader, csvReader, errorsList);

                ValidateLineLimit(csvReader, errorsList);
            }

            var result = new ImportDataValidationResult { Errors = errorsList.ToArray() };

            return result;
        }

        private static void ValidateLineLimit(CsvReader csvReader, List<string> errorsList)
        {
            if (errorsList.Count == 0)
            {
                ReadCsvHeader(csvReader);

                var totalCount = 1;

                while (csvReader.Read())
                {
                    totalCount++;
                }

                if (totalCount > ModuleConstants.ImportLimitOfLines)
                {
                    errorsList.Add(ModuleConstants.ValidationErrors.ExceedingLineLimits);
                }
            }
        }

        private static void ValidateRequiredColumns(StreamReader streamReader, CsvReader csvReader, List<string> errorsList)
        {
            SeekStreamReaderToStart(streamReader);

            if (errorsList.Count == 0)
            {
                try
                {
                    ReadCsvHeader(csvReader);
                    csvReader.ValidateHeader<CsvPrice>();
                }
                catch (ValidationException)
                {
                    errorsList.Add(ModuleConstants.ValidationErrors.MissingRequiredColumns);
                }
            }
        }

        private static async Task ValidateDelimiterAndDataExists(StreamReader streamReader, string delimiter, List<string> errorsList)
        {
            var headerLine = await streamReader.ReadLineAsync();

            if (headerLine == null || headerLine.Trim() == "")
            {
                errorsList.Add(ModuleConstants.ValidationErrors.NoData);
            }
            else
            {
                if (!headerLine.Contains(delimiter))
                {
                    errorsList.Add(ModuleConstants.ValidationErrors.WrongDelimiter);
                }

                var fistDataRowStr = await streamReader.ReadLineAsync();

                if (fistDataRowStr == null || fistDataRowStr.Trim() == "")
                {
                    errorsList.Add(ModuleConstants.ValidationErrors.NoData);
                }
            }
        }

        private static void ReadCsvHeader(CsvReader csvReader)
        {
            csvReader.Read();
            csvReader.ReadHeader();
        }

        private static void SeekStreamReaderToStart(StreamReader streamReader)
        {
            streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
            streamReader.DiscardBufferedData();
        }
    }
}
