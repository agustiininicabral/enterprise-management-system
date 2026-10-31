using System;
using EnterpriseManagement.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace EnterpriseManagement.Api.Data.Migrations;

[DbContext(typeof(AppDbContext))]
partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
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
            b.HasOne("EnterpriseManagement.Api.Domain.User", "User")
                .WithMany("Tasks").HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade).IsRequired();
            b.Navigation("User");
        });

        modelBuilder.Entity("EnterpriseManagement.Api.Domain.User", b =>
        {
            b.Navigation("Tasks");
        });
#pragma warning restore 612, 618
    }
}
