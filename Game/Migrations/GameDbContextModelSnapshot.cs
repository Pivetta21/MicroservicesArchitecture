﻿// <auto-generated />
using System;
using Game.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Game.Migrations
{
    [DbContext(typeof(GameDbContext))]
    partial class GameDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DungeonsItems", b =>
                {
                    b.Property<long>("DungeonsId")
                        .HasColumnType("bigint")
                        .HasColumnName("dungeons_id");

                    b.Property<long>("RewardsId")
                        .HasColumnType("bigint")
                        .HasColumnName("rewards_id");

                    b.HasKey("DungeonsId", "RewardsId")
                        .HasName("pk_dungeons_items");

                    b.HasIndex("RewardsId")
                        .HasDatabaseName("ix_dungeons_items_rewards_id");

                    b.ToTable("dungeons_items", (string)null);
                });

            modelBuilder.Entity("Game.Models.DungeonEntrances", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<Guid>("CharacterTransactionId")
                        .HasColumnType("uuid")
                        .HasColumnName("character_transaction_id");

                    b.Property<long>("DungeonId")
                        .HasColumnType("bigint")
                        .HasColumnName("dungeon_id");

                    b.Property<bool>("Processed")
                        .HasColumnType("boolean")
                        .HasColumnName("processed");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.HasKey("Id")
                        .HasName("pk_dungeon_entrances");

                    b.HasIndex("DungeonId")
                        .HasDatabaseName("ix_dungeon_entrances_dungeon_id");

                    b.ToTable("dungeon_entrances", (string)null);
                });

            modelBuilder.Entity("Game.Models.DungeonJournals", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<Guid>("CharacterTransactionId")
                        .HasColumnType("uuid")
                        .HasColumnName("character_transaction_id");

                    b.Property<long>("DungeonId")
                        .HasColumnType("bigint")
                        .HasColumnName("dungeon_id");

                    b.Property<long>("ElapsedMilliseconds")
                        .HasColumnType("bigint")
                        .HasColumnName("elapsed_milliseconds");

                    b.Property<long>("RewardId")
                        .HasColumnType("bigint")
                        .HasColumnName("reward_id");

                    b.Property<bool?>("WasSuccessful")
                        .HasColumnType("boolean")
                        .HasColumnName("was_successful");

                    b.HasKey("Id")
                        .HasName("pk_dungeon_journals");

                    b.HasIndex("DungeonId")
                        .HasDatabaseName("ix_dungeon_journals_dungeon_id");

                    b.HasIndex("RewardId")
                        .HasDatabaseName("ix_dungeon_journals_reward_id");

                    b.ToTable("dungeon_journals", (string)null);
                });

            modelBuilder.Entity("Game.Models.Dungeons", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("Cost")
                        .HasColumnType("bigint")
                        .HasColumnName("cost");

                    b.Property<int>("Difficulty")
                        .HasColumnType("integer")
                        .HasColumnName("difficulty");

                    b.Property<long>("MaxExperience")
                        .HasColumnType("bigint")
                        .HasColumnName("max_experience");

                    b.Property<long>("MaxGold")
                        .HasColumnType("bigint")
                        .HasColumnName("max_gold");

                    b.Property<long>("MinExperience")
                        .HasColumnType("bigint")
                        .HasColumnName("min_experience");

                    b.Property<long>("MinGold")
                        .HasColumnType("bigint")
                        .HasColumnName("min_gold");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<int>("RequiredLevel")
                        .HasColumnType("integer")
                        .HasColumnName("required_level");

                    b.HasKey("Id")
                        .HasName("pk_dungeons");

                    b.ToTable("dungeons", (string)null);
                });

            modelBuilder.Entity("Game.Models.Items", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("MaxQuality")
                        .HasColumnType("integer")
                        .HasColumnName("max_quality");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<int>("Rarity")
                        .HasColumnType("integer")
                        .HasColumnName("rarity");

                    b.Property<Guid>("TransactionId")
                        .HasColumnType("uuid")
                        .HasColumnName("transaction_id");

                    b.HasKey("Id");

                    b.ToTable("items", (string)null);

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("Game.Models.Armors", b =>
                {
                    b.HasBaseType("Game.Models.Items");

                    b.Property<int>("Resistance")
                        .HasColumnType("integer")
                        .HasColumnName("resistance");

                    b.ToTable("armors", (string)null);
                });

            modelBuilder.Entity("Game.Models.Weapons", b =>
                {
                    b.HasBaseType("Game.Models.Items");

                    b.Property<int>("Power")
                        .HasColumnType("integer")
                        .HasColumnName("power");

                    b.ToTable("weapons", (string)null);
                });

            modelBuilder.Entity("DungeonsItems", b =>
                {
                    b.HasOne("Game.Models.Dungeons", null)
                        .WithMany()
                        .HasForeignKey("DungeonsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_dungeons_items_dungeons_dungeons_id");

                    b.HasOne("Game.Models.Items", null)
                        .WithMany()
                        .HasForeignKey("RewardsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_dungeons_items_items_rewards_id");
                });

            modelBuilder.Entity("Game.Models.DungeonEntrances", b =>
                {
                    b.HasOne("Game.Models.Dungeons", "Dungeon")
                        .WithMany()
                        .HasForeignKey("DungeonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_dungeon_entrances_dungeons_dungeon_id");

                    b.Navigation("Dungeon");
                });

            modelBuilder.Entity("Game.Models.DungeonJournals", b =>
                {
                    b.HasOne("Game.Models.Dungeons", "Dungeon")
                        .WithMany()
                        .HasForeignKey("DungeonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_dungeon_journals_dungeons_dungeon_id");

                    b.HasOne("Game.Models.Items", "Reward")
                        .WithMany("DungeonJournals")
                        .HasForeignKey("RewardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_dungeon_journals_items_reward_id");

                    b.Navigation("Dungeon");

                    b.Navigation("Reward");
                });

            modelBuilder.Entity("Game.Models.Armors", b =>
                {
                    b.HasOne("Game.Models.Items", null)
                        .WithOne()
                        .HasForeignKey("Game.Models.Armors", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_armors_items_id");
                });

            modelBuilder.Entity("Game.Models.Weapons", b =>
                {
                    b.HasOne("Game.Models.Items", null)
                        .WithOne()
                        .HasForeignKey("Game.Models.Weapons", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_weapons_items_id");
                });

            modelBuilder.Entity("Game.Models.Items", b =>
                {
                    b.Navigation("DungeonJournals");
                });
#pragma warning restore 612, 618
        }
    }
}
