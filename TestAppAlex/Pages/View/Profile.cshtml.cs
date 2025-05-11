using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestAppAlex.Pages.View
{
    public class ProfileModel : PageModel
    {
        [BindProperty]
        public string DisplayedName { get; set; } = "User";

        [BindProperty]
        public string SubscriptionType { get; set; } = "Gratuit";

        public void OnGet()
        {
            // Load data here if needed (e.g. from DB)
        }

        public IActionResult OnPost()
        {
            // Save logic here (DB, TempData, etc)
            TempData["Message"] = "Profil actualizat!";
            return Page();
        }
    }
}
