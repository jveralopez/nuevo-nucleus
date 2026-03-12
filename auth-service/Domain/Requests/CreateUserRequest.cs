namespace AuthService.Domain.Requests;

public record CreateUserRequest(string Username, string Password, string? Role, string? Estado);
