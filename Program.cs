using Lab1OuterSort;
using System.Diagnostics;

Console.WriteLine("Hello, World!");

var generator = new DataGenerator();
var path = generator.Generate("data.csv");

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();
Sorter sorter = new Sorter();
sorter.Sort("data.csv");
stopwatch.Stop();
Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}");

if (sorter.CheckIfSorted("data.csv"))
{
    Console.WriteLine("File sorted successfully.");
}
else
{
    Console.WriteLine("File not sorted correctly.");
}

//using (var writer = new StreamWriter("abc.csv"))
//using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
//{
//    csv.WriteHeader<Record>();
//    csv.NextRecord();
//    csv.WriteRecord(record);

//}

