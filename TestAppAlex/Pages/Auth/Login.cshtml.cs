using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

public class LoginModel : PageModel
{
    [BindProperty]
    public LoginInput Input { get; set; } = new LoginInput();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        // No special logic on GET
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please fill all fields correctly.";
            return Page();
        }

        // TODO: Authenticate user from database
        // For now simulate a successful login if email contains "test" and password is "1234"

        if (Input.Email.Contains("test") && Input.Password == "1234")
        {
            // Successful fake login
            return RedirectToPage("/Home/Index");
        }
        else
        {
            ErrorMessage = "Invalid email or password.";
            return Page();
        }
    }

    public class LoginInput
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}
