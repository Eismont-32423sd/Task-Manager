using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastracture.Migrations
{
    /// <inheritdoc />
    public partial class updated_connection_between_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_OwnerId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectUser_Projects_OwnedProjectsId",
                table: "ProjectUser");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectUser_Users_ParticipantsId",
                table: "ProjectUser");

            migrationBuilder.DropIndex(
                name: "IX_Projects_OwnerId",
                table: "Projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectUser",
                table: "ProjectUser");

            migrationBuilder.DropIndex(
                name: "IX_ProjectUser_ParticipantsId",
                table: "ProjectUser");

            migrationBuilder.RenameTable(
                name: "ProjectUser",
                newName: "ProjectParticipants");

            migrationBuilder.RenameColumn(
                name: "OwnedProjectsId",
                table: "ProjectParticipants",
                newName: "ProjectsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectParticipants",
                table: "ProjectParticipants",
                columns: new[] { "ParticipantsId", "ProjectsId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectParticipants_ProjectsId",
                table: "ProjectParticipants",
                column: "ProjectsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectParticipants_Projects_ProjectsId",
                table: "ProjectParticipants",
                column: "ProjectsId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectParticipants_Users_ParticipantsId",
                table: "ProjectParticipants",
                column: "ParticipantsId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectParticipants_Projects_ProjectsId",
                table: "ProjectParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectParticipants_Users_ParticipantsId",
                table: "ProjectParticipants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectParticipants",
                table: "ProjectParticipants");

            migrationBuilder.DropIndex(
                name: "IX_ProjectParticipants_ProjectsId",
                table: "ProjectParticipants");

            migrationBuilder.RenameTable(
                name: "ProjectParticipants",
                newName: "ProjectUser");

            migrationBuilder.RenameColumn(
                name: "ProjectsId",
                table: "ProjectUser",
                newName: "OwnedProjectsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectUser",
                table: "ProjectUser",
                columns: new[] { "OwnedProjectsId", "ParticipantsId" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OwnerId",
                table: "Projects",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectUser_ParticipantsId",
                table: "ProjectUser",
                column: "ParticipantsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_OwnerId",
                table: "Projects",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectUser_Projects_OwnedProjectsId",
                table: "ProjectUser",
                column: "OwnedProjectsId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectUser_Users_ParticipantsId",
                table: "ProjectUser",
                column: "ParticipantsId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
