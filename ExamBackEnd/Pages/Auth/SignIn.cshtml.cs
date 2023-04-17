using ExamBackEnd.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Security.Claims;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;

namespace ExamBackEnd.Pages.Auth
{
    public class SignInModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public class SignInForm
        {
            [Required]
            public string Email { set; get; } = "";

            [Required]
            public string Password { set; get; } = "";
        }

        [BindProperty]
        public SignInForm Form { set; get; } = new SignInForm();

        public SignInModel(ApplicationDbContext applicationDbContext)
        {
            _db = applicationDbContext;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(string returnUrl, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid == false)
            {
                return Page();
            }

            var user = await _db.Users.AsNoTracking()
                .Where(Q => Q.Email == Form.Email)
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                ModelState.AddModelError("Form.Password", "Invalid email or password");
                return Page();
            }

            var correct = BCrypt.Net.BCrypt.Verify(Form.Password, user.Password);
            if (correct == false)
            {
                ModelState.AddModelError("Form.Password", "Invalid email or password");
                return Page();
            }

            var id = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            id.AddClaim(new Claim(Claims.Subject, user.Id));
            id.AddClaim(new Claim(Claims.Name, user.Name));
            id.AddClaim(new Claim(Claims.Email, user.Email));

            var principal = new ClaimsPrincipal(id);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // islocalurl lebih aman karena ngecek usernya diredirect kemana, biar nggak kena scam
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("/");
        }
    }
}
