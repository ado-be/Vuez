using System.Collections.Generic;
using vuez.Models; // ak ConfigurationSheet a ProgramItem sú tu

namespace vuez.Models.ViewModels
{
    public class ConfigurationConfigListViewModel
    {
        public ConfigurationSheet ConfigurationSheet { get; set; } = null!;
        public List<ProgramItem> ProgramItems { get; set; } = new();
    }

}