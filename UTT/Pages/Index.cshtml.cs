using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UTT
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private UserManager<IdentityUser> _userManager;
        public IndexModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }
        
        public string UserName { get; set; }

        public void OnGet()
        {
            UserName = _userManager.GetUserName(User);
        }
    }
}