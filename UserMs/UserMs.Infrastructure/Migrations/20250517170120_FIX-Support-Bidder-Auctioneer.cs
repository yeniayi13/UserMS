using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserMs.Infrastructure.Migrations
{

    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class FIXSupportBidderAuctioneer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auctioneers_Users_AuctioneerUserId",
                table: "Auctioneers");

            migrationBuilder.DropForeignKey(
                name: "FK_Bidders_Users_BidderUserId",
                table: "Bidders");

            migrationBuilder.DropForeignKey(
                name: "FK_Supports_Users_SupportUserId",
                table: "Supports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Supports",
                table: "Supports");

            migrationBuilder.DropIndex(
                name: "IX_Supports_SupportUserId",
                table: "Supports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bidders",
                table: "Bidders");

            migrationBuilder.DropIndex(
                name: "IX_Bidders_BidderUserId",
                table: "Bidders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Auctioneers",
                table: "Auctioneers");

            migrationBuilder.DropIndex(
                name: "IX_Auctioneers_AuctioneerUserId",
                table: "Auctioneers");

            migrationBuilder.DropColumn(
                name: "SupportId",
                table: "Supports");

            migrationBuilder.DropColumn(
                name: "BidderId",
                table: "Bidders");

            migrationBuilder.DropColumn(
                name: "AuctioneerId",
                table: "Auctioneers");

            migrationBuilder.RenameColumn(
                name: "SupportUserId",
                table: "Supports",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "BidderUserId",
                table: "Bidders",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "AuctioneerUserId",
                table: "Auctioneers",
                newName: "UserId");

            migrationBuilder.AddColumn<string>(
                name: "UserAddress",
                table: "Supports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "Supports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserLastName",
                table: "Supports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Supports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserPassword",
                table: "Supports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserPhone",
                table: "Supports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserAddress",
                table: "Bidders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "Bidders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserLastName",
                table: "Bidders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Bidders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserPassword",
                table: "Bidders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserPhone",
                table: "Bidders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserAddress",
                table: "Auctioneers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "Auctioneers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserLastName",
                table: "Auctioneers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Auctioneers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserPassword",
                table: "Auctioneers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserPhone",
                table: "Auctioneers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Supports",
                table: "Supports",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bidders",
                table: "Bidders",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Auctioneers",
                table: "Auctioneers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Supports",
                table: "Supports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bidders",
                table: "Bidders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Auctioneers",
                table: "Auctioneers");

            migrationBuilder.DropColumn(
                name: "UserAddress",
                table: "Supports");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "Supports");

            migrationBuilder.DropColumn(
                name: "UserLastName",
                table: "Supports");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Supports");

            migrationBuilder.DropColumn(
                name: "UserPassword",
                table: "Supports");

            migrationBuilder.DropColumn(
                name: "UserPhone",
                table: "Supports");

            migrationBuilder.DropColumn(
                name: "UserAddress",
                table: "Bidders");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "Bidders");

            migrationBuilder.DropColumn(
                name: "UserLastName",
                table: "Bidders");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Bidders");

            migrationBuilder.DropColumn(
                name: "UserPassword",
                table: "Bidders");

            migrationBuilder.DropColumn(
                name: "UserPhone",
                table: "Bidders");

            migrationBuilder.DropColumn(
                name: "UserAddress",
                table: "Auctioneers");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "Auctioneers");

            migrationBuilder.DropColumn(
                name: "UserLastName",
                table: "Auctioneers");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Auctioneers");

            migrationBuilder.DropColumn(
                name: "UserPassword",
                table: "Auctioneers");

            migrationBuilder.DropColumn(
                name: "UserPhone",
                table: "Auctioneers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Supports",
                newName: "SupportUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Bidders",
                newName: "BidderUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Auctioneers",
                newName: "AuctioneerUserId");

            migrationBuilder.AddColumn<Guid>(
                name: "SupportId",
                table: "Supports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BidderId",
                table: "Bidders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AuctioneerId",
                table: "Auctioneers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Supports",
                table: "Supports",
                column: "SupportId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bidders",
                table: "Bidders",
                column: "BidderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Auctioneers",
                table: "Auctioneers",
                column: "AuctioneerId");

            migrationBuilder.CreateIndex(
                name: "IX_Supports_SupportUserId",
                table: "Supports",
                column: "SupportUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bidders_BidderUserId",
                table: "Bidders",
                column: "BidderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Auctioneers_AuctioneerUserId",
                table: "Auctioneers",
                column: "AuctioneerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auctioneers_Users_AuctioneerUserId",
                table: "Auctioneers",
                column: "AuctioneerUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bidders_Users_BidderUserId",
                table: "Bidders",
                column: "BidderUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Supports_Users_SupportUserId",
                table: "Supports",
                column: "SupportUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
