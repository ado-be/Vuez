using Microsoft.AspNetCore.Mvc;

namespace vuez.Models
{
    public class PdfCreateModel 
    {
        public int Id { get; set; } // Voliteľné, ak potrebujete primárny kľúč

        public string ProtocolName { get; set; } // Názov protokolu
        public string ProtocolSE { get; set; } // Číslo protokolu SE
        public string ProtocolSupplier { get; set; } // Číslo protokolu dodávateľa
        public int ListCount { get; set; } // Počet listov

        public string Type { get; set; } // Typ dokumentu
        public string Producer { get; set; } // Producent
        public string List_Num { get; set; } // Číslo zoznamu
    }
}
