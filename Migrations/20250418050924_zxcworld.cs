using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class zxcworld : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_ProjectTasks_ProjectTaskId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_ProjectTasks_ProjectTaskId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMembers_Projects_ProjectId",
                table: "ProjectMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTasks_Projects_ProjectId",
                table: "ProjectTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignees_ProjectTasks_ProjectTaskId",
                table: "TaskAssignees");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistories_ProjectTasks_ProjectTaskId",
                table: "TaskHistories");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_ProjectTasks_ProjectTaskId",
                table: "Attachments",
                column: "ProjectTaskId",
                principalTable: "ProjectTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_ProjectTasks_ProjectTaskId",
                table: "Comments",
                column: "ProjectTaskId",
                principalTable: "ProjectTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMembers_Projects_ProjectId",
                table: "ProjectMembers",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTasks_Projects_ProjectId",
                table: "ProjectTasks",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignees_ProjectTasks_ProjectTaskId",
                table: "TaskAssignees",
                column: "ProjectTaskId",
                principalTable: "ProjectTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskHistories_ProjectTasks_ProjectTaskId",
                table: "TaskHistories",
                column: "ProjectTaskId",
                principalTable: "ProjectTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_ProjectTasks_ProjectTaskId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_ProjectTasks_ProjectTaskId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMembers_Projects_ProjectId",
                table: "ProjectMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTasks_Projects_ProjectId",
                table: "ProjectTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignees_ProjectTasks_ProjectTaskId",
                table: "TaskAssignees");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistories_ProjectTasks_ProjectTaskId",
                table: "TaskHistories");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_ProjectTasks_ProjectTaskId",
                table: "Attachments",
                column: "ProjectTaskId",
                principalTable: "ProjectTasks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_ProjectTasks_ProjectTaskId",
                table: "Comments",
                column: "ProjectTaskId",
                principalTable: "ProjectTasks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMembers_Projects_ProjectId",
                table: "ProjectMembers",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTasks_Projects_ProjectId",
                table: "ProjectTasks",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignees_ProjectTasks_ProjectTaskId",
                table: "TaskAssignees",
                column: "ProjectTaskId",
                principalTable: "ProjectTasks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskHistories_ProjectTasks_ProjectTaskId",
                table: "TaskHistories",
                column: "ProjectTaskId",
                principalTable: "ProjectTasks",
                principalColumn: "Id");
        }
    }
}
