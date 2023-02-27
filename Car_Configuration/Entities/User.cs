using Microsoft.AspNetCore.Identity;

namespace Car_Configuration.Entities;
public class User : IdentityUser<int>
{
    public virtual List<UserWheelColor>? UserWheelColors { get; set; }
}
