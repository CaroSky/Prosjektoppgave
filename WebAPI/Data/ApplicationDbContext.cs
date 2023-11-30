using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models.Entities;
using SharedModels.Entities;

namespace WebAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public
            ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }
        public DbSet<Blog> Blog { get; set; }

        public DbSet<Post> Post { get; set; }

        public DbSet<Comment> Comment { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Her legger du til konfigurasjonen for relasjonen mellom Blog og IdentityUser
            modelBuilder.Entity<Blog>()
                .HasOne<IdentityUser>() // Du refererer til standard IdentityUser-klassen
                .WithMany() // En bruker kan ha mange blogger
                .HasForeignKey(b => b.UserId); // UserId er fremmednøkkelen i Blog-klassen
                                               //.IsRequired(false); // Gjør dette valgfritt hvis en blogg ikke nødvendigvis trenger en bruker


            //modelBuilder.Entity<Post>()
            //   .HasMany(p => p.Tags)
            //   .WithMany(t => t.Posts);

            // Seeding a user
            var userId = Guid.NewGuid().ToString(); // Create a GUID for the user Id
            var user = new IdentityUser
            {
                Id = userId,
                UserName = "testuser",
                NormalizedUserName = "TESTUSER",
                Email = "testuser@example.com",
                NormalizedEmail = "TESTUSER@EXAMPLE.COM",
                EmailConfirmed = true,
                PasswordHash = new PasswordHasher<IdentityUser>().HashPassword(null, "Passord123!"),
                SecurityStamp = string.Empty, // Normally this would be a random value
                // PasswordHash would be set to a hashed password
                // You can use the PasswordHasher<IdentityUser> to create this
            };

            modelBuilder.Entity<IdentityUser>().HasData(user);

            
             modelBuilder.Entity<Blog>().HasData(new Blog
            {
                BlogId = 1, // Gi et unikt ID til blogginnlegget
                Title = "Eksempel på blogginnlegg",
                Content = "Dette er innholdet i blogginnlegget.",
                Created = DateTime.UtcNow,
            UserId = "0cde7c88-d719-4905-89f4-9b94ce2390c2",
            IsPostAllowed = true
            });


        }
    }
}