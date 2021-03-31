using System.IO;
using System.Text;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Data.Services;
using Xunit;

namespace VirtoCommerce.SimpleExportImportModule.Tests
{
    [Trait("Category", "CI")]
    public class CsvPriceDataValidatorTests
    {
        private const string CsvFileContentWithoutError = @"
SKU;Product Name;Currency;List price;Sale price;Min quantity;Modified;Valid from;Valid to;Created date;Created by;Modified By
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

        [Fact]
        public async Task Validate_FileNotExists_ReturnErrorCode()
        {
            // Arrange
            var blobStorageProviderMoq = new Mock<IBlobStorageProvider>();

            blobStorageProviderMoq.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .ReturnsAsync((BlobInfo)null);

            var validator = new CsvPriceDataValidator(blobStorageProviderMoq.Object);

            // Act
            var result = await validator.ValidateAsync("file url");

            // Assert
            Assert.Single(result.Errors);
            Assert.True(result.Errors[0] == ModuleConstants.ValidationErrors.FileNotExisted);
        }

        [Fact]
        public async Task Validate_FileWithLargeSize_ReturnErrorCode()
        {
            // Arrange
            var blobStorageProviderMoq = new Mock<IBlobStorageProvider>();

            var blobInfo = new BlobInfo() { Size = ModuleConstants.FileMaxSize + 1 };
            blobStorageProviderMoq.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(blobInfo));

            var validator = new CsvPriceDataValidator(blobStorageProviderMoq.Object);

            // Act
            var result = await validator.ValidateAsync("file url");

            // Assert
            Assert.Single(result.Errors);
            Assert.True(result.Errors[0] == ModuleConstants.ValidationErrors.ExceedingFileMaxSize);
        }


        [Fact]
        public async Task Validate_FileWithoutError_ReturnEmptyErrors()
        {
            // Arrange
            var blobStorageProviderMoq = new Mock<IBlobStorageProvider>();

            var blobInfo = new BlobInfo() { Size = ModuleConstants.FileMaxSize };
            blobStorageProviderMoq.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(blobInfo));

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(CsvFileContentWithoutError));
            blobStorageProviderMoq.Setup(x => x.OpenRead(It.IsAny<string>()))
                .Returns(stream);

            var validator = new CsvPriceDataValidator(blobStorageProviderMoq.Object);

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

            var blobInfo = new BlobInfo() { Size = ModuleConstants.FileMaxSize };
            blobStorageProviderMoq.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(blobInfo));

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(CsvFileWithWrongDelimiter));
            blobStorageProviderMoq.Setup(x => x.OpenRead(It.IsAny<string>()))
                .Returns(stream);

            var validator = new CsvPriceDataValidator(blobStorageProviderMoq.Object);

            // Act
            var result = await validator.ValidateAsync("file url");

            // Assert
            Assert.Single(result.Errors);
            Assert.True(result.Errors[0] == ModuleConstants.ValidationErrors.WrongDelimiter);
        }


        [Fact]
        public async Task Validate_WrongHeader_ReturnErrorCode()
        {
            // Arrange
            var blobStorageProviderMoq = new Mock<IBlobStorageProvider>();

            var blobInfo = new BlobInfo() { Size = ModuleConstants.FileMaxSize };
            blobStorageProviderMoq.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(blobInfo));

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(CsvFileWithWrongHeader));
            blobStorageProviderMoq.Setup(x => x.OpenRead(It.IsAny<string>()))
                .Returns(stream);

            var validator = new CsvPriceDataValidator(blobStorageProviderMoq.Object);

            // Act
            var result = await validator.ValidateAsync("file url");

            // Assert
            Assert.Single(result.Errors);
            Assert.True(result.Errors[0] == ModuleConstants.ValidationErrors.MissingRequiredColumns);
        }

        [Fact]
        public async Task Validate_NoData_ReturnErrorCode()
        {
            // Arrange
            var blobStorageProviderMoq = new Mock<IBlobStorageProvider>();

            var blobInfo = new BlobInfo() { Size = ModuleConstants.FileMaxSize };
            blobStorageProviderMoq.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(blobInfo));

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(CsvFileWithoutData));
            blobStorageProviderMoq.Setup(x => x.OpenRead(It.IsAny<string>()))
                .Returns(stream);

            var validator = new CsvPriceDataValidator(blobStorageProviderMoq.Object);

            // Act
            var result = await validator.ValidateAsync("file url");

            // Assert
            Assert.Single(result.Errors);
            Assert.True(result.Errors[0] == ModuleConstants.ValidationErrors.NoData);
        }
    }
}
