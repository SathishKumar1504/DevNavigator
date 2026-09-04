using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevNavigator.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFileRepositoryRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProjectName",
                table: "Files",
                newName: "RelativePath");

            migrationBuilder.AddColumn<DateTime>(
                name: "IndexedAt",
                table: "Files",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "RepositoryId",
                table: "Files",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Files_RepositoryId",
                table: "Files",
                column: "RepositoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Repositories_RepositoryId",
                table: "Files",
                column: "RepositoryId",
                principalTable: "Repositories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Repositories_RepositoryId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_RepositoryId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "IndexedAt",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "RepositoryId",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "RelativePath",
                table: "Files",
                newName: "ProjectName");
        }
    }
}
