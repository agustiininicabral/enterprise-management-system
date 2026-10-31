using System;
using EnterpriseManagement.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseManagement.Api.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261031000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Tasks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tasks", x => x.Id);
                table.ForeignKey(
                    name: "FK_Tasks_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_Users_Email", table: "Users", column: "Email", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Tasks_UserId_CreatedAt", table: "Tasks", columns: new[] { "UserId", "CreatedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Tasks");
        migrationBuilder.DropTable(name: "Users");
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
