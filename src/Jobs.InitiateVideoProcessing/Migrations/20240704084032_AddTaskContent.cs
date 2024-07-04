using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jobs.InitiateVideoProcessing.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TaskContent",
                table: "ProcessingTasks",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaskContent",
                table: "ProcessingTasks");
        }
    }
}
