﻿// <auto-generated />
using System;
using AdventureGameEditor.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AdventureGameEditor.Migrations
{
    [DbContext(typeof(AdventureGameEditorContext))]
    partial class AdventureGameEditorContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("AdventureGameEditor.Models.Alternative", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("TrialID")
                        .HasColumnType("int");

                    b.Property<int?>("TrialResultID")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("TrialID");

                    b.HasIndex("TrialResultID");

                    b.ToTable("Alternative");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.Field", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ColNumber")
                        .HasColumnType("int");

                    b.Property<string>("GameTitle")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDownWay")
                        .HasColumnType("bit");

                    b.Property<bool>("IsLeftWay")
                        .HasColumnType("bit");

                    b.Property<bool>("IsRightWay")
                        .HasColumnType("bit");

                    b.Property<bool>("IsUpWay")
                        .HasColumnType("bit");

                    b.Property<int?>("MapRowID")
                        .HasColumnType("int");

                    b.Property<int?>("OwnerId")
                        .HasColumnType("int");

                    b.Property<int>("RowNumber")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("TrialID")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("MapRowID");

                    b.HasIndex("OwnerId");

                    b.HasIndex("TrialID");

                    b.ToTable("Field");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.Game", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CurrentWayDirectionsCode")
                        .HasColumnType("int");

                    b.Property<int?>("GameLostResultID")
                        .HasColumnType("int");

                    b.Property<int?>("GameWonResultID")
                        .HasColumnType("int");

                    b.Property<int?>("OwnerId")
                        .HasColumnType("int");

                    b.Property<int>("PlayCounter")
                        .HasColumnType("int");

                    b.Property<int?>("PreludeID")
                        .HasColumnType("int");

                    b.Property<int?>("StartFieldID")
                        .HasColumnType("int");

                    b.Property<int>("TableSize")
                        .HasColumnType("int");

                    b.Property<int?>("TargetFieldID")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Visibility")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("GameLostResultID");

                    b.HasIndex("GameWonResultID");

                    b.HasIndex("OwnerId");

                    b.HasIndex("PreludeID");

                    b.HasIndex("StartFieldID");

                    b.HasIndex("TargetFieldID");

                    b.ToTable("Game");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.GameResult", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("GameTitle")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsGameWonResult")
                        .HasColumnType("bit");

                    b.Property<int?>("OwnerId")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.HasIndex("OwnerId");

                    b.ToTable("GameResult");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.GameplayData", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("CurrentPlayerPositionID")
                        .HasColumnType("int");

                    b.Property<int>("GameCondition")
                        .HasColumnType("int");

                    b.Property<string>("GameTitle")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("LastPlayDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("PlayerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("StepCount")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("CurrentPlayerPositionID");

                    b.ToTable("GameplayData");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.Image", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Picture")
                        .HasColumnType("varbinary(max)");

                    b.HasKey("ID");

                    b.ToTable("Image");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.IsVisitedField", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ColNumber")
                        .HasColumnType("int");

                    b.Property<int?>("GameplayDataID")
                        .HasColumnType("int");

                    b.Property<bool>("IsVisited")
                        .HasColumnType("bit");

                    b.Property<int>("RowNumber")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("GameplayDataID");

                    b.ToTable("IsVisitedField");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.MapImage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<byte[]>("Image")
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("Theme")
                        .HasColumnType("int");

                    b.Property<int>("WayDirectionsCode")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("MapImage");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.MapRow", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("GameID")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("GameID");

                    b.ToTable("MapRow");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.Message", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("UserId");

                    b.ToTable("Message");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.Prelude", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("GameTitle")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ImageID")
                        .HasColumnType("int");

                    b.Property<int?>("OwnerId")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.HasIndex("ImageID");

                    b.HasIndex("OwnerId");

                    b.ToTable("Prelude");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.Trial", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TrialType")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.ToTable("Trial");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.TrialResult", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ResultType")
                        .HasColumnType("int");

                    b.Property<int?>("TeleportTargetID")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.HasIndex("TeleportTargetID");

                    b.ToTable("TrialResult");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NickName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NormalizedEmail")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("User");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<int>", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.Alternative", b =>
                {
                    b.HasOne("AdventureGameEditor.Models.Trial", null)
                        .WithMany("Alternatives")
                        .HasForeignKey("TrialID");

                    b.HasOne("AdventureGameEditor.Models.TrialResult", "TrialResult")
                        .WithMany()
                        .HasForeignKey("TrialResultID");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.Field", b =>
                {
                    b.HasOne("AdventureGameEditor.Models.MapRow", null)
                        .WithMany("Row")
                        .HasForeignKey("MapRowID");

                    b.HasOne("AdventureGameEditor.Models.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");

                    b.HasOne("AdventureGameEditor.Models.Trial", "Trial")
                        .WithMany()
                        .HasForeignKey("TrialID");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.Game", b =>
                {
                    b.HasOne("AdventureGameEditor.Models.GameResult", "GameLostResult")
                        .WithMany()
                        .HasForeignKey("GameLostResultID");

                    b.HasOne("AdventureGameEditor.Models.GameResult", "GameWonResult")
                        .WithMany()
                        .HasForeignKey("GameWonResultID");

                    b.HasOne("AdventureGameEditor.Models.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");

                    b.HasOne("AdventureGameEditor.Models.Prelude", "Prelude")
                        .WithMany()
                        .HasForeignKey("PreludeID");

                    b.HasOne("AdventureGameEditor.Models.Field", "StartField")
                        .WithMany()
                        .HasForeignKey("StartFieldID");

                    b.HasOne("AdventureGameEditor.Models.Field", "TargetField")
                        .WithMany()
                        .HasForeignKey("TargetFieldID");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.GameResult", b =>
                {
                    b.HasOne("AdventureGameEditor.Models.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.GameplayData", b =>
                {
                    b.HasOne("AdventureGameEditor.Models.Field", "CurrentPlayerPosition")
                        .WithMany()
                        .HasForeignKey("CurrentPlayerPositionID");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.IsVisitedField", b =>
                {
                    b.HasOne("AdventureGameEditor.Models.GameplayData", null)
                        .WithMany("VisitedFields")
                        .HasForeignKey("GameplayDataID");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.MapRow", b =>
                {
                    b.HasOne("AdventureGameEditor.Models.Game", null)
                        .WithMany("Map")
                        .HasForeignKey("GameID");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.Message", b =>
                {
                    b.HasOne("AdventureGameEditor.Models.User", null)
                        .WithMany("Messages")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.Prelude", b =>
                {
                    b.HasOne("AdventureGameEditor.Models.Image", "Image")
                        .WithMany()
                        .HasForeignKey("ImageID");

                    b.HasOne("AdventureGameEditor.Models.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");
                });

            modelBuilder.Entity("AdventureGameEditor.Models.TrialResult", b =>
                {
                    b.HasOne("AdventureGameEditor.Models.Field", "TeleportTarget")
                        .WithMany()
                        .HasForeignKey("TeleportTargetID");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole<int>", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.HasOne("AdventureGameEditor.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.HasOne("AdventureGameEditor.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<int>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole<int>", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AdventureGameEditor.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.HasOne("AdventureGameEditor.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
