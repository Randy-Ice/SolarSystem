using Microsoft.EntityFrameworkCore;

namespace SolarSystem.Data
{
    public class ProjectDB : DbContext
    {
        public ProjectDB(DbContextOptions<ProjectDB> options) : base(options)
        {

        }
    }
}
