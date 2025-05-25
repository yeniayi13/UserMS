using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserMs.Infrastructure.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class ADDBidderAuctioneerSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Auctioneers",
                columns: table => new
                {
                    AuctioneerId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuctioneerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuctioneerDni = table.Column<string>(type: "text", nullable: false),
                    AuctioneerDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    AuctioneerBirthday = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auctioneers", x => x.AuctioneerId);
                    table.ForeignKey(
                        name: "FK_Auctioneers_Users_AuctioneerUserId",
                        column: x => x.AuctioneerUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bidders",
                columns: table => new
                {
                    BidderId = table.Column<Guid>(type: "uuid", nullable: false),
                    BidderUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BidderDni = table.Column<string>(type: "text", nullable: false),
                    BidderBirthday = table.Column<DateOnly>(type: "date", nullable: false),
                    BidderDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bidders", x => x.BidderId);
                    table.ForeignKey(
                        name: "FK_Bidders_Users_BidderUserId",
                        column: x => x.BidderUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Supports",
                columns: table => new
                {
                    SupportId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupportUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupportDni = table.Column<string>(type: "text", nullable: false),
                    SupportDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    SupportSpecialization = table.Column<int>(type: "integer", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supports", x => x.SupportId);
                    table.ForeignKey(
                        name: "FK_Supports_Users_SupportUserId",
                        column: x => x.SupportUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auctioneers_AuctioneerUserId",
                table: "Auctioneers",
                column: "AuctioneerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bidders_BidderUserId",
                table: "Bidders",
                column: "BidderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Supports_SupportUserId",
                table: "Supports",
                column: "SupportUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auctioneers");

            migrationBuilder.DropTable(
                name: "Bidders");

            migrationBuilder.DropTable(
                name: "Supports");
        }
    }
}
