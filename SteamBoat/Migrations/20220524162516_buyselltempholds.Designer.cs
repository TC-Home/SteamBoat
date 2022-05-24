﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SteamBoat.Data;

namespace SteamBoat.Migrations
{
    [DbContext(typeof(SteamBoatContext))]
    [Migration("20220524162516_buyselltempholds")]
    partial class buyselltempholds
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.16")
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

            modelBuilder.Entity("SteamBoat.Models.Item", b =>
                {
                    b.Property<string>("hash_name_key")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AH1")
                        .HasColumnType("int");

                    b.Property<int>("AH10")
                        .HasColumnType("int");

                    b.Property<int>("AH2")
                        .HasColumnType("int");

                    b.Property<int>("AH3")
                        .HasColumnType("int");

                    b.Property<int>("AH4")
                        .HasColumnType("int");

                    b.Property<int>("AH5")
                        .HasColumnType("int");

                    b.Property<int>("AH6")
                        .HasColumnType("int");

                    b.Property<int>("AH7")
                        .HasColumnType("int");

                    b.Property<int>("AH8")
                        .HasColumnType("int");

                    b.Property<int>("AH9")
                        .HasColumnType("int");

                    b.Property<int>("Activity")
                        .HasColumnType("int");

                    b.Property<int>("Ave_buy")
                        .HasColumnType("int");

                    b.Property<int>("Ave_profic_pc")
                        .HasColumnType("int");

                    b.Property<int>("Ave_profit")
                        .HasColumnType("int");

                    b.Property<int>("Ave_sell")
                        .HasColumnType("int");

                    b.Property<bool>("CancelCurrentBid")
                        .HasColumnType("bit");

                    b.Property<int>("Fruit")
                        .HasColumnType("int");

                    b.Property<int>("Fruit2")
                        .HasColumnType("int");

                    b.Property<string>("Game")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int>("Gap")
                        .HasColumnType("int");

                    b.Property<int>("Gap_mybid")
                        .HasColumnType("int");

                    b.Property<int>("IdealBidInt")
                        .HasColumnType("int");

                    b.Property<string>("IdealBidStr")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("IdealBid_Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("IdealSellInt")
                        .HasColumnType("int");

                    b.Property<string>("IdealSellStr")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("IdealSell_Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IncludeInAutoBid")
                        .HasColumnType("bit");

                    b.Property<string>("ItemPageURL")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ItemStatsURL")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("LastNumberSold")
                        .HasColumnType("int");

                    b.Property<int>("LastProfitInt")
                        .HasColumnType("int");

                    b.Property<DateTime>("LastSellDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("LastSellInt")
                        .HasColumnType("int");

                    b.Property<int>("Median")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int>("NumForSale")
                        .HasColumnType("int");

                    b.Property<int>("Recent")
                        .HasColumnType("int");

                    b.Property<int>("StartingPrice")
                        .HasColumnType("int");

                    b.Property<int>("bid1Price")
                        .HasColumnType("int");

                    b.Property<int>("bid1Quant")
                        .HasColumnType("int");

                    b.Property<int>("bid2Price")
                        .HasColumnType("int");

                    b.Property<int>("bid2Quant")
                        .HasColumnType("int");

                    b.Property<int>("bid3Price")
                        .HasColumnType("int");

                    b.Property<int>("bid3Quant")
                        .HasColumnType("int");

                    b.Property<int>("bid4Price")
                        .HasColumnType("int");

                    b.Property<int>("bid4Quant")
                        .HasColumnType("int");

                    b.Property<int>("bid5Price")
                        .HasColumnType("int");

                    b.Property<int>("bid5Quant")
                        .HasColumnType("int");

                    b.Property<int>("bid_price")
                        .HasColumnType("int");

                    b.Property<string>("bid_price_in_pound")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("bid_quant")
                        .HasColumnType("int");

                    b.Property<string>("buys_html")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("item_nameid")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("max_buy_price")
                        .HasColumnType("int");

                    b.Property<int>("min_sell_price")
                        .HasColumnType("int");

                    b.Property<int>("next_min_sell_price")
                        .HasColumnType("int");

                    b.Property<int>("orders")
                        .HasColumnType("int");

                    b.Property<int>("sell1Price")
                        .HasColumnType("int");

                    b.Property<int>("sell1Quant")
                        .HasColumnType("int");

                    b.Property<int>("sell2Price")
                        .HasColumnType("int");

                    b.Property<int>("sell2Quant")
                        .HasColumnType("int");

                    b.Property<int>("sell3Price")
                        .HasColumnType("int");

                    b.Property<int>("sell3Quant")
                        .HasColumnType("int");

                    b.Property<int>("sell4Price")
                        .HasColumnType("int");

                    b.Property<int>("sell4Quant")
                        .HasColumnType("int");

                    b.Property<int>("sell5Price")
                        .HasColumnType("int");

                    b.Property<int>("sell5Quant")
                        .HasColumnType("int");

                    b.Property<string>("sells_html")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("tempBuyHold")
                        .HasColumnType("bit");

                    b.Property<bool>("tempSellHold")
                        .HasColumnType("bit");

                    b.Property<int>("total_buys")
                        .HasColumnType("int");

                    b.Property<int>("total_buys_sum_amount")
                        .HasColumnType("int");

                    b.Property<int>("total_profit")
                        .HasColumnType("int");

                    b.Property<int>("total_profit_including_stock")
                        .HasColumnType("int");

                    b.Property<int>("total_profit_sales_only")
                        .HasColumnType("int");

                    b.Property<int>("total_sales")
                        .HasColumnType("int");

                    b.Property<int>("total_sales_sum_amount")
                        .HasColumnType("int");

                    b.HasKey("hash_name_key");

                    b.ToTable("Items");
                });

            modelBuilder.Entity("SteamBoat.Models.ItemForSale", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Game_hash_name_key")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("max_buy_bid")
                        .HasColumnType("int");

                    b.Property<int>("max_buy_bid_diff")
                        .HasColumnType("int");

                    b.Property<int>("sale_price")
                        .HasColumnType("int");

                    b.Property<int>("sale_price_after_fees")
                        .HasColumnType("int");

                    b.Property<int>("sale_price_diff")
                        .HasColumnType("int");

                    b.Property<string>("sell_price_without_fees")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Game_hash_name_key");

                    b.ToTable("ItemsForSale");
                });

            modelBuilder.Entity("SteamBoat.Models.Mission", b =>
                {
                    b.Property<int>("MissionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ItemUrl")
                        .HasColumnType("nvarchar(max)");

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

            modelBuilder.Entity("SteamBoat.Models.Transaction", b =>
                {
                    b.Property<string>("Tran_Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("DateT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Game_hash_name_key")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("int_sale_price_after_fees")
                        .HasColumnType("int");

                    b.Property<string>("sale_price_after_fees")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("type")
                        .IsRequired()
                        .HasColumnType("nvarchar(1)");

                    b.HasKey("Tran_Id");

                    b.HasIndex("Game_hash_name_key");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("SteamBoat.Models.exclude", b =>
                {
                    b.Property<string>("Game")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Game");

                    b.ToTable("exclude");
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

            modelBuilder.Entity("SteamBoat.Models.ItemForSale", b =>
                {
                    b.HasOne("SteamBoat.Models.Item", null)
                        .WithMany("ItemsForSale")
                        .HasForeignKey("Game_hash_name_key");
                });

            modelBuilder.Entity("SteamBoat.Models.Transaction", b =>
                {
                    b.HasOne("SteamBoat.Models.Item", null)
                        .WithMany("Transactions")
                        .HasForeignKey("Game_hash_name_key");
                });

            modelBuilder.Entity("SteamBoat.Models.Item", b =>
                {
                    b.Navigation("ItemsForSale");

                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("SteamBoat.Models.Mission", b =>
                {
                    b.Navigation("FeederUrls");
                });
#pragma warning restore 612, 618
        }
    }
}
