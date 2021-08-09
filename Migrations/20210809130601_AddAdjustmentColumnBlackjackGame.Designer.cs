﻿// <auto-generated />
using System;
using Abyss.Persistence.Document;
using Abyss.Persistence.Relational;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Abyss.Migrations
{
    [DbContext(typeof(AbyssPersistenceContext))]
    [Migration("20210809130601_AddAdjustmentColumnBlackjackGame")]
    partial class AddAdjustmentColumnBlackjackGame
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.3")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("Abyss.Persistence.Relational.BlackjackGameRecord", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<decimal>("Adjustment")
                        .HasColumnType("numeric");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("DealerCards")
                        .HasColumnType("text");

                    b.Property<bool>("DidPlayerDoubleDown")
                        .HasColumnType("boolean");

                    b.Property<decimal>("PlayerBalanceAfterGame")
                        .HasColumnType("numeric");

                    b.Property<decimal>("PlayerBalanceBeforeGame")
                        .HasColumnType("numeric");

                    b.Property<string>("PlayerCards")
                        .HasColumnType("text");

                    b.Property<decimal>("PlayerFinalBet")
                        .HasColumnType("numeric");

                    b.Property<decimal>("PlayerId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("PlayerInitialBet")
                        .HasColumnType("numeric");

                    b.Property<string>("Result")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("BlackjackGames");
                });

            modelBuilder.Entity("Abyss.Persistence.Relational.JsonRow<Abyss.Persistence.Document.GuildConfig>", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<GuildConfig>("Data")
                        .HasColumnType("jsonb");

                    b.HasKey("GuildId");

                    b.ToTable("GuildConfigurations");
                });

            modelBuilder.Entity("Abyss.Persistence.Relational.Reminder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("CreatorId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTimeOffset>("DueAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("MessageId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Text")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("Id");

                    b.ToTable("Reminders");
                });

            modelBuilder.Entity("Abyss.Persistence.Relational.TriviaCategoryVoteRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("CategoryId")
                        .HasColumnType("text");

                    b.Property<string>("CategoryName")
                        .HasColumnType("text");

                    b.Property<int>("TimesPicked")
                        .HasColumnType("integer");

                    b.Property<decimal>("TriviaRecordId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("TriviaRecordId");

                    b.ToTable("TriviaCategoryVoteRecord");
                });

            modelBuilder.Entity("Abyss.Persistence.Relational.TriviaRecord", b =>
                {
                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("CorrectAnswers")
                        .HasColumnType("integer");

                    b.Property<int>("IncorrectAnswers")
                        .HasColumnType("integer");

                    b.Property<int>("TotalMatches")
                        .HasColumnType("integer");

                    b.HasKey("UserId");

                    b.ToTable("TriviaRecords");
                });

            modelBuilder.Entity("Abyss.Persistence.Relational.UserAccount", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("BadgesString")
                        .HasColumnType("text");

                    b.Property<decimal>("Coins")
                        .HasColumnType("numeric");

                    b.Property<int>("ColorB")
                        .HasColumnType("integer");

                    b.Property<int>("ColorG")
                        .HasColumnType("integer");

                    b.Property<int>("ColorR")
                        .HasColumnType("integer");

                    b.Property<string>("Description")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<DateTimeOffset?>("FirstInteraction")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("LatestInteraction")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("UserAccounts");
                });

            modelBuilder.Entity("Abyss.Persistence.Relational.TriviaCategoryVoteRecord", b =>
                {
                    b.HasOne("Abyss.Persistence.Relational.TriviaRecord", "TriviaRecord")
                        .WithMany("CategoryVoteRecords")
                        .HasForeignKey("TriviaRecordId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TriviaRecord");
                });

            modelBuilder.Entity("Abyss.Persistence.Relational.TriviaRecord", b =>
                {
                    b.Navigation("CategoryVoteRecords");
                });
#pragma warning restore 612, 618
        }
    }
}
