using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace vuez.Models.ViewModels
{
    public class ProgramItemConfigViewModel
    {
        [BindNever]
        public ProgramItem Item { get; set; } = new ProgramItem();

        public ProgramItemDetail Detail { get; set; } = new ProgramItemDetail();

        [BindNever]
        public ConfigurationSheet ConfigurationSheet { get; set; } = new ConfigurationSheet();

        [BindNever]
        public List<ProgramItem> AllItems { get; set; } = new List<ProgramItem>();

        // Přidané reference na nové modely
        public ProgramReview ProgramReview { get; set; }

        public ProgramVerification ProgramVerification { get; set; }

        public ProgramRelease ProgramRelease { get; set; }
    }
}