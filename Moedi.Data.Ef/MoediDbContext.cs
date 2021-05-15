using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Moedi.Data.Ef
{
    public abstract class MoediDbContext : DbContext
    {
        protected abstract string ConnectionName { get; }

        protected abstract string Schema { get; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ValidateEntities();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ValidateEntities()
        {
            var entities = ChangeTracker.Entries()
                .Where(e => new[] { EntityState.Added, EntityState.Modified }.Contains(e.State))
                .Select(e => e.Entity);

            foreach (var entity in entities)
            {
                var validationContext = new ValidationContext(entity);
                Validator.ValidateObject(entity, validationContext);
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(x =>
            {
                x.MigrationsHistoryTable("__EFMigrationsHistory", Schema);
            });

            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            if (!string.IsNullOrEmpty(envName))
            {
                configuration
                    .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true);
            }

            configuration.AddEnvironmentVariables();

            var connectionString = configuration.Build().GetConnectionString(ConnectionName);

            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}