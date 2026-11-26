namespace ArabClinic.Application.Accounts;

public interface IAccountService
{
    Task<AccountResult> RegisterAsync(RegisterAccountRequest request);

    Task<AccountResult> LoginAsync(LoginAccountRequest request);

    Task LogoutAsync();

    Task<AccountProfile?> GetProfileAsync(string userId);
}