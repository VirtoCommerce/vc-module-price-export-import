using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Models;
using VirtoCommerce.SimpleExportImportModule.Core.Services;
using VirtoCommerce.SimpleExportImportModule.Data.Models;

namespace VirtoCommerce.SimpleExportImportModule.Data.Services
{
    public class CsvPriceDataValidator : ICsvPriceDataValidator
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
                var stream = _blobStorageProvider.OpenRead(fileUrl);
                var csvConfiguration = new Configuration(CultureInfo.InvariantCulture) { Delimiter = ";" };
                using var streamReader = new StreamReader(stream);
                using var csvReader = new CsvReader(streamReader, csvConfiguration);

                var headerStr = await streamReader.ReadLineAsync();

                if (headerStr == null || headerStr.Trim() == "")
                {
                    errorsList.Add(ModuleConstants.ValidationErrors.NoData);
                }
                else
                {
                    var headerColumns = headerStr.Split(csvConfiguration.Delimiter);

                    if (headerColumns.Length < 2)
                    {
                        errorsList.Add(ModuleConstants.ValidationErrors.WrongDelimiter);
                    }

                    var fistDataRowStr = await streamReader.ReadLineAsync();

                    if (fistDataRowStr == null || fistDataRowStr.Trim() == "")
                    {
                        errorsList.Add(ModuleConstants.ValidationErrors.NoData);
                    }
                }

                stream.Seek(0, SeekOrigin.Begin);
                streamReader.DiscardBufferedData();

                if (errorsList.Count == 0)
                {
                    try
                    {
                        csvReader.Read();
                        csvReader.ReadHeader();
                        csvReader.ValidateHeader<CsvPrice>();
                    }
                    catch (ValidationException e)
                    {
                        errorsList.Add(ModuleConstants.ValidationErrors.MissingRequiredColumns);
                        Debug.Write(e.Message);
                    }
                }

                if (errorsList.Count == 0)
                {
                    var totalCount = 0;

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

            var result = new ImportDataValidationResult { Errors = errorsList.ToArray() };

            return result;
        }
    }
}
