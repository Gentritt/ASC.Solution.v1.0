using ASC.Solution.v1._1.Models;
using ElCamino.AspNetCore.Identity.AzureTable.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using elCamino = ElCamino.AspNetCore.Identity.AzureTable.Model;

namespace ASC.Solution.v1._1.Data
{
    public interface IIdentitySeed
    {
        Task Seed(UserManager<ApplicationUser> userManager, RoleManager<elCamino.IdentityRole> roleManger, IOptions<ApplicationSettings> options);
    }
}
