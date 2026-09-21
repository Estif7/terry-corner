namespace TerryCorner.Api.Controllers;

public record RegisterRequest(string FullName, string Email, string PhoneNumber, string Password);
public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);
public record LogoutRequest(string RefreshToken);
