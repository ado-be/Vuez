using System.Collections.Generic;
using vuez.Models; // ak ConfigurationSheet a ProgramItem sú tu

namespace vuez.Models.ViewModels
{
    public class ConfigurationSheetViewModel
    {
        public ConfigurationSheet ConfigurationSheet { get; set; } = new ConfigurationSheet();
        public List<ProgramItem> ProgramItems { get; set; } = new List<ProgramItem>();
    }
}
