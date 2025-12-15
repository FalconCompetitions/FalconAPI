using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoTccBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ExerciseTypes",
                columns: new[] { "Id", "Label" },
                values: new object[,]
                {
                    { 1, "Estruturas de Dados" },
                    { 2, "Algoritmos" },
                    { 3, "Matemática Computacional" },
                    { 4, "Grafos" },
                    { 5, "Programação Dinâmica" },
                    { 6, "Geometria Computacional" },
                    { 7, "Teoria dos Números" },
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
