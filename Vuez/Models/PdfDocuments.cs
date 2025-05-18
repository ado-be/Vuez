using Microsoft.AspNetCore.Mvc;

namespace vuez.Models
{
    public class PdfDocument 
    {
        public int Id { get; set; }               // ID PDF
        public string Name { get; set; }          // Názov súboru
        public byte[] FileData { get; set; }      // Obsah PDF
        public DateTime UploadedAt { get; set; }  // Dátum nahratia
    }
}
