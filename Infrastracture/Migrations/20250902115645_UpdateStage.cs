using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastracture.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameTable(
                name: "ProjectParticipants",
                newName: "ProjectUser");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectParticipants_ProjectsId",
                table: "ProjectUser",
                newName: "IX_ProjectUser_ProjectsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectUser",
                table: "ProjectUser",
                columns: new[] { "ParticipantsId", "ProjectsId" });

            migrationBuilder.CreateTable(
                name: "Stages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stages_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StageAssignments",
                columns: table => new
                {
                    StageId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageAssignments", x => new { x.StageId, x.UserId });
                    table.ForeignKey(
                        name: "FK_StageAssignments_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "Stages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StageAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Commitments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StageAssignmentStageId = table.Column<Guid>(type: "uuid", nullable: false),
                    StageAssignmentUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    CommitDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commitments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commitments_StageAssignments_StageAssignmentStageId_StageAs~",
                        columns: x => new { x.StageAssignmentStageId, x.StageAssignmentUserId },
                        principalTable: "StageAssignments",
                        principalColumns: new[] { "StageId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Commitments_StageAssignmentStageId_StageAssignmentUserId",
                table: "Commitments",
                columns: new[] { "StageAssignmentStageId", "StageAssignmentUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_StageAssignments_UserId",
                table: "StageAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Stages_ProjectId",
                table: "Stages",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectUser_Projects_ProjectsId",
                table: "ProjectUser",
                column: "ProjectsId",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectUser_Projects_ProjectsId",
                table: "ProjectUser");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectUser_Users_ParticipantsId",
                table: "ProjectUser");

            migrationBuilder.DropTable(
                name: "Commitments");

            migrationBuilder.DropTable(
                name: "StageAssignments");

            migrationBuilder.DropTable(
                name: "Stages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectUser",
                table: "ProjectUser");

            migrationBuilder.RenameTable(
                name: "ProjectUser",
                newName: "ProjectParticipants");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectUser_ProjectsId",
                table: "ProjectParticipants",
                newName: "IX_ProjectParticipants_ProjectsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectParticipants",
                table: "ProjectParticipants",
                columns: new[] { "ParticipantsId", "ProjectsId" });

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
    }
}
