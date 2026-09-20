using System.ComponentModel.DataAnnotations;

namespace bg_backend.DTOs;

public record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(6)] string Password
);

public record LoginResponse(
    string Token,
    string Email,
    string Role,
    DateTime ExpiresAt
);
