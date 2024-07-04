using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jobs.InitiateVideoProcessing.Migrations
{
    /// <inheritdoc />
    public partial class AddEventMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventMessage",
                table: "VideoEvents",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventMessage",
                table: "VideoEvents");
        }
    }
}
