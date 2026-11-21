using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SolarSystem.Data
{
    public class AuthDB : IdentityDbContext<IdentityUser>
    {
        public AuthDB(DbContextOptions<AuthDB> options) : base(options)
        {

        }
    }
}
