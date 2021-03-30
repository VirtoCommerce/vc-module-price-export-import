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
            else if (blobInfo.Size < 1024 * 1024 * 8m)
            {
                errorsList.Add(ModuleConstants.ValidationErrors.ExceedingFileMinSize);
            }
            else if (blobInfo.Size > 1024 * 1024 * 1024 * 8m)
            {
                errorsList.Add(ModuleConstants.ValidationErrors.ExceedingFileMaxSize);
            }
            else
            {
                var stream = _blobStorageProvider.OpenRead(fileUrl);

                var csvConfiguration = new Configuration(CultureInfo.InvariantCulture) { Delimiter = ";" };
                var streamReader = new StreamReader(stream);
                //csvConfiguration.ReadingExceptionOccurred = ex => false;
                var csvReader = new CsvReader(streamReader, csvConfiguration);

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

            return new ImportDataValidationResult { Errors = errorsList.ToArray() };

        }
    }
}
