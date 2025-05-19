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

        public List<ProgramItem> AllItems { get; set; } = new List<ProgramItem>();
    }
}
