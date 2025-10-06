using CsvHelper;
using System.Globalization;
using System.Reactive.Disposables;

namespace Lab1OuterSort
{
    internal class Sorter
    {
        public const int filesCount = 10;

        private readonly List<string> filesBpaths = new List<string> { "b1.csv", "b2.csv", "b3.csv", "b4.csv", "b5.csv", "b6.csv", "b7.csv", "b8.csv", "b9.csv", "b10.csv" };
        private readonly List<string> filesCpaths = new List<string> { "c1.csv", "c2.csv", "c3.csv", "c4.csv", "c5.csv", "c6.csv", "c7.csv", "c8.csv", "c9.csv", "c10.csv" };

        public void Sort(string pathToFileA)
        {
            
            WriteFromAtoBfiles(pathToFileA, filesCount, filesBpaths);

            string? finalSortedFilePath = null;
            do
            {
                SortXintoY(filesCount, filesBpaths, filesCpaths);

                if (!SecondFileHasRecords(filesCpaths))
                {
                    finalSortedFilePath = filesCpaths[0];
                    break;
                }

                SortXintoY(filesCount, filesCpaths, filesBpaths);
            } while (SecondFileHasRecords(filesBpaths)); // check if all files except the first are empty

            try
            {
                File.Replace((finalSortedFilePath ?? filesBpaths[0]), pathToFileA, "givenFileInBeforeSortState.csv");
            }
            catch (IOException iox)
            {
                Console.WriteLine(iox.Message);
            }

            CleanUpFiles();
        }

        private void CleanUpFiles()
        {
            foreach (var path in filesBpaths.Concat(filesCpaths))
            {
                try
                {
                    File.Delete(path);
                }
                catch (IOException iox)
                {
                    Console.WriteLine(iox.Message);
                }
            }
        }

        private void SortXintoY(int filesCount, List<string> filesXpaths, List<string> filesYpaths)
        {
            using (var Xreaders = CreateReaders(filesXpaths))
            using (var Ywriters = CreateWriters(filesYpaths))
            {
                WriteHeadersToWriters(Ywriters); // TODO: maybe move it to CreateWriters?

                var readers = Xreaders.OfType<CsvReader>().ToArray();
                var currentRecords = InitHeadsOfReadFiles(filesCount, readers); // currentRecords[i] - current record from i-th reader
                

                var writers = Ywriters.OfType<CsvWriter>().ToArray();
                var writerIndex = 0;
                do
                {
                    var max = currentRecords // won't ever be null because of the condition in while
                        .Aggregate((r1, r2) =>
                        {
                            if (r1 == null) return r2;
                            if (r2 == null) return r1;
                            return r1.Key > r2.Key ? r1 : r2;
                        }); // get max because we need sequences in descending order

                    writers[writerIndex].WriteRecord(max);
                    writers[writerIndex].NextRecord();

                    UpdateCurrentRecords(currentRecords, readers, max);

                    if (currentRecords.All(r => r == null)) // if all current sequences are finished
                    {
                        writerIndex = (writerIndex + 1) % filesCount; //move to the next c file
                        MoveToNextSequences(currentRecords, readers);
                    }
                } while (currentRecords.Any(r => r != null)); // check if there is at least one non-null record
            }
        }

        private Record[] InitHeadsOfReadFiles(int filesCount, CsvReader[] readers)
        {
            Record[] currentRecords = new Record[filesCount];
            for (int i = 0; i < filesCount; i++)
            {
                var reader = readers[i];
                reader.Read();
                reader.ReadHeader();
                if (reader.Read())
                    currentRecords[i] = reader.GetRecord<Record>();
            }
            return currentRecords;
        }

        private void WriteFromAtoBfiles(string pathToFileA, int filesCount, List<string> filesBpaths)
        {
            using (var reader = new StreamReader(pathToFileA))
            using (var A = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = A.GetRecords<Record>();
                Record? previousRecord = null;

                using (var Bwriters = CreateWriters(filesBpaths))
                {
                    int writerIndex = 0;
                    var BwritersArray = Bwriters.OfType<CsvWriter>().ToArray();
                    WriteHeadersToWriters(Bwriters);
                    foreach (var record in records)
                    {
                        if (!IsSequence(previousRecord, record))
                            writerIndex = (writerIndex + 1) % filesCount;
                        BwritersArray[writerIndex].WriteRecord(record);
                        BwritersArray[writerIndex].NextRecord();
                        previousRecord = record;
                    }
                }
            }
        }

        public bool CheckIfSorted(string path)
        {
            using (var reader = new StreamReader(path))
            using (var A = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = A.GetRecords<Record>();
                Record? previousRecord = null;
                foreach (var record in records)
                {
                    if (!IsSequence(previousRecord, record))
                        return false;
                    previousRecord = record;
                }
            }
            return true;
        }

        private bool SecondFileHasRecords(List<string> filesBpaths)
        {
            long length = new FileInfo(filesBpaths[1]).Length;
            const long headerLength = 18;
            return length > headerLength;
        }



        /// <summary>
        /// Changes currentRecords to the next records in the sequences elements, value remains if sequence ended (it will be null)
        /// </summary>
        /// <param name="currentRecords"></param>
        /// <param name="readers"></param>
        private void MoveToNextSequences(Record[] currentRecords, CsvReader[] readers)
        {
            for (var i = 0; i < readers.Length; i++)
            {
                var reader = readers[i];
                try
                {
                    currentRecords[i] = reader.GetRecord<Record>();
                }
                catch (Exception e)
                {
                    currentRecords[i] = null;
                }
            }
        }

        /// <summary>
        /// Writes null if next element will break the sequence
        /// </summary>
        /// <param name="currentRecords"></param>
        /// <param name="readers"></param>
        /// <param name="max"></param>
        private void UpdateCurrentRecords(Record[] currentRecords, CsvReader[] readers, Record max)
        {
            for (var i = 0; i<readers.Length; i++)
            {
                if (currentRecords[i] == max)
                {
                    var reader = readers[i];
                    if (reader.Read())
                    {
                        var nextRecord = reader.GetRecord<Record>();
                        if (IsSequence(currentRecords[i], nextRecord))
                            currentRecords[i] = nextRecord;
                        else
                            currentRecords[i] = null;
                    }
                    else
                        currentRecords[i] = null;
                    break;
                }
            }
        }

        private CompositeDisposable CreateWriters(List<string> paths)
        {
            var writers = new CompositeDisposable();
            foreach (var path in paths)
            {
                var writer = new StreamWriter(path);
                var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
                writers.Add(csvWriter);
            }
            return writers;
        }

        private CompositeDisposable CreateReaders(List<string> paths)
        {
            var readers = new CompositeDisposable();
            foreach (var path in paths)
            {
                var reader = new StreamReader(path);
                var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
                readers.Add(csvReader);
            }
            return readers;
        }

        private void WriteHeadersToWriters(CompositeDisposable writers)
        {
            foreach (CsvWriter writer in writers)
            {
                writer.WriteHeader<Record>();
                writer.NextRecord();
            }
        }

        private bool IsSequence(Record? previous, Record current)
        {
            if (previous == null)
                return true;
            return previous.Key >= current.Key;
        }
    }
}
