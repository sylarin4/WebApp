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
    [Migration("20190416134508_AddJoinToBookModel")]
    partial class AddJoinToBookModel
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

            modelBuilder.Entity("Library.Models.BookImage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BookId");

                    b.Property<byte[]>("Image");

                    b.HasKey("Id");

                    b.HasIndex("BookId");

                    b.ToTable("BookImage");
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

            modelBuilder.Entity("Library.Models.BookImage", b =>
                {
                    b.HasOne("Library.Models.Book", "Book")
                        .WithMany("BookImages")
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Library.Models.Lending", b =>
                {
                    b.HasOne("Library.Models.Guest", "Guest")
                        .WithMany("Lendings")
                        .HasForeignKey("GuestID");

                    b.HasOne("Library.Models.Vol", "Vol")
                        .WithMany("Lendings")
                        .HasForeignKey("VolID");
                });

            modelBuilder.Entity("Library.Models.Vol", b =>
                {
                    b.HasOne("Library.Models.Book", "Book")
                        .WithMany("Vols")
                        .HasForeignKey("BookID");
                });
#pragma warning restore 612, 618
        }
    }
}
