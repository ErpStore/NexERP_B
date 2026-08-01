using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InstantSerchViewModel
{
    public class SearchFieldModel
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }

        public string? Value { get; set; }
        public decimal? NumberValue { get; set; }
        public DateTime? DateValue { get; set; }
        public bool BoolValue { get; set; }
        public string? DropdownValue { get; set; }
        public List<string> Options { get; set; } = new();

        public SearchFieldModel(string key, string label, string type, List<string>? options = null)
        {
            Key = key;
            Label = label;
            Type = type;
            if (options != null)
                Options = options;
        }

        public string? GetValueAsString() => Type switch
        {
            "text" => Value,
            "number" => NumberValue?.ToString(),
            "date" => DateValue?.ToString("yyyy-MM-dd"),
            "bool" => BoolValue ? "true" : null,
            "dropdown" => DropdownValue,
            _ => null
        };
    }
}
