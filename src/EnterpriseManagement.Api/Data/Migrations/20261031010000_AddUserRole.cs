using EnterpriseManagement.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseManagement.Api.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261031010000_AddUserRole")]
public partial class AddUserRole : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Role",
            table: "Users",
            type: "nvarchar(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "User");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Role", table: "Users");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "8.0.33");
        modelBuilder.Entity("EnterpriseManagement.Api.Domain.User", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
            b.Property<string>("Email").IsRequired().HasMaxLength(256).HasColumnType("nvarchar(256)");
            b.Property<string>("PasswordHash").IsRequired().HasColumnType("nvarchar(max)");
            b.Property<string>("Role").IsRequired().ValueGeneratedOnAdd().HasMaxLength(32).HasDefaultValue("User").HasColumnType("nvarchar(32)");
            b.HasKey("Id");
            b.HasIndex("Email").IsUnique();
            b.ToTable("Users");
        });
        modelBuilder.Entity("EnterpriseManagement.Api.Domain.WorkTask", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
            b.Property<string>("Description").HasMaxLength(2000).HasColumnType("nvarchar(2000)");
            b.Property<string>("Status").IsRequired().HasMaxLength(20).HasColumnType("nvarchar(20)");
            b.Property<string>("Title").IsRequired().HasMaxLength(120).HasColumnType("nvarchar(120)");
            b.Property<DateTime>("UpdatedAt").HasColumnType("datetime2");
            b.Property<Guid>("UserId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("UserId", "CreatedAt");
            b.ToTable("Tasks");
        });
        modelBuilder.Entity("EnterpriseManagement.Api.Domain.WorkTask", b =>
        {
            b.HasOne("EnterpriseManagement.Api.Domain.User", "User").WithMany("Tasks")
                .HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade).IsRequired();
            b.Navigation("User");
        });
        modelBuilder.Entity("EnterpriseManagement.Api.Domain.User", b => b.Navigation("Tasks"));
#pragma warning restore 612, 618
    }
}
