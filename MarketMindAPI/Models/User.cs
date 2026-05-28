using Microsoft.AspNetCore.Identity;

namespace MarketMindAPI.Models;

public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
