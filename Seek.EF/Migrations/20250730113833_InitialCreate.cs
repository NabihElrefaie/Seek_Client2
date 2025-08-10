using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seek.EF.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FK_Auth",
                columns: table => new
                {
                    Ud = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HashedLogin = table.Column<string>(type: "TEXT", nullable: false),
                    HashedPassword = table.Column<string>(type: "TEXT", nullable: false),
                    Hashed_Refresh_Token = table.Column<string>(type: "TEXT", nullable: false),
                    ExpiresIn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FK_Auth", x => x.Ud);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FK_Auth_HashedLogin",
                table: "FK_Auth",
                column: "HashedLogin",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FK_Auth");
        }
    }
}
