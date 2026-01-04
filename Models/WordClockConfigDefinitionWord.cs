using System.Runtime.Serialization;

namespace kitchenview.Models
{
    public enum SpecialType
    {
        NONE,
        MERGE_WITH_NEXT,
        ALWAYS_ON,
        HOUR_WORD,
        BEFORE,
        AFTER
    }

    public enum Type
    {
        HOUR,
        MINUTE
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

        [DataMember(Name = "Type")]
        public string? Type { get; set; }

        public bool IsEnabled { get; set; }
    }
}