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
                .HasForeignKey(b => b.UserId) // UserId er fremmednøkkelen i Blog-klassen
                .IsRequired(false); // Gjør dette valgfritt hvis en blogg ikke nødvendigvis trenger en bruker


            //modelBuilder.Entity<Post>()
            //   .HasMany(p => p.Tags)
            //   .WithMany(t => t.Posts);

        }
    }
}