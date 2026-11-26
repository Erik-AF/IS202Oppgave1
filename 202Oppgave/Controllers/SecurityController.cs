using _202Oppgave.Models.Entities;
using _202Oppgave.Models.Authorization;
using _202Oppgave.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;

namespace _202Oppgave.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
[ResponseCache(Duration = 0,Location = ResponseCacheLocation.None,NoStore = true)]
public class SecurityController(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager,IDataProtectionProvider dataProtectionProvider): Controller
{
    private readonly ITimeLimitedDataProtector setupProtector =
    dataProtectionProvider
        .CreateProtector("Koordinert.2FA.Setup.v1")
        .ToTimeLimitedDataProtector();

    private static string CreateSetupBinding(ApplicationUser user,string key)
    {
        var value = $"{user.Id}:{user.SecurityStamp}:{key}";

        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        var model = new TwoFactorViewModel
        {
            Is2faEnabled = await userManager.GetTwoFactorEnabledAsync(user),

            HasAuthenticator = !string.IsNullOrEmpty(await userManager.GetAuthenticatorKeyAsync(user)),

            RecoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user),Is2faRequired = MfaRequirements.IsRequired(User)
        };

        return View(model);
    }

    private async Task<bool> VerifyCredentialsAsync(ApplicationUser user,ConfirmPasswordViewModel model,bool requireTwoFactor)
    {
        if (!ModelState.IsValid)
        {
            return false;
        }

        var passwordResult = await signInManager
            .CheckPasswordSignInAsync(
                user,
                model.Password,
                lockoutOnFailure: true);

        if (!passwordResult.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                "Kunne ikke bekrefte identiteten din.");

            return false;
        }

        if (requireTwoFactor)
        {
            var code = model.Code?.Trim() ?? string.Empty;

            var validCode = await userManager
                .VerifyTwoFactorTokenAsync(
                    user,
                    userManager.Options.Tokens
                        .AuthenticatorTokenProvider,
                    code);

            if (!validCode)
            {
                if (await userManager.GetLockoutEnabledAsync(user))
                {
                    await userManager.AccessFailedAsync(user);
                }

                ModelState.AddModelError(
                    string.Empty,
                    "Ugyldig sikkerhetskode.");

                return false;
            }

            var resetResult = await userManager
                .ResetAccessFailedCountAsync(user);

            if (!resetResult.Succeeded)
            {
                AddIdentityErrors(resetResult);
                return false;
            }
        }

        return true;
    }

    private async Task<EnableAuthenticatorViewModel?>  BuildAuthenticatorModelAsync(ApplicationUser user)
    {
        var key = await userManager
            .GetAuthenticatorKeyAsync(user);

        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        var email = Uri.EscapeDataString(
            user.Email ?? user.UserName ?? user.Id);

        var issuer = Uri.EscapeDataString("ko/ordinert");

        var authenticatorUri =
            $"otpauth://totp/{issuer}:{email}" +
            $"?secret={key}&issuer={issuer}&digits=6&period=30";

        using var generator = new QRCodeGenerator();

        using var qrCodeData = generator.CreateQrCode(
            authenticatorUri,
            QRCodeGenerator.ECCLevel.Q);

        using var qrCode = new PngByteQRCode(qrCodeData);

        var qrCodeBytes = qrCode.GetGraphic(8);

        return new EnableAuthenticatorViewModel
        {
            SharedKey = key,

            QrCodeImage = "data:image/png;base64," +
                Convert.ToBase64String(qrCodeBytes),

            SetupToken = setupProtector.Protect( CreateSetupBinding(user, key),TimeSpan.FromMinutes(10))
        };
    }
    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(
                string.Empty,
                error.Description);
        }
    }
    private async Task<bool> InvalidateSessionsAsync(ApplicationUser user)
    {
        var result = await userManager.UpdateSecurityStampAsync(user);

        await signInManager.ForgetTwoFactorClientAsync();
        await signInManager.SignOutAsync();

        return result.Succeeded;
    }
    [HttpGet]
    public async Task<IActionResult> StartAuthenticator()
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        if (await userManager.GetTwoFactorEnabledAsync(user))
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new ConfirmPasswordViewModel());
    }
    [HttpPost]
    public async Task<IActionResult> StartAuthenticator(ConfirmPasswordViewModel model)
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        if (await userManager.GetTwoFactorEnabledAsync(user))
        {
            return RedirectToAction(nameof(Index));
        }

        if (!await VerifyCredentialsAsync(user, model, false))
        {
            return View(model);
        }

        var result = await userManager.ResetAuthenticatorKeyAsync(user);

        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(model);
        }

        /// Oppdater gjeldende sesjon etter nøkkelendringen.
        await signInManager.RefreshSignInAsync(user);

        var authenticatorModel =
            await BuildAuthenticatorModelAsync(user);

        if (authenticatorModel is null)
        {
            return StatusCode(500);
        }

        return View("EnableAuthenticator", authenticatorModel);
    }

    [HttpPost]
    public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        if (await userManager.GetTwoFactorEnabledAsync(user))
        {
            return RedirectToAction(nameof(Index));
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            await signInManager.SignOutAsync();

            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            return View("StartAuthenticator",
                new ConfirmPasswordViewModel());
        }
         var key = await userManager.GetAuthenticatorKeyAsync(user);

            if (string.IsNullOrEmpty(key))
            {
                return RedirectToAction(nameof(StartAuthenticator));
            }

            try
            {
                var binding = setupProtector.Unprotect(
                    model.SetupToken);

                if (binding != CreateSetupBinding(user, key))
                {
                    return RedirectToAction(nameof(StartAuthenticator));
                }
            }
            catch (CryptographicException)
            {
                return RedirectToAction(nameof(StartAuthenticator));
            }

        var validCode = await userManager
            .VerifyTwoFactorTokenAsync(
                user,
                userManager.Options.Tokens
                    .AuthenticatorTokenProvider,
                model.Code);

        if (!validCode)
        {
            var failure = await userManager.AccessFailedAsync(user);

            if (!failure.Succeeded ||
                await userManager.IsLockedOutAsync(user))
            {
                await signInManager.SignOutAsync();

                return RedirectToAction("Login", "Account");
            }

            ModelState.AddModelError(
                string.Empty,
                "Ugyldig kode. Start oppsettet på nytt.");

            return View("StartAuthenticator",
                new ConfirmPasswordViewModel());
        }

        var codes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        if (codes is null)
        {
            return StatusCode(500);
        }

        var recoveryCodes = codes.ToArray();

        var resetResult = await userManager.ResetAccessFailedCountAsync(user);

        if (!resetResult.Succeeded)
        {
            return StatusCode(500);
        }

        var result = await userManager.SetTwoFactorEnabledAsync(user, true);

        if (!result.Succeeded)
        {
            AddIdentityErrors(result);

            return View("StartAuthenticator", new ConfirmPasswordViewModel());
        }

        if (!await InvalidateSessionsAsync(user))
        {
            /// 2FA kan allerede være aktivert.
            /// Vis kodene, men informer om at gamle sesjoner
            /// ikke nødvendigvis er ugyldiggjort.
            ViewData["SecurityWarning"] =
                "2FA er aktivert, men ikke alle gamle " +
                "sesjoner kunne ugyldiggjøres.";
        }

        return View("RecoveryCodes", new RecoveryCodesViewModel
        {
            RecoveryCodes = recoveryCodes
        });
    }

    [HttpGet]
    public async Task<IActionResult> Disable2fa()
    {
        if (MfaRequirements.IsRequired(User))
        {
            return Forbid();
        }

        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        if (!await userManager.GetTwoFactorEnabledAsync(user))
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new ConfirmPasswordViewModel());
    }
    [HttpPost]
    public async Task<IActionResult> Disable2fa(ConfirmPasswordViewModel model)
    {
        if (MfaRequirements.IsRequired(User))
        {
            return Forbid();
        }

        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        if (!await userManager.GetTwoFactorEnabledAsync(user))
        {
            return RedirectToAction(nameof(Index));
        }

        if (!await VerifyCredentialsAsync(user, model, true))
        {
            return View(model);
        }

        var result = await userManager.SetTwoFactorEnabledAsync(user, false);

        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(model);
        }

        var keyResult = await userManager.ResetAuthenticatorKeyAsync(user);

        if (!keyResult.Succeeded)
        {
            await InvalidateSessionsAsync(user);
            return StatusCode(500);
        }

        var recoveryResult = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 0);

        if (recoveryResult is null)
        {
            await InvalidateSessionsAsync(user);
            return StatusCode(500);
        }

        if (!await InvalidateSessionsAsync(user))
        {
            return StatusCode(500);
        }

        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public async Task<IActionResult> GenerateRecoveryCodes()
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        if (!await userManager.GetTwoFactorEnabledAsync(user))
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new ConfirmPasswordViewModel());
    }
    [HttpPost]
    public async Task<IActionResult> GenerateRecoveryCodes(ConfirmPasswordViewModel model)
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        if (!await userManager.GetTwoFactorEnabledAsync(user))
        {
            return RedirectToAction(nameof(Index));
        }

        if (!await VerifyCredentialsAsync(user, model, true))
        {
            return View(model);
        }

        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        if (recoveryCodes is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Kunne ikke generere nye koder.");

            return View(model);
        }

        if (!await InvalidateSessionsAsync(user))
        {
            ViewData["SecurityWarning"] =
                "Ikke alle gamle sesjoner kunne ugyldiggjøres.";
        }

        return View("RecoveryCodes", new RecoveryCodesViewModel
        {
            RecoveryCodes = recoveryCodes.ToArray()
        });
    }
}