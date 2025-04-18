﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TransactionalOutbox.Infrastructure.Data;

#nullable disable

namespace TransactionalOutbox.Infrastructure.Migrations
{
    [DbContext(typeof(OutboxDbContext))]
    partial class OutboxDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TransactionalOutbox.Abstractions.LogTailingOutboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PayloadType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("LogTailingOutboxMessages");
                });

            modelBuilder.Entity("TransactionalOutbox.Abstractions.PollingOutboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PayloadType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ProcessedCount")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("ProcessedDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ProcessedDate", "ProcessedCount", "CreationDate");

                    b.ToTable("PollingOutboxMessages");
                });
#pragma warning restore 612, 618
        }
    }
}
