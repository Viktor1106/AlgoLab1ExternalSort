namespace Lab1OuterSort
{
    internal class Record
    {
        public double Key { get; set; }
        public string Value { get; set; }
        public DateOnly Date { get; set; }
        public Record(double Key, string Value, DateOnly Date)
        {
            this.Key = Key;
            this.Value = Value;
            this.Date = Date;
        }

        static public Record EOFRecord => new Record(double.MaxValue, string.Empty, DateOnly.MaxValue);

        public override string ToString()
        {
            return $"{Key},{Value}, {Date.ToString()}";
        }
    }
}
