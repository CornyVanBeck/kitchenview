using System.Collections.Generic;
using System.Runtime.Serialization;

namespace kitchenview.Models
{
    [DataContract(Name = "Definition")]
    public class WordClockConfigDefinition
    {
        [DataMember(Name = "Index")]
        public int Index { get; set; }

        [DataMember(Name = "Words")]
        public IEnumerable<WordClockConfigDefinitionWord> Words { get; set; }
    }
}