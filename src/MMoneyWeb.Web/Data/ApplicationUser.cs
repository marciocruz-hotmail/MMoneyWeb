using Microsoft.AspNetCore.Identity;

namespace MMoneyWeb.Web.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public int? LancamentosIdContaCorrente { get; set; }

    public int? LancamentosIdCompetencia { get; set; }
}

