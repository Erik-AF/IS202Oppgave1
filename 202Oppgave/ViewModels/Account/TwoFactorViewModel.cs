namespace _202Oppgave.ViewModels.Account;

public class TwoFactorViewModel
{
    public bool Is2faEnabled { get; set; }

    public bool HasAuthenticator { get; set; }

    public int RecoveryCodesLeft { get; set; }

    public bool Is2faRequired { get; set; }
}