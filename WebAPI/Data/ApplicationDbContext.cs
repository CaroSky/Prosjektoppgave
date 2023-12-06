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

        public DbSet<Tag> Tag { get; set; }

        public DbSet<PostTag> PostTag { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PostTag>()
                .HasKey(pt => new { pt.PostsPostId, pt.TagsTagId });

            // Her legger du til konfigurasjonen for relasjonen mellom Blog og IdentityUser
            modelBuilder.Entity<Blog>()
                .HasOne<IdentityUser>() // Du refererer til standard IdentityUser-klassen
                .WithMany() // En bruker kan ha mange blogger
                .HasForeignKey(b => b.OwnerId); // UserId er fremmednøkkelen i Blog-klassen
                                               //.IsRequired(false); // Gjør dette valgfritt hvis en blogg ikke nødvendigvis trenger en bruker


           // //modelBuilder.Entity<Post>()
           // //   .HasMany(p => p.Tags)
           // //   .WithMany(t => t.Posts);

            // Seeding a user
           /*// var userId = Guid.NewGuid().ToString(); // Create a GUID for the user Id
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

            modelBuilder.Entity<IdentityUser>().HasData(user);*/


            modelBuilder.Entity<Post>().Property<int>("BlogId");

            modelBuilder.Entity<Post>().HasData(new
            {
                PostId = 1,
                Title = "Seedet Post Tittel",
                Content = "Dette er innholdet i den seedede posten.",
                Created = DateTime.UtcNow,
                IsCommentAllowed = true,
                // Riktig måte å sette skyggeegenskapen på
                BlogId = 1006,  // Eksisterende BlogId
                //AuthorId = "c12eacb0 - c1a9 - 48c3 - b4a7 - c9e7a7ce3436"
            });


        }
    }
}