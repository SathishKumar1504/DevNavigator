using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevNavigator.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeSymbolRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CodeSymbolRelationships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FromSymbolId = table.Column<int>(type: "INTEGER", nullable: false),
                    ToSymbolId = table.Column<int>(type: "INTEGER", nullable: false),
                    RelationshipType = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeSymbolRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodeSymbolRelationships_CodeSymbols_FromSymbolId",
                        column: x => x.FromSymbolId,
                        principalTable: "CodeSymbols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CodeSymbolRelationships_CodeSymbols_ToSymbolId",
                        column: x => x.ToSymbolId,
                        principalTable: "CodeSymbols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodeSymbolRelationships_FromSymbolId",
                table: "CodeSymbolRelationships",
                column: "FromSymbolId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeSymbolRelationships_ToSymbolId",
                table: "CodeSymbolRelationships",
                column: "ToSymbolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodeSymbolRelationships");
        }
    }
}
