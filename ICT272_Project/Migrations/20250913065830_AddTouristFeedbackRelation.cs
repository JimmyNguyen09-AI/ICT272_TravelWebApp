using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ICT272_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddTouristFeedbackRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_TouristID",
                table: "Feedbacks",
                column: "TouristID");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Tourists_TouristID",
                table: "Feedbacks",
                column: "TouristID",
                principalTable: "Tourists",
                principalColumn: "TouristID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Tourists_TouristID",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_TouristID",
                table: "Feedbacks");
        }
    }
}
