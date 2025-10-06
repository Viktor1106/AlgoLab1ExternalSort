// See https://aka.ms/new-console-template for more information
using CsvHelper;
using Lab1OuterSort;
using System.Diagnostics;
using System.Globalization;

Console.WriteLine("Hello, World!");

var generator = new DataGenerator();
var path = generator.Generate("data.csv");

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();
Sorter sorter = new Sorter();
sorter.Sort("data.csv");
stopwatch.Stop();
Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}");

//using (var writer = new StreamWriter("abc.csv"))
//using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
//{
//    csv.WriteHeader<Record>();
//    csv.NextRecord();
//    csv.WriteRecord(record);

//}

