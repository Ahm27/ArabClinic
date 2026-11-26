namespace ArabClinic.Application.Accounts;

public sealed record LoginAccountRequest(
    string Email,
    string Password,
    bool RememberMe);