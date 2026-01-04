namespace BACApp.Core.DTO;

public sealed class LoginResponse
{
    public string? First_Name { get; set; } // if API uses snake_case
    public string? Last_Name { get; set; }
    public int? User_Id { get; set; }
    public string? Email { get; set; }

    // The token field per API docs
    public string? Token { get; set; }

    public string? Avatar { get; set; }

    public CompanyDto[]? Companies { get; set; }
}
