using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jobs.InitiateVideoProcessing.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaPartPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MediaPartPath",
                table: "ProcessingTasks",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Resolution",
                table: "ProcessingTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingTasks_MediaPartPath",
                table: "ProcessingTasks",
                column: "MediaPartPath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProcessingTasks_MediaPartPath",
                table: "ProcessingTasks");

            migrationBuilder.DropColumn(
                name: "MediaPartPath",
                table: "ProcessingTasks");

            migrationBuilder.DropColumn(
                name: "Resolution",
                table: "ProcessingTasks");
        }
    }
}
