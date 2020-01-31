﻿// <auto-generated />
using Library.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;
using Library.Contexts;

namespace Library.Migrations
{
    [DbContext(typeof(LibraryContext))]
    [Migration("20190414133248_Models_BuildingImage_Added")]
    partial class Models_BuildingImage_Added
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.3-rtm-10026")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Library.Models.Book", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Author");

                    b.Property<int>("ISBN");

                    b.Property<int>("ReleaseYear");

                    b.Property<string>("Title");

                    b.HasKey("ID");

                    b.ToTable("Book");
                });

            modelBuilder.Entity("Library.Models.Guest", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<int>("GuestID");

                    b.Property<string>("Name");

                    b.Property<string>("Password");

                    b.Property<string>("PhoneNumber");

                    b.HasKey("ID");

                    b.ToTable("Guest");
                });

            modelBuilder.Entity("Library.Models.Lending", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("EndDay");

                    b.Property<int?>("GuestID");

                    b.Property<bool>("IsActive");

                    b.Property<DateTime>("StartDay");

                    b.Property<int?>("VolID");

                    b.HasKey("ID");

                    b.HasIndex("GuestID");

                    b.HasIndex("VolID");

                    b.ToTable("Lending");
                });

            modelBuilder.Entity("Library.Models.Librarian", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("LibrarianID");

                    b.Property<string>("Name");

                    b.Property<string>("Password");

                    b.HasKey("ID");

                    b.ToTable("Librarian");
                });

            modelBuilder.Entity("Library.Models.Vol", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("BookID");

                    b.Property<int>("VolID");

                    b.HasKey("ID");

                    b.HasIndex("BookID");

                    b.ToTable("Vol");
                });

            modelBuilder.Entity("Library.Models.Lending", b =>
                {
                    b.HasOne("Library.Models.Guest", "Guest")
                        .WithMany()
                        .HasForeignKey("GuestID");

                    b.HasOne("Library.Models.Vol", "Vol")
                        .WithMany()
                        .HasForeignKey("VolID");
                });

            modelBuilder.Entity("Library.Models.Vol", b =>
                {
                    b.HasOne("Library.Models.Book", "Book")
                        .WithMany()
                        .HasForeignKey("BookID");
                });
#pragma warning restore 612, 618
        }
    }
}
