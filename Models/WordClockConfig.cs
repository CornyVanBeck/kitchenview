using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace kitchenview.Models
{
    public enum SpaceFillerType
    {
        RANDOM_LETTER,
        DOT
    }

    [DataContract(Name = "WordClock")]
    public class WordClockConfig
    {
        [DataMember(Name = "SpaceFiller")]
        public string SpaceFiller { get; set; }

        [DataMember(Name = "Definitions")]
        public IEnumerable<WordClockConfigDefinition> Definitions { get; set; }
    }
}