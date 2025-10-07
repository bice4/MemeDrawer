using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemDrawer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MemItemsInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BlobFileName = table.Column<string>(type: "TEXT", nullable: false),
                    Md5Hash = table.Column<string>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.UniqueConstraint("AK_Images_Md5Hash", x => x.Md5Hash);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Images");
        }
    }
}
