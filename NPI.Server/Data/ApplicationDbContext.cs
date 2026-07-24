using Microsoft.EntityFrameworkCore;
using NPI.Server.Models;

namespace NPI.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // ── Core entities ─────────────────────────────────────────────────────
        public DbSet<AuditLogs> AuditLogs { get; set; }
        public DbSet<Customers> Customers { get; set; }
        public DbSet<Departments> Departments { get; set; }

        // ── Form Config ───────────────────────────────────────────────────
        public DbSet<FormCategory> FormCategories { get; set; }
        public DbSet<FormSection> FormSections { get; set; }
        public DbSet<FormField> FormFields { get; set; }
        public DbSet<EnquiryFieldValues> EnquiryFieldValues { get; set; }

        // ── Enquiries ─────────────────────────────────────────────────────────
        public DbSet<Enquiries> Enquiries { get; set; }
        public DbSet<EnquiryCustomerRef> EnquiryCustomerRef { get; set; }
        public DbSet<EnquiryReviews> EnquiryReviews { get; set; }
        public DbSet<EnquiryRevisionSnapshots> EnquiryRevisionSnapshots { get; set; }

        // ── Files & Projects ──────────────────────────────────────────────────
        public DbSet<Files> Files { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<Projects> Projects { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<TaskTemplates> TaskTemplates { get; set; }
        public DbSet<TaskComments> TaskComments { get; set; }

        // ── Teams & Roles ─────────────────────────────────────────────────────
        public DbSet<ProjectTeams> ProjectTeams { get; set; }
        public DbSet<ProjectRoles> ProjectRoles { get; set; }

        // ── Revisions ─────────────────────────────────────────────────────────
        public DbSet<ProjectRevisions> ProjectRevisions { get; set; }
        public DbSet<TaskRevisions> TaskRevisions { get; set; }

        // ── Users & Auth ──────────────────────────────────────────────────────
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<UserSessions> UserSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Unique indexes ────────────────────────────────────────────────
            modelBuilder.Entity<Projects>()
                .HasIndex(p => p.proj_no).IsUnique();

            modelBuilder.Entity<Users>()
                .HasIndex(u => u.username).IsUnique();

            modelBuilder.Entity<Users>()
                .HasIndex(u => u.email).IsUnique();

            modelBuilder.Entity<Enquiries>()
                .HasIndex(e => e.enquiry_no).IsUnique();

            // ── EnquiryFieldValues ─────────────────────────────────────────────
            modelBuilder.Entity<EnquiryFieldValues>()
                .HasOne(v => v.Enquiry)
                .WithMany(e => e.FieldValues)
                .HasForeignKey(v => v.enquiry_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EnquiryFieldValues>()
                .HasIndex(v => new { v.enquiry_id, v.section_key, v.field_key })
                .IsUnique()
                .HasDatabaseName("IX_EnquiryFieldValues_Unique");

            modelBuilder.Entity<EnquiryCustomerRef>()
                .HasOne(c => c.Enquiry)
                .WithOne(e => e.CustomerRef)
                .HasForeignKey<EnquiryCustomerRef>(c => c.enquiry_id)
                .OnDelete(DeleteBehavior.Cascade);

            // EnquiryReviews
            modelBuilder.Entity<EnquiryReviews>()
                .HasOne(r => r.Enquiry)
                .WithMany(e => e.Reviews)
                .HasForeignKey(r => r.enquiry_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EnquiryReviews>()
                .HasOne(r => r.ReviewedByUser)
                .WithMany()
                .HasForeignKey(r => r.reviewed_by)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EnquiryReviews>()
                .HasIndex(r => new { r.enquiry_id, r.created_at });

            // EnquiryRevisionSnapshots
            modelBuilder.Entity<EnquiryRevisionSnapshots>()
                .HasOne(s => s.Enquiry)
                .WithMany(e => e.RevisionSnapshots)
                .HasForeignKey(s => s.enquiry_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EnquiryRevisionSnapshots>()
                .HasOne(s => s.SubmittedByUser)
                .WithMany()
                .HasForeignKey(s => s.submitted_by)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EnquiryRevisionSnapshots>()
                .HasIndex(s => new { s.enquiry_id, s.revision_no })
                .IsUnique()
                .HasDatabaseName("IX_EnquiryRevSnap_Enquiry_Rev");

            // ── Projects ──────────────────────────────────────────────────────
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

            // ── Tasks ─────────────────────────────────────────────────────────
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

            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.ParentTask)
                .WithMany(t => t.SubTasks)
                .HasForeignKey(t => t.parent_task_id)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskComments
            modelBuilder.Entity<TaskComments>()
                .HasOne(c => c.Task)
                .WithMany()
                .HasForeignKey(c => c.task_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskComments>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.user_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskComments>()
                .HasIndex(c => new { c.task_id, c.created_at })
                .HasDatabaseName("IX_TaskComments_Task_Created");

            // ── TaskTemplates ─────────────────────────────────────────────────
            modelBuilder.Entity<TaskTemplates>()
                .HasOne(t => t.Department)
                .WithMany()
                .HasForeignKey(t => t.dept_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskTemplates>()
                .HasIndex(t => new { t.stage_id, t.task_code })
                .IsUnique()
                .HasDatabaseName("IX_TaskTemplates_Stage_Code");

            // ── ProjectTeams ──────────────────────────────────────────────────
            modelBuilder.Entity<ProjectTeams>()
                .HasIndex(pr => new { pr.proj_id, pr.user_id })
                .IsUnique()
                .HasDatabaseName("IX_ProjectTeams_ProjUser");

            modelBuilder.Entity<ProjectTeams>()
                .HasOne(pt => pt.Project)
                .WithMany(p => p.ProjectTeams)
                .HasForeignKey(pt => pt.proj_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectTeams>()
                .HasOne(pt => pt.AssignedByUser)
                .WithMany()
                .HasForeignKey(pt => pt.assigned_by)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Files ─────────────────────────────────────────────────────────
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

            modelBuilder.Entity<Files>()
                .HasOne(f => f.Project)
                .WithMany(p => p.Files)
                .HasForeignKey(f => f.proj_id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TaskRevisions>()
                .HasOne(tr => tr.Revision)
                .WithMany(pr => pr.TaskRevisions)
                .HasForeignKey(tr => tr.revision_id)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            // ── Notifications ─────────────────────────────────────────────────
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
                .WithMany(p => p.Notifications)
                .HasForeignKey(n => n.proj_id)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}