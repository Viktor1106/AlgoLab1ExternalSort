using CsvHelper;
using System.Globalization;

namespace Lab1OuterSort
{
    internal class DataGenerator
    {
        /// <summary>
        /// Create file with random data
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>Path to the file with data</returns>
        public string Generate(string fileName)
        {
            using (var writer = new StreamWriter($"./{fileName}"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                var records = GenerateData(1000000); //10000000
                csv.WriteRecords(records);
            }

            return Path.GetFullPath($"./{fileName}");
        }

        private IEnumerable<Record> GenerateData(int count)
        {
            Random random = new Random();
            //var sequence = Enumerable.Range(0, count).Shuffle(random);
            //foreach (var key in sequence)
            for (int i=0; i<count; i++)
            {
                yield return new Record(
                    random.NextDouble(),
                    RandomString(20, random),
                    DateOnly.FromDateTime(DateTime.Now.AddDays(-random.Next(0, 365 * 5)))
                );
            }
        }

        // input sould be bigger than 51 bytes
        private int ByteSizeToRecordsCount(int byteSize)
        {
            const int recordSize = 51; // in bytes
            return (byteSize + recordSize - 1) / recordSize;
        }

        private static string RandomString(int length, Random random)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
