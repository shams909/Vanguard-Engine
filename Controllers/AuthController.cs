using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vanguard_Engine.Models;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null, string? error = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (error == "no_session")
        {
            TempData["ErrorMessage"] = "Secure Google connection could not be established. Please run the application over HTTPS (https://localhost:7012) to allow secure session cookies, or ensure third-party cookies are enabled in your browser.";
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _authService.LoginAsync(model);
        if (!result.Success)
        {
            if (result.IsEmailUnverified)
            {
                TempData["UnverifiedEmail"] = model.Email;
                ModelState.AddModelError(string.Empty, "Please verify your email before logging in.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Invalid email or password");
            }
            return View(model);
        }

        var user = result.User!;

        // INTERCEPT: Check if existing user has a phone number
        if (string.IsNullOrEmpty(user.PhoneNumber))
        {
            return RedirectToAction(nameof(UpdatePhone), new { userId = user.Id });
        }

        await SignInUser(user, model.RememberMe);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public async Task<IActionResult> LoginWithGoogle()
    {
        // Dynamically get the base URL from the current request
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var successUrl = $"{baseUrl}/google-success";
        var failureUrl = $"{baseUrl}/auth/login";
        
        var authUrl = await _authService.GetOAuth2UrlAsync("google", successUrl, failureUrl);
        return Redirect(authUrl);
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> OAuthCallback(string? userId, string? secret)
    {
        var finalUserId = userId ?? Request.Query["userId"];
        var finalSecret = secret ?? Request.Query["secret"];

        if (string.IsNullOrEmpty(finalUserId) || string.IsNullOrEmpty(finalSecret))
        {
            return RedirectToAction(nameof(Login));
        }

        var result = await _authService.HandleOAuthCallbackAsync(finalUserId, finalSecret);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Login));
        }

        if (result.IsNewUser)
        {
            var model = new CompleteProfileViewModel
            {
                AppwriteUserId = result.AppwriteUserId!,
                Email = result.Email!,
                Username = result.Name ?? ""
            };
            return View("CompleteProfile", model);
        }

        if (!result.IsNewUser && result.User != null)
        {
            // INTERCEPT: Check if existing Google user has a phone number
            if (string.IsNullOrEmpty(result.User.PhoneNumber))
            {
                return RedirectToAction(nameof(UpdatePhone), new { userId = result.User.Id });
            }
            
            await SignInUser(result.User);
            return RedirectToAction("Index", "Dashboard");
        }

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult CompleteProfile()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteProfile(CompleteProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var success = await _authService.CreateProfileAsync(model);
        if (success)
        {
            // Login after profile creation
            var user = await _authService.GetUserByIdAsync(model.AppwriteUserId);
            if (user != null)
            {
                await SignInUser(user);
                return RedirectToAction("Index", "Dashboard");
            }
            
            TempData["SuccessMessage"] = "Profile completed! Please log in.";
            return RedirectToAction(nameof(Login));
        }

        ModelState.AddModelError(string.Empty, "Failed to complete profile.");
        return View(model);
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [Route("google-success")]
    public IActionResult GoogleSuccess()
    {
        return Content($@"
            <div style='font-family:sans-serif; text-align:center; padding:50px; background:#f3ede1; min-height:100vh; display:flex; align-items:center; justify-content:center;'>
                <div style='max-width:500px; width:100%; padding:40px; background:#fff; border-radius:24px; box-shadow:0 10px 40px rgba(0,0,0,0.05);'>
                    <div style='width:50px; height:50px; border:4px solid #f3ede1; border-top:4px solid #b45309; border-radius:50%; margin:0 auto 24px; animation:spin 1s linear infinite;'></div>
                    <h2 style='color:#292524; margin:0 0 10px; font-weight:800;'>Verifying Account...</h2>
                    <p style='color:#78716c; font-size:0.95rem;' id='status'>Finalizing your secure connection.</p>
                </div>

                <script src='https://cdn.jsdelivr.net/npm/appwrite@14.0.0'></script>
                <script>
                    const client = new Appwrite.Client()
                        .setEndpoint('https://fra.cloud.appwrite.io/v1')
                        .setProject('6a018c59002ede066bcc');
                    const account = new Appwrite.Account(client);

                    async function run() {{
                        // 1. Try URL Tokens
                        const hashParams = new URLSearchParams(window.location.hash.substring(1));
                        const queryParams = new URLSearchParams(window.location.search);
                        const userId = hashParams.get('userId') || queryParams.get('userId');
                        const secret = hashParams.get('secret') || queryParams.get('secret');

                        if (userId && secret) {{
                            window.location.href = '/auth/oauthcallback?userId=' + encodeURIComponent(userId) + '&secret=' + encodeURIComponent(secret);
                            return;
                        }}

                        // 2. Fallback: Ask the Browser (SDK)
                        try {{
                            const user = await account.get();
                            window.location.href = '/auth/oauthcallback?userId=' + user.$id + '&secret=' + user.$id;
                        }} catch (e) {{
                            window.location.href = '/auth/login?error=no_session';
                        }}
                    }}
                    run();
                </script>
                <style>@@keyframes spin {{ 0% {{ transform: rotate(0deg); }} 100% {{ transform: rotate(360deg); }} }}</style>
            </div>
        ", "text/html");
    }

    [HttpGet]
    public IActionResult UpdatePhone(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return RedirectToAction(nameof(Login));
        
        return View(new UpdatePhoneViewModel { UserId = userId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePhone(UpdatePhoneViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var success = await _authService.UpdatePhoneNumberAsync(model.UserId, model.PhoneNumber);
        if (success)
        {
            var user = await _authService.GetUserByIdAsync(model.UserId);
            if (user != null)
            {
                await SignInUser(user);
                return RedirectToAction("Index", "Dashboard");
            }
        }

        ModelState.AddModelError(string.Empty, "Failed to update phone number.");
        return View(model);
    }

    private async Task SignInUser(Vanguard_Engine.Entities.User user, bool isPersistent = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role?.RoleName ?? "Guard")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var result = await _authService.RegisterAsync(model, baseUrl);
        if (result)
        {
            TempData["SuccessMessage"] = "Registration successful! A verification link has been sent to your email. Please verify your account before logging in.";
            return RedirectToAction(nameof(Login));
        }

        ModelState.AddModelError(string.Empty, "Registration failed. Email might already be in use.");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> VerifyEmail(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            TempData["ErrorMessage"] = "Invalid verification link.";
            return RedirectToAction(nameof(Login));
        }

        var success = await _authService.VerifyEmailAsync(userId, token);
        if (success)
        {
            TempData["SuccessMessage"] = "Your email has been verified successfully! You can now log in.";
        }
        else
        {
            TempData["ErrorMessage"] = "Verification link is invalid or has expired. Please request a new verification link below.";
            
            // Premium UX: Auto-populate the unverified email so the resend button appears instantly
            var user = await _authService.GetUserByIdAsync(userId);
            if (user != null && !user.IsEmailVerified)
            {
                TempData["UnverifiedEmail"] = user.Email;
            }
        }

        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendVerification(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            TempData["ErrorMessage"] = "Email address is required.";
            return RedirectToAction(nameof(Login));
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var success = await _authService.ResendVerificationEmailAsync(email, baseUrl);
        if (success)
        {
            TempData["SuccessMessage"] = "A fresh verification link has been sent to your email.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to resend verification. The email may already be verified or the account doesn't exist.";
        }

        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // 1. Sign out of our app
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // 2. Return a cleanup page that wipes the Appwrite session from the browser
        return Content($@"
            <div style='font-family:sans-serif; text-align:center; padding:50px; background:#f3ede1; min-height:100vh; display:flex; align-items:center; justify-content:center;'>
                <div style='max-width:400px; width:100%; padding:40px; background:#fff; border-radius:24px; box-shadow:0 10px 40px rgba(0,0,0,0.05);'>
                    <div style='width:50px; height:50px; border:4px solid #f3ede1; border-top:4px solid #b45309; border-radius:50%; margin:0 auto 24px; animation:spin 1s linear infinite;'></div>
                    <h2 style='color:#292524; margin:0 0 10px; font-weight:800;'>Signing Out...</h2>
                    <p style='color:#78716c; font-size:0.95rem;'>Cleaning up your secure session.</p>
                </div>
                <script src='https://cdn.jsdelivr.net/npm/appwrite@14.0.0'></script>
                <script>
                    const client = new Appwrite.Client()
                        .setEndpoint('https://fra.cloud.appwrite.io/v1')
                        .setProject('6a018c59002ede066bcc');
                    const account = new Appwrite.Account(client);
                    
                    account.deleteSession('current')
                        .finally(() => {{
                            window.location.href = '/auth/login?loggedout=true';
                        }});
                </script>
                <style>@@keyframes spin {{ 0% {{ transform: rotate(0deg); }} 100% {{ transform: rotate(360deg); }} }}</style>
            </div>
        ", "text/html");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // ─── Forgot Password ────────────────────────────────────────────────────────

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        // Always show success to avoid email enumeration attacks
        await _authService.ForgotPasswordAsync(model.Email, baseUrl);

        TempData["SuccessMessage"] = "If that email is registered, we've sent a secure password reset link. Please check your inbox (and spam folder).";
        return View(model);
    }

    // ─── Reset Password ─────────────────────────────────────────────────────────

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> ResetPassword(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            TempData["ErrorMessage"] = "Invalid password reset link.";
            return RedirectToAction(nameof(Login));
        }

        var isValid = await _authService.ValidateResetTokenAsync(token);
        if (!isValid)
        {
            TempData["ErrorMessage"] = "This password reset link is invalid or has expired. Please request a new one.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        return View(new ResetPasswordViewModel { Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var success = await _authService.ResetPasswordAsync(model.Token, model.Password);
        if (!success)
        {
            TempData["ErrorMessage"] = "This password reset link is invalid or has expired. Please request a new one.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        TempData["SuccessMessage"] = "Password updated successfully. Please login with your new password.";
        return RedirectToAction(nameof(Login));
    }
}
