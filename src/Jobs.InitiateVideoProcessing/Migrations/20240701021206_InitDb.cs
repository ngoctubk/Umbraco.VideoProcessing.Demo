using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jobs.InitiateVideoProcessing.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VideoEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VideoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VideoPath = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Event = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VideoPath = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    IsExtracted = table.Column<bool>(type: "bit", nullable: false),
                    IsCut = table.Column<bool>(type: "bit", nullable: false),
                    RawMetadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuccessEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoEvents_Event",
                table: "VideoEvents",
                column: "Event");

            migrationBuilder.CreateIndex(
                name: "IX_VideoEvents_EventDate",
                table: "VideoEvents",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_VideoEvents_VideoId",
                table: "VideoEvents",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoEvents_VideoPath",
                table: "VideoEvents",
                column: "VideoPath");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_CreatedDate",
                table: "Videos",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_IsCut",
                table: "Videos",
                column: "IsCut");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_IsExtracted",
                table: "Videos",
                column: "IsExtracted");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_ModifiedDate",
                table: "Videos",
                column: "ModifiedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_VideoPath",
                table: "Videos",
                column: "VideoPath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoEvents");

            migrationBuilder.DropTable(
                name: "Videos");
        }
    }
}
