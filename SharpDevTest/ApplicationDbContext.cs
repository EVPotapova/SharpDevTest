using Microsoft.AspNet.Identity.EntityFramework;
using SharpDevTest.Models;
using SharpDevTest.Models.DbModel;
using System.Data.Entity;

namespace SharpDevTest
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("Server=.\\SQLEXPRESS;Database=SharpDevTest;Trusted_Connection=True;", throwIfV1Schema: false)//TODO: to config
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<ApplicationDbContext>());

        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }


        public DbSet<TransactionDbModel> Transactions { get; set; }
    }
}