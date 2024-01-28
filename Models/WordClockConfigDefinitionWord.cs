using System.Runtime.Serialization;

namespace kitchenview.Models
{
    public enum SpecialType
    {
        MERGE_WITH_NEXT,
        ALWAYS_ON,
        HOUR_WORD
    }

    [DataContract]
    public class WordClockConfigDefinitionWord
    {
        [DataMember(Name = "Word")]
        public string Word { get; set; }

        [DataMember(Name = "Value")]
        public int? Value { get; set; }

        [DataMember(Name = "Special")]
        public string? Special { get; set; }

        public bool IsEnabled { get; set; }
    }
}