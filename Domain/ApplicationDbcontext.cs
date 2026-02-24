using Domain.Entities.Base;
using Domain.Entities.Identity;
using Domain.Entities.Interceptors;
using Domain.Entities.Main;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Domain;

public class ApplicationDbcontext(DbContextOptions<ApplicationDbcontext> options, AuditInterceptor auditInterceptor) : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{

    // Identity
    public required DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public required DbSet<ApplicationRole> ApplicationRoles { get; set; }

    // Tasks
    public DbSet<AppTask> Tasks { get; set; }
    public DbSet<TaskAssignee> TaskAssignees { get; set; }
    public DbSet<TaskFile> TaskFiles { get; set; }
    public DbSet<TaskOccurrence> TaskOccurrences { get; set; }

    // Chat
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageFile> MessageFiles { get; set; }

    // Notifications
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // ✅ global soft-delete filter — auto-excludes IsDeleted = true everywhere
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(
                        System.Linq.Expressions.Expression.Lambda(
                            System.Linq.Expressions.Expression.Equal(
                                System.Linq.Expressions.Expression.Property(
                                    System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e"),
                                    nameof(AuditableEntity.IsDeleted)),
                                System.Linq.Expressions.Expression.Constant(false)),
                            System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e")));
            }
        }


        var cascadeFKs = modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

        foreach (var fk in cascadeFKs)
            fk.DeleteBehavior = DeleteBehavior.Restrict;

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .AddInterceptors(auditInterceptor)
            .ConfigureWarnings(warnings =>
             warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }
}
