﻿// <auto-generated />
using System;
using Jobs.InitiateVideoProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Jobs.InitiateVideoProcessing.Migrations
{
    [DbContext(typeof(MediaProcessingDbContext))]
    [Migration("20240701021206_InitDb")]
    partial class InitDb
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Jobs.InitiateVideoProcessing.Models.Video", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsCut")
                        .HasColumnType("bit");

                    b.Property<bool>("IsExtracted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("RawMetadata")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("SuccessEventId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("VideoPath")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedDate");

                    b.HasIndex("IsCut");

                    b.HasIndex("IsExtracted");

                    b.HasIndex("ModifiedDate");

                    b.HasIndex("VideoPath");

                    b.ToTable("Videos", (string)null);
                });

            modelBuilder.Entity("Jobs.InitiateVideoProcessing.Models.VideoEvent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Event")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("EventDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("VideoId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("VideoPath")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.HasKey("Id");

                    b.HasIndex("Event");

                    b.HasIndex("EventDate");

                    b.HasIndex("VideoId");

                    b.HasIndex("VideoPath");

                    b.ToTable("VideoEvents", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}