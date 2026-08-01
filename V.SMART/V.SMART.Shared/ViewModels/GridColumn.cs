namespace V.SMART.Shared.ViewModels
{
    public class GridColumn
    {
        public string Title { get; set; }
        public string Field { get; set; }
        public bool IsVisible { get; set; } = false;
        public bool IsDate { get; set; } = false;

        public string Width { get; set; } = "120px";
        public string Align { get; set; } = "text-center";

        public bool IsDetailColumn { get; set; }
    }
}
