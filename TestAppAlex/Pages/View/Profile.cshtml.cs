using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using TestAppAlex.Services;

namespace TestAppAlex.Pages.View
{
    public class ProfileModel : PageModel
    {
        private readonly IUserService _userService;

        public ProfileModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public string DisplayedName { get; set; } = "User";

        [BindProperty]
        public string SubscriptionType { get; set; } = "Basic";

        public void OnGet()
        {
         
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
            ?? User.Identity?.Name; // invalid email


            if (!string.IsNullOrEmpty(email))
            {
                await _userService.DeleteAccountAsync(email);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            return RedirectToPage("/Auth/Login");
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Auth/Login");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var email = User.Identity?.Name;
            if (!string.IsNullOrEmpty(email))
            {
                await _userService.UpdatePlanAsync(email, SubscriptionType);
                TempData["Message"] = "Plan actualizat!";
            }
            return Page();
        }
    }
}
