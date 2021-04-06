using System.Threading.Tasks;
using Moq;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Data.Services;
using Xunit;

namespace VirtoCommerce.SimpleExportImportModule.Tests
{
    [Trait("Category", "CI")]
    public class CsvPriceDataValidatorTests
    {
        private const string CsvFileContentWithoutError = @"SKU;Product Name;Currency;List price;Sale price;Min quantity;Modified;Valid from;Valid to;Created date;Created by;Modified By
41MY02;""1-1/4"""" Steel Hex Flange Bolt, Grade 5, Zinc Plated Finish, 5/16""""-18 Dia/Thread Size, 50 PK"";USD;9.3000;;1;09/17/2019 10:01:41;;;09/17/2019 10:01:41;unknown;unknown
4GVA7;""1"""" Steel Carriage Bolt, Grade 5, Chrome Plated Finish, 1/4""""-20 Dia/Thread Size, 5 PK"";USD;13.1000;9.0000;11;03/24/2021 07:15:43;;;09/17/2019 10:01:41;unknown;admin
19N051;""3-3/4"""" Alloy Steel Bolt, Grade 8, 3/4""""-10 Dia/Thread Size, 10 PK"";USD;108.0000;;1;09/17/2019 10:01:41;;;09/17/2019 10:01:41;unknown;unknown";

        private const string CsvFileWithWrongDelimiter = @"SKU|List price|Sale price|Min quantity
563123605|10|9|1
";
        private const string CsvFileWithWrongHeader = @"SKU;List price;Sales price;Min quantity
563123605;10;9;1
";

        private const string CsvFileWithoutData = @"";

        private const string CsvFileWithoutDataWithHeader = @"SKU;List price;Sales price;Min quantity";

        private const string CsvHeader = "SKU;Min quantity;List price;Sale price";

        private const string CsvRecord = "TestSku;1;100;99";


        [Fact]
        public async Task Validate_FileNotExists_ReturnErrorCode()
        {
            // Arrange
            var blobStorageProviderMoq = new Mock<IBlobStorageProvider>();

            blobStorageProviderMoq.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .ReturnsAsync((BlobInfo)null);

            var settingsManagerMoq = GetSettingsManagerMoq();

            var validator = new CsvPriceDataValidator(blobStorageProviderMoq.Object, settingsManagerMoq.Object);

            // Act
            var result = await validator.ValidateAsync("file url");

            // Assert
            Assert.Single(result.Errors);
            Assert.True(result.Errors[0].ErrorCode == ModuleConstants.ValidationErrors.FileNotExisted);
        }

        [Fact]
        public async Task Validate_FileWithLargeSize_ReturnErrorCode()
        {
            // Arrange
            var blobStorageProviderMoq = new Mock<IBlobStorageProvider>();

            var blobInfo = new BlobInfo() { Size = (int)ModuleConstants.Settings.General.ImportFileMaxSize.DefaultValue + 1 };
            blobStorageProviderMoq.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(blobInfo));

            var settingsManagerMoq = GetSettingsManagerMoq();

            var validator = new CsvPriceDataValidator(blobStorageProviderMoq.Object, settingsManagerMoq.Object);

            // Act
            var result = await validator.ValidateAsync("file url");

            // Assert
            Assert.Single(result.Errors);
            Assert.True(result.Errors[0].ErrorCode == ModuleConstants.ValidationErrors.ExceedingFileMaxSize);
        }


        [Fact]
        public async Task Validate_FileWithoutError_ReturnEmptyErrors()
        {
            // Arrange
            var blobStorageProviderMoq = new Mock<IBlobStorageProvider>();

            var blobInfo = new BlobInfo() { Size = (int)ModuleConstants.Settings.General.ImportFileMaxSize.DefaultValue };
            blobStorageProviderMoq.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(blobInfo));

            var stream = TestHelper.GetStream(CsvFileContentWithoutError);
            blobStorageProviderMoq.Setup(x => x.OpenRead(It.IsAny<string>()))
                .Returns(stream);

            var settingsManagerMoq = GetSettingsManagerMoq();

            var validator = new CsvPriceDataValidator(blobStorageProviderMoq.Object, settingsManagerMoq.Object);

            // Act
            var result = await validator.ValidateAsync("file url");

            // Assert
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task Validate_WongDelimiter_ReturnErrorCode()
        {
            // Arrange
            var blobStorageProviderMoq = new Mock<IBlobStorageProvider>();

            var blobInfo = new BlobInfo() { Size = (int)ModuleConstants.Settings.General.ImportFileMaxSize.DefaultValue };
            blobStorageProviderMoq.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(blobInfo));

            var stream = TestHelper.GetStream(CsvFileWithWrongDelimiter);
            blobStorageProviderMoq.Setup(x => x.OpenRead(It.IsAny<string>()))
                .Returns(stream);

