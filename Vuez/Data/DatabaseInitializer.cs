using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using vuez.Models;

namespace vuez
{
    public static class DatabaseInitializer
    {
        public static async Task SeedAsync(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            // Seedovanie údajov pre tabuľku Indicators
            if (!context.Indicators.Any())
            {
                context.Indicators.AddRange(
                    new Indicators { Id = 1, Name = "Indicator1", Type = "TypeA", Producer = "Producer1", List_Num = "LN001" },
                    new Indicators { Id = 2, Name = "Indicator2", Type = "TypeB", Producer = "Producer2", List_Num = "LN002" }
                );
            }

            // Seedovanie údajov pre tabuľku PdfDocuments
            

           
          
        }

    
     
        }
    }

