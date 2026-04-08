using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Services.Task.Migrations
{
    /// <inheritdoc />
    public partial class OutBoxMessageInTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskStatusHistories_Tasks_TaskId",
                table: "TaskStatusHistories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Tasks");

            migrationBuilder.AddColumn<Guid>(
                name: "TaskId",
                table: "OutboxMessages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_TaskId",
                table: "OutboxMessages",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_OutboxMessages_Tasks_TaskId",
                table: "OutboxMessages",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OutboxMessages_Tasks_TaskId",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_TaskId",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "OutboxMessages");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Tasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_TaskStatusHistories_Tasks_TaskId",
                table: "TaskStatusHistories",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
