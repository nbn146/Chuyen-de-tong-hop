using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAIChatRagCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIChatSessions",
                columns: table => new
                {
                    SessionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    GuestTokenHash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIChatSessions", x => x.SessionID);
                    table.CheckConstraint("CK_AIChatSessions_Owner", "([UserID] IS NOT NULL AND [GuestTokenHash] IS NULL) OR ([UserID] IS NULL AND [GuestTokenHash] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_AIChatSessions_Users",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "AIIndexJobs",
                columns: table => new
                {
                    JobID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    NextAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIIndexJobs", x => x.JobID);
                    table.CheckConstraint("CK_AIIndexJobs_Attempts", "[Attempts] >= 0");
                    table.ForeignKey(
                        name: "FK_AIIndexJobs_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                });

            migrationBuilder.CreateTable(
                name: "AIKnowledgeChunks",
                columns: table => new
                {
                    ChunkID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    ChunkOrdinal = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentHash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    EmbeddingModel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IndexedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIKnowledgeChunks", x => x.ChunkID);
                    table.ForeignKey(
                        name: "FK_AIKnowledgeChunks_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                });

            migrationBuilder.CreateTable(
                name: "AIChatMessages",
                columns: table => new
                {
                    MessageID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnswerType = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    ModelName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIChatMessages", x => x.MessageID);
                    table.ForeignKey(
                        name: "FK_AIChatMessages_Sessions",
                        column: x => x.SessionID,
                        principalTable: "AIChatSessions",
                        principalColumn: "SessionID");
                });

            migrationBuilder.CreateTable(
                name: "AIMessageSources",
                columns: table => new
                {
                    SourceID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageID = table.Column<long>(type: "bigint", nullable: false),
                    ChunkID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    SourceVersion = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIMessageSources", x => x.SourceID);
                    table.ForeignKey(
                        name: "FK_AIMessageSources_Chunks",
                        column: x => x.ChunkID,
                        principalTable: "AIKnowledgeChunks",
                        principalColumn: "ChunkID");
                    table.ForeignKey(
                        name: "FK_AIMessageSources_Messages",
                        column: x => x.MessageID,
                        principalTable: "AIChatMessages",
                        principalColumn: "MessageID");
                    table.ForeignKey(
                        name: "FK_AIMessageSources_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIChatMessages_SessionID_CreatedAt",
                table: "AIChatMessages",
                columns: new[] { "SessionID", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AIChatSessions_ExpiresAt",
                table: "AIChatSessions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_AIChatSessions_UserID_UpdatedAt",
                table: "AIChatSessions",
                columns: new[] { "UserID", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AIIndexJobs_ProductID_Status",
                table: "AIIndexJobs",
                columns: new[] { "ProductID", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AIIndexJobs_Status_NextAttemptAt_CreatedAt",
                table: "AIIndexJobs",
                columns: new[] { "Status", "NextAttemptAt", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AIKnowledgeChunks_ProductID_ChunkOrdinal",
                table: "AIKnowledgeChunks",
                columns: new[] { "ProductID", "ChunkOrdinal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIKnowledgeChunks_Status_IndexedAt",
                table: "AIKnowledgeChunks",
                columns: new[] { "Status", "IndexedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AIMessageSources_ChunkID",
                table: "AIMessageSources",
                column: "ChunkID");

            migrationBuilder.CreateIndex(
                name: "IX_AIMessageSources_MessageID_Rank",
                table: "AIMessageSources",
                columns: new[] { "MessageID", "Rank" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIMessageSources_ProductID",
                table: "AIMessageSources",
                column: "ProductID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIIndexJobs");

            migrationBuilder.DropTable(
                name: "AIMessageSources");

            migrationBuilder.DropTable(
                name: "AIKnowledgeChunks");

            migrationBuilder.DropTable(
                name: "AIChatMessages");

            migrationBuilder.DropTable(
                name: "AIChatSessions");
        }
    }
}
