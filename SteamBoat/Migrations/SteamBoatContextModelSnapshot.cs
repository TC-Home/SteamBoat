﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SteamBoat.Data;

namespace SteamBoat.Migrations
{
    [DbContext(typeof(SteamBoatContext))]
    partial class SteamBoatContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.12")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SteamBoat.Models.FeederUrl", b =>
                {
                    b.Property<int>("FeederId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("MissionId")
                        .HasColumnType("int");

                    b.Property<string>("Url")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("isJSON")
                        .HasColumnType("bit");

                    b.HasKey("FeederId");

                    b.HasIndex("MissionId");

                    b.ToTable("FeederUrl");
                });

            modelBuilder.Entity("SteamBoat.Models.Mission", b =>
                {
                    b.Property<int>("MissionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ListingsMax")
                        .HasColumnType("int");

                    b.Property<int>("ListingsMin")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int>("PriceMax")
                        .HasColumnType("int");

                    b.Property<int>("PriceMin")
                        .HasColumnType("int");

                    b.HasKey("MissionId");

                    b.ToTable("Mission");
                });

            modelBuilder.Entity("SteamBoat.Models.FeederUrl", b =>
                {
                    b.HasOne("SteamBoat.Models.Mission", "Mission")
                        .WithMany("FeederUrls")
                        .HasForeignKey("MissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Mission");
                });

            modelBuilder.Entity("SteamBoat.Models.Mission", b =>
                {
                    b.Navigation("FeederUrls");
                });
#pragma warning restore 612, 618
        }
    }
}
