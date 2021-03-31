using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.SimpleExportImportModule.Tests
{
    public static class TestHelper
    {
        public static async Task<Stream> GetStream(string csv)
        {
            var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream, leaveOpen: true);
            await writer.WriteAsync(csv);
            await writer.FlushAsync();
            stream.Position = 0;
            return stream;
        }

        public static string GetCsv(IEnumerable<string> records, string header = null)
        {
            var csv = new StringBuilder();

            if (header != null)
            {
                csv.AppendLine(header);
            }

            foreach (var record in records)
            {
                csv.AppendLine(record);
            }

            return csv.ToString();
        }

        public static string[] GetArrayOfSameRecords(string recordValue, long number)
        {
            var result = new List<string>();

            for (long i = 0; i < number; i++)
            {
                result.Add(recordValue);
            }

            return result.ToArray();
        }
    }
}
