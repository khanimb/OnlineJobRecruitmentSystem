using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineJobRecruitmentSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveJobPostLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobPostLimit",
                table: "EmployerProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JobPostLimit",
                table: "EmployerProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
