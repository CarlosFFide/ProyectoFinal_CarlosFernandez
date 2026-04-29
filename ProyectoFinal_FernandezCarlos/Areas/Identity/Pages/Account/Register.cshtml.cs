#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using ProyectoFinal_FernandezCarlos.Data;
using ProyectoFinal_FernandezCarlos.Models;

namespace ProyectoFinal_FernandezCarlos.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
            [EmailAddress(ErrorMessage = "Debe ingresar un correo electrónico válido.")]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; }

            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [StringLength(100, ErrorMessage = "La contraseña debe tener entre {2} y {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Debe confirmar la contraseña.")]
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare("Password", ErrorMessage = "La contraseña y su confirmación no coinciden.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuario estudiante creado correctamente.");

                    await _userManager.AddToRoleAsync(user, "Estudiante");

                    var estudiante = new Estudiante
                    {
                        UserId = user.Id
                    };

                    _context.Estudiantes.Add(estudiante);
                    await _context.SaveChangesAsync();

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new
                        {
                            area = "Identity",
                            userId = userId,
                            code = code,
                            returnUrl = returnUrl
                        },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(
                        Input.Email,
                        "Confirmación de correo",
                        $"Confirme su cuenta ingresando al siguiente enlace: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>confirmar correo</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new
                        {
                            email = Input.Email,
                            returnUrl = returnUrl
                        });
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return Redirect("/Estudiante/Inicio");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, TraducirErrorIdentity(error));
                }
            }

            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException(
                    $"No se pudo crear una instancia de '{nameof(ApplicationUser)}'. Verifique que la clase no sea abstracta y tenga un constructor sin parámetros.");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("El sistema requiere soporte de correo electrónico para registrar usuarios.");
            }

            return (IUserEmailStore<ApplicationUser>)_userStore;
        }

        private string TraducirErrorIdentity(IdentityError error)
        {
            return error.Code switch
            {
                "DuplicateUserName" => "Ya existe un usuario registrado con este correo electrónico.",
                "DuplicateEmail" => "Ya existe un usuario registrado con este correo electrónico.",
                "InvalidUserName" => "El correo electrónico ingresado no es válido como nombre de usuario.",
                "InvalidEmail" => "El correo electrónico ingresado no es válido.",
                "PasswordTooShort" => "La contraseña es demasiado corta.",
                "PasswordRequiresNonAlphanumeric" => "La contraseña debe incluir al menos un carácter especial.",
                "PasswordRequiresDigit" => "La contraseña debe incluir al menos un número.",
                "PasswordRequiresLower" => "La contraseña debe incluir al menos una letra minúscula.",
                "PasswordRequiresUpper" => "La contraseña debe incluir al menos una letra mayúscula.",
                "PasswordRequiresUniqueChars" => "La contraseña debe incluir más variedad de caracteres.",
                _ => error.Description
            };
        }
    }
}