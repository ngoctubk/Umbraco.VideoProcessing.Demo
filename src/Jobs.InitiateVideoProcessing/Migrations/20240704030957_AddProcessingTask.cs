using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jobs.InitiateVideoProcessing.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessingTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoEvents_VideoId",
                table: "VideoEvents");

            migrationBuilder.DropColumn(
                name: "SuccessEventId",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "VideoId",
                table: "VideoEvents");

            migrationBuilder.CreateTable(
                name: "ProcessingTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VideoPath = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TaskName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsDone = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessingTasks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingTasks_CreatedDate",
                table: "ProcessingTasks",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingTasks_ModifiedDate",
                table: "ProcessingTasks",
                column: "ModifiedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingTasks_TaskName",
                table: "ProcessingTasks",
                column: "TaskName");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingTasks_VideoPath",
                table: "ProcessingTasks",
                column: "VideoPath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessingTasks");

            migrationBuilder.AddColumn<Guid>(
                name: "SuccessEventId",
                table: "Videos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VideoId",
                table: "VideoEvents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_VideoEvents_VideoId",
                table: "VideoEvents",
                column: "VideoId");
        }
    }
}
