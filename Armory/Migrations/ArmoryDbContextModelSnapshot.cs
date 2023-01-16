﻿// <auto-generated />
using System;
using Armory.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Armory.Migrations
{
    [DbContext(typeof(ArmoryDbContext))]
    partial class ArmoryDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Armory.Models.Builds", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("ArmorId")
                        .HasColumnType("bigint")
                        .HasColumnName("armor_id");

                    b.Property<long>("WeaponId")
                        .HasColumnType("bigint")
                        .HasColumnName("weapon_id");

                    b.HasKey("Id")
                        .HasName("pk_builds");

                    b.HasIndex("ArmorId")
                        .IsUnique()
                        .HasDatabaseName("ix_builds_armor_id");

                    b.HasIndex("WeaponId")
                        .IsUnique()
                        .HasDatabaseName("ix_builds_weapon_id");

                    b.ToTable("builds", (string)null);
                });

            modelBuilder.Entity("Armory.Models.Characters", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("BuildId")
                        .HasColumnType("bigint")
                        .HasColumnName("build_id");

                    b.Property<int>("Damage")
                        .HasColumnType("integer")
                        .HasColumnName("damage");

                    b.Property<double>("Experience")
                        .HasColumnType("double precision")
                        .HasColumnName("experience");

                    b.Property<long>("Gold")
                        .HasColumnType("bigint")
                        .HasColumnName("gold");

                    b.Property<long>("InventoryId")
                        .HasColumnType("bigint")
                        .HasColumnName("inventory_id");

                    b.Property<bool>("IsPlaying")
                        .HasColumnType("boolean")
                        .HasColumnName("is_playing");

                    b.Property<int>("Level")
                        .HasColumnType("integer")
                        .HasColumnName("level");

                    b.Property<int>("Life")
                        .HasColumnType("integer")
                        .HasColumnName("life");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("character varying(40)")
                        .HasColumnName("name");

                    b.Property<int>("Specialization")
                        .HasColumnType("integer")
                        .HasColumnName("specialization");

                    b.Property<Guid>("TransactionId")
                        .HasColumnType("uuid")
                        .HasColumnName("transaction_id");

                    b.Property<Guid>("UserTransactionId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_transaction_id");

                    b.HasKey("Id")
                        .HasName("pk_characters");

                    b.HasIndex("BuildId")
                        .IsUnique()
                        .HasDatabaseName("ix_characters_build_id");

                    b.HasIndex("InventoryId")
                        .IsUnique()
                        .HasDatabaseName("ix_characters_inventory_id");

                    b.ToTable("characters", (string)null);
                });

            modelBuilder.Entity("Armory.Models.DungeonEntrances", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("CharacterId")
                        .HasColumnType("bigint")
                        .HasColumnName("character_id");

                    b.Property<Guid>("DungeonTransactionId")
                        .HasColumnType("uuid")
                        .HasColumnName("dungeon_transaction_id");

                    b.Property<long?>("PayedFee")
                        .HasColumnType("bigint")
                        .HasColumnName("payed_fee");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.Property<Guid>("TransactionId")
                        .HasColumnType("uuid")
                        .HasColumnName("transaction_id");

                    b.HasKey("Id")
                        .HasName("pk_dungeon_entrances");

                    b.HasIndex("CharacterId")
                        .HasDatabaseName("ix_dungeon_entrances_character_id");

                    b.ToTable("dungeon_entrances", (string)null);
                });

            modelBuilder.Entity("Armory.Models.Inventories", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("Size")
                        .HasColumnType("integer")
                        .HasColumnName("size");

                    b.HasKey("Id")
                        .HasName("pk_inventories");

                    b.ToTable("inventories", (string)null);
                });

            modelBuilder.Entity("Armory.Models.Items", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long?>("InventoryId")
                        .HasColumnType("bigint")
                        .HasColumnName("inventory_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("character varying(40)")
                        .HasColumnName("name");

                    b.Property<int>("Rarity")
                        .HasColumnType("integer")
                        .HasColumnName("rarity");

                    b.Property<Guid>("TransactionId")
                        .HasColumnType("uuid")
                        .HasColumnName("transaction_id");

                    b.HasKey("Id");

                    b.HasIndex("InventoryId")
                        .HasDatabaseName("ix_items_inventory_id");

                    b.ToTable("items", (string)null);

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("Armory.Models.Armors", b =>
                {
                    b.HasBaseType("Armory.Models.Items");

                    b.Property<int>("Resistance")
                        .HasColumnType("integer")
                        .HasColumnName("resistance");

                    b.ToTable("armors", (string)null);
                });

            modelBuilder.Entity("Armory.Models.Weapons", b =>
                {
                    b.HasBaseType("Armory.Models.Items");

                    b.Property<int>("Power")
                        .HasColumnType("integer")
                        .HasColumnName("power");

                    b.ToTable("weapons", (string)null);
                });

            modelBuilder.Entity("Armory.Models.Builds", b =>
                {
                    b.HasOne("Armory.Models.Armors", "Armor")
                        .WithOne("Build")
                        .HasForeignKey("Armory.Models.Builds", "ArmorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_builds_items_armor_id");

                    b.HasOne("Armory.Models.Weapons", "Weapon")
                        .WithOne("Build")
                        .HasForeignKey("Armory.Models.Builds", "WeaponId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_builds_items_weapon_id");

                    b.Navigation("Armor");

                    b.Navigation("Weapon");
                });

            modelBuilder.Entity("Armory.Models.Characters", b =>
                {
                    b.HasOne("Armory.Models.Builds", "Build")
                        .WithOne("Character")
                        .HasForeignKey("Armory.Models.Characters", "BuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_characters_builds_build_id");

                    b.HasOne("Armory.Models.Inventories", "Inventory")
                        .WithOne("Character")
                        .HasForeignKey("Armory.Models.Characters", "InventoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_characters_inventories_inventory_id");

                    b.Navigation("Build");

                    b.Navigation("Inventory");
                });

            modelBuilder.Entity("Armory.Models.DungeonEntrances", b =>
                {
                    b.HasOne("Armory.Models.Characters", "Character")
                        .WithMany()
                        .HasForeignKey("CharacterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_dungeon_entrances_characters_character_id");

                    b.Navigation("Character");
                });

            modelBuilder.Entity("Armory.Models.Items", b =>
                {
                    b.HasOne("Armory.Models.Inventories", "Inventory")
                        .WithMany("Items")
                        .HasForeignKey("InventoryId")
                        .HasConstraintName("fk_items_inventories_inventory_id");

                    b.Navigation("Inventory");
                });

            modelBuilder.Entity("Armory.Models.Armors", b =>
                {
                    b.HasOne("Armory.Models.Items", null)
                        .WithOne()
                        .HasForeignKey("Armory.Models.Armors", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_armors_items_id");
                });

            modelBuilder.Entity("Armory.Models.Weapons", b =>
                {
                    b.HasOne("Armory.Models.Items", null)
                        .WithOne()
                        .HasForeignKey("Armory.Models.Weapons", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_weapons_items_id");
                });

            modelBuilder.Entity("Armory.Models.Builds", b =>
                {
                    b.Navigation("Character")
                        .IsRequired();
                });

            modelBuilder.Entity("Armory.Models.Inventories", b =>
                {
                    b.Navigation("Character")
                        .IsRequired();

                    b.Navigation("Items");
                });

            modelBuilder.Entity("Armory.Models.Armors", b =>
                {
                    b.Navigation("Build")
                        .IsRequired();
                });

            modelBuilder.Entity("Armory.Models.Weapons", b =>
                {
                    b.Navigation("Build")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
