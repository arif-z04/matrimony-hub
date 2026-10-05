using Microsoft.AspNetCore.Identity;

namespace MatrimonyHub.Domain.Entities;

public class ApplicationRole : IdentityRole<int>
{
    public ApplicationRole() : base() { }
    public ApplicationRole(string roleName) : base(roleName) { }
    public string? Description { get; set; }
}
