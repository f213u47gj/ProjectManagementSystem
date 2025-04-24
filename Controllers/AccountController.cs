using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.IRepositories;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.ViewModels.forAccount;

namespace ProjectManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(IUserRepository userRepository, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _signInManager = signInManager;
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Projects");

            if (ModelState.IsValid)
            {
                var result = await _userRepository.LoginUserAsync(model.UserName, model.Password, model.RememberMe);
                if (result)
                {
                    return RedirectToAction("Index", "Projects");
                }
                ModelState.AddModelError(string.Empty, "Неправильное имя пользователя или пароль");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _userRepository.LogoutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationViewModel model)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Projects");

            if (ModelState.IsValid)
            {
                var result = await _userRepository.RegisterUserAsync(model);
                if (result)
                {
                    return RedirectToAction("Index", "Projects");
                }
                ModelState.AddModelError(string.Empty, "Ошибка регистрации. Проверьте данные.");
            }
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userRepository.GetCurrentUserAsync(userId);

            if (user == null) return NotFound();

            var model = new ProfileViewModel
            {
                AvatarUrl = user.AvatarUrl,
                UserName = user.UserName
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _userRepository.UpdateUserProfileAsync(User, model);
            if (!result)
            {
                ModelState.AddModelError("", "Не удалось обновить профиль.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Профиль обновлён";
            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, errors) = await _userRepository.ChangePasswordAsync(User, model.OldPassword, model.NewPassword);
            if (success)
            {
                TempData["SuccessMessage"] = "Пароль успешно изменён";
                return RedirectToAction(nameof(Profile));
            }

            if (errors != null)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Ошибка смены пароля.");
            }

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangeEmail()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _userRepository.ChangeEmailAsync(User, model.NewEmail);
            if (result)
            {
                TempData["SuccessMessage"] = "Почта обновлена";
                return RedirectToAction(nameof(Profile));
            }

            ModelState.AddModelError("", "Ошибка при обновлении почты.");
            return View(model);
        }
    }
}
