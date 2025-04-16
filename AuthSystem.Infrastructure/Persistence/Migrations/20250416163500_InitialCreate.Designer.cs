// <auto-generated />
using System;
using AuthSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AuthSystem.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250416163500_InitialCreate")]
    partial class InitialCreate
    {
        /// <summary>
        /// Método para construir el modelo
        /// </summary>
        /// <param name="modelBuilder">Constructor del modelo</param>
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AuthSystem.Domain.Entities.AuditLog", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("Action")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("EntityId")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("EntityName")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("Endpoint")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("IpAddress")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("NewValues")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("OldValues")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("QueryString")
                    .HasMaxLength(500)
                    .HasColumnType("nvarchar(500)");

                b.Property<string>("Severity")
                    .IsRequired()
                    .ValueGeneratedOnAdd()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)")
                    .HasDefaultValue("Information");

                b.Property<DateTime>("Timestamp")
                    .HasColumnType("datetime2");

                b.Property<string>("UserAgent")
                    .HasMaxLength(500)
                    .HasColumnType("nvarchar(500)");

                b.Property<Guid?>("UserId")
                    .HasColumnType("uniqueidentifier");

                b.HasKey("Id");

                b.HasIndex("UserId");

                b.ToTable("AuditLogs");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.Branch", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("Address")
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("CreatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("Description")
                    .HasMaxLength(500)
                    .HasColumnType("nvarchar(500)");

                b.Property<string>("Email")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<bool>("IsActive")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bit")
                    .HasDefaultValue(true);

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<Guid>("OrganizationId")
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("Phone")
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.Property<DateTime?>("UpdatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("UpdatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.HasKey("Id");

                b.HasIndex("OrganizationId");

                b.HasIndex("Name", "OrganizationId")
                    .IsUnique();

                b.ToTable("Branches");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.Module", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("CreatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("Description")
                    .HasMaxLength(500)
                    .HasColumnType("nvarchar(500)");

                b.Property<int>("DisplayOrder")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasDefaultValue(0);

                b.Property<string>("Icon")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<bool>("IsEnabled")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bit")
                    .HasDefaultValue(true);

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<Guid?>("ParentId")
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("Route")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<DateTime?>("UpdatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("UpdatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.HasKey("Id");

                b.HasIndex("Name")
                    .IsUnique();

                b.HasIndex("ParentId");

                b.ToTable("Modules");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.Organization", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("Address")
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("CreatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("Description")
                    .HasMaxLength(500)
                    .HasColumnType("nvarchar(500)");

                b.Property<string>("Email")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<bool>("IsActive")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bit")
                    .HasDefaultValue(true);

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("Phone")
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.Property<string>("TaxId")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.Property<DateTime?>("UpdatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("UpdatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("Website")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.HasKey("Id");

                b.HasIndex("Name")
                    .IsUnique();

                b.HasIndex("TaxId")
                    .IsUnique();

                b.ToTable("Organizations");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.Permission", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("CreatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("Description")
                    .HasMaxLength(500)
                    .HasColumnType("nvarchar(500)");

                b.Property<bool>("IsActive")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bit")
                    .HasDefaultValue(true);

                b.Property<Guid>("ModuleId")
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("Type")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.Property<DateTime?>("UpdatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("UpdatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.HasKey("Id");

                b.HasIndex("ModuleId");

                b.HasIndex("Name", "ModuleId")
                    .IsUnique();

                b.ToTable("Permissions");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.Role", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("CreatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("Description")
                    .HasMaxLength(500)
                    .HasColumnType("nvarchar(500)");

                b.Property<bool>("IsActive")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bit")
                    .HasDefaultValue(true);

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<Guid?>("OrganizationId")
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTime?>("UpdatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("UpdatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.HasKey("Id");

                b.HasIndex("OrganizationId");

                b.HasIndex("Name", "OrganizationId")
                    .IsUnique()
                    .HasFilter("[OrganizationId] IS NOT NULL");

                b.ToTable("Roles");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.RolePermission", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("CreatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<bool>("IsActive")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bit")
                    .HasDefaultValue(true);

                b.Property<Guid>("PermissionId")
                    .HasColumnType("uniqueidentifier");

                b.Property<Guid>("RoleId")
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTime?>("UpdatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("UpdatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.HasKey("Id");

                b.HasIndex("PermissionId");

                b.HasIndex("RoleId", "PermissionId")
                    .IsUnique();

                b.ToTable("RolePermissions");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.User", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("CreatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("Email")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<int>("FailedLoginAttempts")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasDefaultValue(0);

                b.Property<string>("FullName")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<bool>("IsActive")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bit")
                    .HasDefaultValue(true);

                b.Property<bool>("IsEmailVerified")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bit")
                    .HasDefaultValue(false);

                b.Property<string>("LdapDN")
                    .HasMaxLength(255)
                    .HasColumnType("nvarchar(255)");

                b.Property<DateTime?>("LastLoginAt")
                    .HasColumnType("datetime2");

                b.Property<DateTime?>("LockoutEnd")
                    .HasColumnType("datetime2");

                b.Property<string>("PasswordHash")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<DateTime?>("UpdatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("UpdatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("UserType")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.Property<string>("Username")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.HasKey("Id");

                b.HasIndex("Email")
                    .IsUnique();

                b.HasIndex("LdapDN")
                    .IsUnique()
                    .HasFilter("[LdapDN] IS NOT NULL");

                b.HasIndex("Username")
                    .IsUnique();

                b.ToTable("Users");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.UserBranch", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<Guid>("BranchId")
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("CreatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<bool>("IsActive")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bit")
                    .HasDefaultValue(true);

                b.Property<bool>("IsDefault")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bit")
                    .HasDefaultValue(false);

                b.Property<DateTime?>("UpdatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("UpdatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<Guid>("UserId")
                    .HasColumnType("uniqueidentifier");

                b.HasKey("Id");

                b.HasIndex("BranchId");

                b.HasIndex("UserId", "BranchId")
                    .IsUnique();

                b.ToTable("UserBranches");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.UserRole", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("CreatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<bool>("IsActive")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bit")
                    .HasDefaultValue(true);

                b.Property<Guid>("RoleId")
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTime?>("UpdatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("UpdatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<Guid>("UserId")
                    .HasColumnType("uniqueidentifier");

                b.HasKey("Id");

                b.HasIndex("RoleId");

                b.HasIndex("UserId", "RoleId")
                    .IsUnique();

                b.ToTable("UserRoles");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.UserSession", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("IpAddress")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<bool>("IsRevoked")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bit")
                    .HasDefaultValue(false);

                b.Property<string>("RefreshToken")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<DateTime>("RefreshTokenExpiryTime")
                    .HasColumnType("datetime2");

                b.Property<DateTime?>("UpdatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("UserAgent")
                    .HasMaxLength(500)
                    .HasColumnType("nvarchar(500)");

                b.Property<Guid>("UserId")
                    .HasColumnType("uniqueidentifier");

                b.HasKey("Id");

                b.HasIndex("RefreshToken")
                    .IsUnique();

                b.HasIndex("UserId");

                b.ToTable("UserSessions");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.AuditLog", b =>
            {
                b.HasOne("AuthSystem.Domain.Entities.User", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.SetNull);

                b.Navigation("User");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.Branch", b =>
            {
                b.HasOne("AuthSystem.Domain.Entities.Organization", "Organization")
                    .WithMany("Branches")
                    .HasForeignKey("OrganizationId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Organization");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.Module", b =>
            {
                b.HasOne("AuthSystem.Domain.Entities.Module", "Parent")
                    .WithMany("Children")
                    .HasForeignKey("ParentId")
                    .OnDelete(DeleteBehavior.Restrict);

                b.Navigation("Parent");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.Permission", b =>
            {
                b.HasOne("AuthSystem.Domain.Entities.Module", "Module")
                    .WithMany("Permissions")
                    .HasForeignKey("ModuleId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Module");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.Role", b =>
            {
                b.HasOne("AuthSystem.Domain.Entities.Organization", "Organization")
                    .WithMany("Roles")
                    .HasForeignKey("OrganizationId")
                    .OnDelete(DeleteBehavior.SetNull);

                b.Navigation("Organization");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.RolePermission", b =>
            {
                b.HasOne("AuthSystem.Domain.Entities.Permission", "Permission")
                    .WithMany("RolePermissions")
                    .HasForeignKey("PermissionId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("AuthSystem.Domain.Entities.Role", "Role")
                    .WithMany("RolePermissions")
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Permission");

                b.Navigation("Role");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.UserBranch", b =>
            {
                b.HasOne("AuthSystem.Domain.Entities.Branch", "Branch")
                    .WithMany("UserBranches")
                    .HasForeignKey("BranchId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("AuthSystem.Domain.Entities.User", "User")
                    .WithMany("UserBranches")
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Branch");

                b.Navigation("User");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.UserRole", b =>
            {
                b.HasOne("AuthSystem.Domain.Entities.Role", "Role")
                    .WithMany("UserRoles")
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("AuthSystem.Domain.Entities.User", "User")
                    .WithMany("UserRoles")
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Role");

                b.Navigation("User");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.UserSession", b =>
            {
                b.HasOne("AuthSystem.Domain.Entities.User", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("User");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.Branch", b =>
            {
                b.Navigation("UserBranches");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.Module", b =>
            {
                b.Navigation("Children");

                b.Navigation("Permissions");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.Organization", b =>
            {
                b.Navigation("Branches");

                b.Navigation("Roles");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.Permission", b =>
            {
                b.Navigation("RolePermissions");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.Role", b =>
            {
                b.Navigation("RolePermissions");

                b.Navigation("UserRoles");
            });

            modelBuilder.Entity("AuthSystem.Domain.Entities.User", b =>
            {
                b.Navigation("UserBranches");

                b.Navigation("UserRoles");
            });
#pragma warning restore 612, 618
        }
    }
}
