using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var username = "sonujse@outlook.com";
            var password = "Password123!";

            var userStore = new CustomerUserStore(new CustomUserDbContext());

            var userManager = new UserManager<CustomerUser, int>(userStore);

            var creatinResult = userManager.Create(new CustomerUser { UserName = username }, password);
            Console.WriteLine("Creation {0}", creatinResult.Succeeded);

            var user = userManager.FindByName(username);

            var checkpassword = userManager.CheckPassword(user, password);
            Console.WriteLine("Passsword match {0}", checkpassword);
        }

        public class CustomerUser : IUser<int>
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string PasswordHash { get; set; }
        }

        public class CustomUserDbContext: DbContext
        {
            public CustomUserDbContext(): base("DefaultConnection") { }
            
            public DbSet<CustomerUser> Users { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                var user = modelBuilder.Entity<CustomerUser>();
                user.ToTable("Users");
                user.HasKey(x => x.Id);
                user.Property(x => x.Id).IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
                user.Property(x => x.UserName).IsRequired().HasMaxLength(256)
                    .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("UserNameIndex") { IsUnique = true}));
                base.OnModelCreating(modelBuilder);
            }
        }

        public class CustomerUserStore : IUserPasswordStore<CustomerUser, int>
        {
            private readonly CustomUserDbContext context;

            public CustomerUserStore(CustomUserDbContext context)
            {
                this.context = context;
            }

            public Task CreateAsync(CustomerUser user)
            {
                context.Users.Add(user);
                return context.SaveChangesAsync();
            }

            public Task DeleteAsync(CustomerUser user)
            {
                context.Users.Remove(user);
                return context.SaveChangesAsync();
            }

            public void Dispose()
            {
                context.Dispose();
            }

            public Task<CustomerUser> FindByIdAsync(int userId)
            {
                return context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            }

            public Task<CustomerUser> FindByNameAsync(string userName)
            {
                return context.Users.FirstOrDefaultAsync(x => x.UserName == userName);
            }

            public Task<string> GetPasswordHashAsync(CustomerUser user)
            {
                return Task.FromResult(user.PasswordHash);
            }

            public Task<bool> HasPasswordAsync(CustomerUser user)
            {
                return Task.FromResult(user.PasswordHash != null);
            }

            public Task SetPasswordHashAsync(CustomerUser user, string passwordHash)
            {
                user.PasswordHash = passwordHash;
                return Task.FromResult(0);             
            }

            public Task UpdateAsync(CustomerUser user)
            {
                context.Users.Attach(user);
                return context.SaveChangesAsync();
            }
        }
    }
}
