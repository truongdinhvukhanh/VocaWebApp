using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VocabWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserId1 = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AspNetUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId1 = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Folders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Folders_AspNetUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VocaSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FolderId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Keywords = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastAccessed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocaSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VocaSets_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VocaSets_Folders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ReviewReminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId1 = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VocaSetId = table.Column<int>(type: "int", nullable: false),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RepeatIntervalDays = table.Column<int>(type: "int", nullable: true),
                    IsEmail = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsNotification = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsSent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    VocaSetId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewReminders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewReminders_AspNetUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewReminders_VocaSets_VocaSetId",
                        column: x => x.VocaSetId,
                        principalTable: "VocaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewReminders_VocaSets_VocaSetId1",
                        column: x => x.VocaSetId1,
                        principalTable: "VocaSets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VocaItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VocaSetId = table.Column<int>(type: "int", nullable: false),
                    Word = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WordType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Pronunciation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AudioUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Meaning = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ExampleSentence = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "not_learned"),
                    VocaSetId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocaItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VocaItems_VocaSets_VocaSetId",
                        column: x => x.VocaSetId,
                        principalTable: "VocaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VocaItems_VocaSets_VocaSetId1",
                        column: x => x.VocaSetId1,
                        principalTable: "VocaSets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VocaSetCopies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginalSetId = table.Column<int>(type: "int", nullable: false),
                    CopiedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CopiedByUserId1 = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CopiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VocaSetId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocaSetCopies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VocaSetCopies_AspNetUsers_CopiedByUserId",
                        column: x => x.CopiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VocaSetCopies_AspNetUsers_CopiedByUserId1",
                        column: x => x.CopiedByUserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VocaSetCopies_VocaSets_OriginalSetId",
                        column: x => x.OriginalSetId,
                        principalTable: "VocaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VocaSetCopies_VocaSets_VocaSetId",
                        column: x => x.VocaSetId,
                        principalTable: "VocaSets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VocaItemHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId1 = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VocaItemId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VocaItemId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocaItemHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VocaItemHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VocaItemHistories_AspNetUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VocaItemHistories_VocaItems_VocaItemId",
                        column: x => x.VocaItemId,
                        principalTable: "VocaItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VocaItemHistories_VocaItems_VocaItemId1",
                        column: x => x.VocaItemId1,
                        principalTable: "VocaItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId1",
                table: "AuditLogs",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_UserId",
                table: "Folders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_UserId1",
                table: "Folders",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IDX_Reminder_ReviewDate",
                table: "ReviewReminders",
                column: "ReviewDate");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReminders_UserId",
                table: "ReviewReminders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReminders_UserId1",
                table: "ReviewReminders",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReminders_VocaSetId",
                table: "ReviewReminders",
                column: "VocaSetId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReminders_VocaSetId1",
                table: "ReviewReminders",
                column: "VocaSetId1");

            migrationBuilder.CreateIndex(
                name: "IX_VocaItemHistories_UserId",
                table: "VocaItemHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VocaItemHistories_UserId1",
                table: "VocaItemHistories",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_VocaItemHistories_VocaItemId",
                table: "VocaItemHistories",
                column: "VocaItemId");

            migrationBuilder.CreateIndex(
                name: "IX_VocaItemHistories_VocaItemId1",
                table: "VocaItemHistories",
                column: "VocaItemId1");

            migrationBuilder.CreateIndex(
                name: "IDX_VocaItem_VocaSetId",
                table: "VocaItems",
                column: "VocaSetId");

            migrationBuilder.CreateIndex(
                name: "IX_VocaItems_VocaSetId1",
                table: "VocaItems",
                column: "VocaSetId1");

            migrationBuilder.CreateIndex(
                name: "IX_VocaSetCopies_CopiedByUserId",
                table: "VocaSetCopies",
                column: "CopiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VocaSetCopies_CopiedByUserId1",
                table: "VocaSetCopies",
                column: "CopiedByUserId1");

            migrationBuilder.CreateIndex(
                name: "IX_VocaSetCopies_OriginalSetId",
                table: "VocaSetCopies",
                column: "OriginalSetId");

            migrationBuilder.CreateIndex(
                name: "IX_VocaSetCopies_VocaSetId",
                table: "VocaSetCopies",
                column: "VocaSetId");

            migrationBuilder.CreateIndex(
                name: "IDX_VocaSet_UserId",
                table: "VocaSets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VocaSets_FolderId",
                table: "VocaSets",
                column: "FolderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ReviewReminders");

            migrationBuilder.DropTable(
                name: "VocaItemHistories");

            migrationBuilder.DropTable(
                name: "VocaSetCopies");

            migrationBuilder.DropTable(
                name: "VocaItems");

            migrationBuilder.DropTable(
                name: "VocaSets");

            migrationBuilder.DropTable(
                name: "Folders");
        }
    }
}
