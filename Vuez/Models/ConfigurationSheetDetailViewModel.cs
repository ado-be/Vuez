// VYTVORTE NOVÝ SÚBOR: Models/ViewModels/ConfigurationSheetDetailViewModel.cs

using System.Collections.Generic;

namespace vuez.Models.ViewModels
{
    public class ConfigurationSheetDetailViewModel
    {
        public ConfigurationSheet ConfigurationSheet { get; set; } = new ConfigurationSheet();
        public List<ProgramItem> ProgramItems { get; set; } = new List<ProgramItem>();
        public List<ProgramItemDetail> ProgramItemDetails { get; set; } = new List<ProgramItemDetail>();
        public List<ProgramReview> ProgramReviews { get; set; } = new List<ProgramReview>();
        public List<ProgramVerification> ProgramVerifications { get; set; } = new List<ProgramVerification>();
        public List<ProgramRelease> ProgramReleases { get; set; } = new List<ProgramRelease>();

        // Helper metódy pre získanie súvisiacich dát
        public ProgramItemDetail? GetDetailForItem(int itemId)
        {
            return ProgramItemDetails.FirstOrDefault(d => d.ItemId == itemId);
        }

        public ProgramReview? GetReviewForDetail(int detailId)
        {
            return ProgramReviews.FirstOrDefault(r => r.DetailId == detailId);
        }

        public ProgramVerification? GetVerificationForDetail(int detailId)
        {
            return ProgramVerifications.FirstOrDefault(v => v.DetailId == detailId);
        }

        public ProgramRelease? GetReleaseForDetail(int detailId)
        {
            return ProgramReleases.FirstOrDefault(r => r.DetailId == detailId);
        }
    }
}