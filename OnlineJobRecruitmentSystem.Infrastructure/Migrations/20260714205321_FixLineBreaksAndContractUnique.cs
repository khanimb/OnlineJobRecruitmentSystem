using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineJobRecruitmentSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixLineBreaksAndContractUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contracts_JobPostId",
                table: "Contracts");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_JobPostId_JobSeekerProfileId",
                table: "Contracts",
                columns: new[] { "JobPostId", "JobSeekerProfileId" },
                unique: true,
                filter: "[Status] = 'Active'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contracts_JobPostId_JobSeekerProfileId",
                table: "Contracts");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_JobPostId",
                table: "Contracts",
                column: "JobPostId");
        }
    }
}
