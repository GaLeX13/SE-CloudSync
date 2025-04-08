using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

public class RegisterModel : PageModel
{
    [BindProperty]
    public RegisterInput Input { get; set; } = new RegisterInput();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        // Nothing to do here yet
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please fill all fields correctly.";
            return Page();
        }

        if (Input.Password != Input.ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return Page();
        }

        // TODO: Save user into database here
        // For now simulate success

        return RedirectToPage("/Auth/Login");
    }

    public class RegisterInput
    {
        [Required]
        public string Name { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = "";
    }
}
