namespace NodeGenerator.Options
{
    public class CsvOptions
    {
        public const string Section = "CSV";

        public string Delimiter { get; set; }
        public int DisplayNameIndex { get; set; }
        public int NodeIndex { get; set; }
        public bool SkipFirst { get; set; }
    }
}