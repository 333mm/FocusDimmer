namespace FocusDimmer.Models
{
    public class ProcessPresetRule
    {
        public string ProcessName { get; set; } = "";  // 例: "notepad"
        public string DisplayName { get; set; } = "";  // 例: "Notepad" (FileDescription)
        public string PresetId { get; set; } = "";     // 適用するプリセットのID

        [System.Text.Json.Serialization.JsonIgnore]
        public string NameForUi => !string.IsNullOrEmpty(DisplayName) ? DisplayName : ProcessName;
    }
}
