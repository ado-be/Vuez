using System.ComponentModel.DataAnnotations;

namespace vuez {

    public class Indicators
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Type { get; set; }

        public string Producer { get; set; }

        public string List_Num { get; set; }
    }
}