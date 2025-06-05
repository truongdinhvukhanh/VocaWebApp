using System.ComponentModel.DataAnnotations;

namespace VocabWebApp.Models
{
    public class VocabItem
    {
        public int Id { get; set; }

        [Required]
        public int VocaSetId { get; set; }
        public VocabSet VocaSet { get; set; }

        [Required]
        [MaxLength(100)]
        public string Word { get; set; }

        [MaxLength(50)]
        public string WordType { get; set; }

        [MaxLength(100)]
        public string Pronunciation { get; set; }

        [MaxLength(255)]
        public string AudioUrl { get; set; }

        [MaxLength(255)]
        public string Meaning { get; set; }

        public string ExampleSentence { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "not_learned";

        public ICollection<VocabItemHistory> Histories { get; set; }
    }

}
