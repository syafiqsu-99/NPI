using Microsoft.EntityFrameworkCore;
using NPI.Server.Models;

namespace NPI.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Approvals> Approvals { get; set; }
        public DbSet<AuditLogs> AuditLogs { get; set; }
        public DbSet<Comments> Comments { get; set; }
        public DbSet<Customers> Customers { get; set; }
        public DbSet<Departments> Departments { get; set; }
        public DbSet<DocumentTypes> DocumentTypes { get; set; }
        public DbSet<Enquiries> Enquiries { get; set; }
        public DbSet<EnquiryGeneralInfo> EnquiryGeneralInfo { get; set; }
        public DbSet<EnquirySealInfo> EnquirySealInfo { get; set; }
        public DbSet<EnquiryCustomerRef> EnquiryCustomerRef { get; set; }
        public DbSet<Files> Files { get; set; }
        public DbSet<Milestones> Milestones { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<Projects> Projects { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<ProjectTeams> ProjectTeams { get; set; }
        public DbSet<ProjectRevisions> ProjectRevisions { get; set; }
        public DbSet<TaskRevisions> TaskRevisions { get; set; }
        public DbSet<ProjectStatusHistory> ProjectStatusHistory { get; set; }
        public DbSet<RolePermissions> RolePermissions { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<UserSessions> UserSessions { get; set; }
        public DbSet<StageCompletionLog> StageCompletionLogs { get; set; }
        public DbSet<TaskDocumentRequirements> TaskDocumentRequirements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Projects>()
                .HasIndex(p => p.proj_no)
                .IsUnique();

            modelBuilder.Entity<Users>()
                .HasIndex(u => u.username)
                .IsUnique();

            modelBuilder.Entity<Users>()
                .HasIndex(u => u.email)
                .IsUnique();

            modelBuilder.Entity<Enquiries>()
                .HasIndex(e => e.enquiry_no)
                .IsUnique();

            modelBuilder.Entity<EnquiryGeneralInfo>()
                .HasOne(g => g.Enquiry)
                .WithOne(e => e.GeneralInfo)
                .HasForeignKey<EnquiryGeneralInfo>(g => g.enquiry_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EnquirySealInfo>()
                .HasOne(s => s.Enquiry)
                .WithOne(e => e.SealInfo)
                .HasForeignKey<EnquirySealInfo>(s => s.enquiry_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EnquiryCustomerRef>()
                .HasOne(c => c.Enquiry)
                .WithOne(e => e.CustomerRef)
                .HasForeignKey<EnquiryCustomerRef>(c => c.enquiry_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Projects>()
                .HasOne(p => p.CreatedByUser)
                .WithMany()
                .HasForeignKey(p => p.created_by)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Projects>()
                .HasOne(p => p.UpdatedByUser)
                .WithMany()
                .HasForeignKey(p => p.updated_by)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.AssignedByUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.assigned_by)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.proj_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectTeams>()
                .HasOne(pt => pt.Project)
                .WithMany(p => p.ProjectTeams)
                .HasForeignKey(pt => pt.proj_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.ParentTask)
                .WithMany(t => t.SubTasks)
                .HasForeignKey(t => t.parent_task_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Milestones>()
                .HasOne(m => m.Tasks)
                .WithOne(t => t.Milestone)
                .HasForeignKey<Milestones>(m => m.task_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Files>()
                .HasOne(f => f.Task)
                .WithMany(t => t.Files)
                .HasForeignKey(f => f.task_id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Files>()
                .HasOne(f => f.ReplacedByFile)
                .WithMany()
                .HasForeignKey(f => f.replaced_by)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectTeams>()
                .HasOne(pt => pt.AssignedByUser)
                .WithMany()
                .HasForeignKey(pt => pt.assigned_by)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskRevisions>()
                .HasOne(tr => tr.Revision)
                .WithMany(pr => pr.TaskRevisions)
                .HasForeignKey(tr => tr.revision_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notifications>()
                .HasIndex(n => new { n.user_id, n.is_read, n.created_at })
                .HasDatabaseName("IX_Notifications_User_Unread");

            modelBuilder.Entity<Notifications>()
                .HasOne(n => n.Task)
                .WithMany()
                .HasForeignKey(n => n.task_id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notifications>()
                .HasOne(n => n.Project)
                .WithMany()
                .HasForeignKey(n => n.proj_id)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<StageCompletionLog>()
                .HasOne(s => s.Project)
                .WithMany()
                .HasForeignKey(s => s.proj_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StageCompletionLog>()
                .HasOne(s => s.CompletedByUser)
                .WithMany()
                .HasForeignKey(s => s.completed_by)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<StageCompletionLog>()
                .HasIndex(s => new { s.proj_id, s.stage_id })
                .HasDatabaseName("IX_StageCompletionLog_Project_Stage");

            modelBuilder.Entity<TaskDocumentRequirements>()
                .HasOne(r => r.Task)
                .WithMany()
                .HasForeignKey(r => r.task_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskDocumentRequirements>()
                .HasOne(r => r.DocumentType)
                .WithMany()
                .HasForeignKey(r => r.doc_type_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskDocumentRequirements>()
                .HasOne(r => r.File)
                .WithMany()
                .HasForeignKey(r => r.file_id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TaskDocumentRequirements>()
                .HasIndex(r => r.task_id)
                .HasDatabaseName("IX_TaskDocumentRequirements_Task");

            modelBuilder.Entity<TaskDocumentRequirements>()
                .HasIndex(r => new { r.task_id, r.doc_type_id })
                .IsUnique()
                .HasDatabaseName("IX_TaskDocumentRequirements_Unique");
        }
    }
}
