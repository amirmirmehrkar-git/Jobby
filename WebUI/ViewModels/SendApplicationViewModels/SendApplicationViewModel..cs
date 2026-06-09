using System.Text.Json.Serialization;

namespace WebUI.ViewModels.SendApplicationViewModel
{
    public class SendApplicationViewModel
    {
        public class ContinueCheckResult
        {
            public bool IsFinal { get; set; }
            public string selector { get; set; } = "";
        }
        public class ApplyCheckResult
        {
            public bool found { get; set; }
            public bool newTab { get; set; }
            public string selector { get; set; }
        }
        public class FormInputOption
        {
            [JsonPropertyName("value")]
            public string Value { get; set; }

            [JsonPropertyName("label")]
            public string Label { get; set; }
        }
        public class FormInputField
        {
            [JsonPropertyName("inputLabel")]
            public string InputLabel { get; set; }

            [JsonPropertyName("inputSelector")]
            public string InputSelector { get; set; }

            [JsonPropertyName("inputOptions")]
            public List<FormInputOption> InputOptions { get; set; } = new List<FormInputOption>();

            [JsonPropertyName("inputType")]
            public string InputType { get; set; }
        }
    }
}
