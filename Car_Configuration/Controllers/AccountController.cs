using Car_Configuration.Entities;
using CarConfiguration.Dtoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Car_Configuration.Controllers;

public class AccountController : Controller
{
    private readonly RoleManager<UserRole> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    public AccountController(UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<UserRole> roleManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public IActionResult SignIn(string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(SignInDto signIn, string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid) return View();

        User user;
        if (signIn.UsernameOrEmail!.Contains("@"))
        {
            user = await _userManager.FindByEmailAsync(signIn.UsernameOrEmail);
        }
        else
        {
            user = await _userManager.FindByNameAsync(signIn.UsernameOrEmail);
        }

        if (user == null)
        {
            ModelState.AddModelError("", "Login or parol incorrect");
            return View(signIn);
        }

        var result = await _signInManager.PasswordSignInAsync(user, signIn.Password, true, true);

        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Login or parol incorrect");
            return View(signIn);
        }
        if (returnUrl != null) return Redirect(returnUrl);

        return Redirect("/");
    }


    public IActionResult Register(string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto register, string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid) return View();

        var userExists = await _userManager.FindByNameAsync(register.Username) != null;
        var emailExists = await _userManager.FindByEmailAsync(register.Email) != null;

        if (userExists)
        {
            ModelState.AddModelError("", "Username already exists.");
            return View(register);
        }

        if (emailExists)
        {
            ModelState.AddModelError("", "Email already exists.");
            return View(register);
        }

        User user = new User
        {
            Email = register.Email,
            UserName = register.Username
        };

        IdentityResult result = await _userManager.CreateAsync(user, register.Password);

        if (!result.Succeeded)
        {
            return View();
        }

        //await _signInManager.SignInAsync(user, true);

        return Redirect($"SignIn?returnUrl={returnUrl}");
    }

    public async Task<IActionResult> LogOut()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction(nameof(SignIn));
    }


    [HttpGet]
    [Authorize(Roles = "admin")]
    public IActionResult AddUserToRole() => View();

    [HttpPost]
    public async Task<IActionResult> AddUserToRole(string username, string rolename)
    {
        var user = await _userManager.FindByNameAsync(username);

        if (user == null)
        {
            ModelState.AddModelError("", "not found");
            return View();
        }
        try
        {
            await _userManager.AddToRolesAsync(user, new string[] { rolename });
        }
        catch
        {
            ModelState.AddModelError("", "error oocured may be not this role");
            return View();
        }

        return Redirect("/");
    }

    public ActionResult AccessDenied()
    {
        return View();
    }
}
