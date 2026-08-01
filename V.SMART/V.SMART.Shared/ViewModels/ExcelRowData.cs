namespace V.SMART.Shared.ViewModels
{
    public class ExcelRowData
    {
        public int RowIndex { get; set; }
        public Dictionary<string, string> Data { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public List<string> Cells { get; set; } = new();

    }
}