            var settingsManagerMoq = GetSettingsManagerMoq();

            var validator = new CsvPriceDataValidator(blobStorageProviderMoq.Object, settingsManagerMoq.Object);

            // Act
            var result = await validator.ValidateAsync("file url");

            // Assert
            Assert.Single(result.Errors);
            Assert.True(result.Errors[0].ErrorCode == ModuleConstants.ValidationErrors.WrongDelimiter);
        }


        [Fact]
        public async Task Validate_WrongHeader_ReturnErrorCode()
        {
            // Arrange
            var blobStorageProviderMoq = new Mock<IBlobStorageProvider>();

            var blobInfo = new BlobInfo() { Size = (int)ModuleConstants.Settings.General.ImportFileMaxSize.DefaultValue };
            blobStorageProviderMoq.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(blobInfo));

            var stream = TestHelper.GetStream(CsvFileWithWrongHeader);
            blobStorageProviderMoq.Setup(x => x.OpenRead(It.IsAny<string>()))
                .Returns(stream);

            var settingsManagerMoq = GetSettingsManagerMoq();

            var validator = new CsvPriceDataValidator(blobStorageProviderMoq.Object, settingsManagerMoq.Object);

            // Act
            var result = await validator.ValidateAsync("file url");

            // Assert
            Assert.Single(result.Errors);
            Assert.True(result.Errors[0].ErrorCode == ModuleConstants.ValidationErrors.MissingRequiredColumns);
        }

        [Fact]
        public async Task Validate_ExceedingLineLimits_ReturnErrorCode()
        {
            // Arrange
            var blobStorageProviderMoq = new Mock<IBlobStorageProvider>();

            var blobInfo = new BlobInfo() { Size = (int)ModuleConstants.Settings.General.ImportFileMaxSize.DefaultValue };
            blobStorageProviderMoq.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(blobInfo));

            var records = TestHelper.GetArrayOfSameRecords(CsvRecord, ModuleConstants.Settings.ImportLimitOfLines + 1);
            var stream = TestHelper.GetStream(TestHelper.GetCsv(records, CsvHeader));
            blobStorageProviderMoq.Setup(x => x.OpenRead(It.IsAny<string>()))
                .Returns(stream);

            var settingsManagerMoq = GetSettingsManagerMoq();

            var validator = new CsvPriceDataValidator(blobStorageProviderMoq.Object, settingsManagerMoq.Object);

            // Act
            var result = await validator.ValidateAsync("file url");

            // Assert
            Assert.Single(result.Errors);
            Assert.True(result.Errors[0].ErrorCode == ModuleConstants.ValidationErrors.ExceedingLineLimits);
        }

        [Theory]
        [InlineData(CsvFileWithoutData)]
        [InlineData(CsvFileWithoutDataWithHeader)]

        public async Task Validate_NoData_ReturnErrorCode(string csv)
        {
            // Arrange
            var blobStorageProviderMoq = new Mock<IBlobStorageProvider>();

            var blobInfo = new BlobInfo() { Size = (int)ModuleConstants.Settings.General.ImportFileMaxSize.DefaultValue };
            blobStorageProviderMoq.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(blobInfo));

            var stream = TestHelper.GetStream(csv);
            blobStorageProviderMoq.Setup(x => x.OpenRead(It.IsAny<string>()))
                .Returns(stream);

            var settingsManagerMoq = GetSettingsManagerMoq();

            var validator = new CsvPriceDataValidator(blobStorageProviderMoq.Object, settingsManagerMoq.Object);

            // Act
            var result = await validator.ValidateAsync("file url");

            // Assert
            Assert.Single(result.Errors);
            Assert.True(result.Errors[0].ErrorCode == ModuleConstants.ValidationErrors.NoData);
        }

        private static Mock<ISettingsManager> GetSettingsManagerMoq()
        {
            var settingsManagerMoq = new Mock<ISettingsManager>();

            settingsManagerMoq.Setup(x =>
                    x.GetObjectSettingAsync(
                        It.Is<string>(x => x == ModuleConstants.Settings.General.ImportFileMaxSize.Name),
                        null, null))
                .ReturnsAsync(new ObjectSettingEntry()
                { Value = ModuleConstants.Settings.General.ImportFileMaxSize.DefaultValue });

            settingsManagerMoq.Setup(x =>
                    x.GetObjectSettingAsync(
                        It.Is<string>(x => x == ModuleConstants.Settings.General.ImportLimitOfLines.Name),
                        null, null))
                .ReturnsAsync(new ObjectSettingEntry()
                { Value = ModuleConstants.Settings.General.ImportLimitOfLines.DefaultValue });
            return settingsManagerMoq;
        }
    }
}
