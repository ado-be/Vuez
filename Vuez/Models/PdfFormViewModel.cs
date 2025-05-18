using Microsoft.AspNetCore.Mvc;

namespace vuez.Models
{
    public class PdfFormViewModel : Controller
    {
        public PdfCreateModel PdfCreate { get; set; }
        public AnotherModel AnotherData { get; set; }
    }
}
