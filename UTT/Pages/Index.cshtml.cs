using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UTT
{
    public class IndexModel : PageModel
    {
        public string UserName => User.Identity.Name;
    }
}