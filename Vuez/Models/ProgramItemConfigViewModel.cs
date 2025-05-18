namespace vuez.Models.ViewModels
{
    public class ProgramItemConfigViewModel
    {
        public ProgramItem Item { get; set; } = new ProgramItem();
        public ProgramItemDetail Detail { get; set; } = new ProgramItemDetail();
        public ConfigurationSheet ConfigurationSheet { get; set; } = new ConfigurationSheet();
        public List<ProgramItem> AllItems { get; set; } = new List<ProgramItem>();
    }
}
