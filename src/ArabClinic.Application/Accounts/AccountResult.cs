namespace ArabClinic.Application.Accounts;

public sealed record AccountResult(
    bool Succeeded,
    IReadOnlyList<string> Errors,
    bool IsLockedOut = false);