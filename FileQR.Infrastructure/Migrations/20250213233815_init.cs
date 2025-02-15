using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileQR.Infrastructure.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QRSettings",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShowArabicNames = table.Column<bool>(type: "bit", nullable: false),
                    QRShowAuthBy = table.Column<bool>(type: "bit", nullable: false),
                    QRShowIssueDate = table.Column<bool>(type: "bit", nullable: false),
                    QRShowIssuedFor = table.Column<bool>(type: "bit", nullable: false),
                    QRShowLink = table.Column<bool>(type: "bit", nullable: false),
                    QRLeft = table.Column<float>(type: "real", nullable: false),
                    QRBottom = table.Column<float>(type: "real", nullable: false),
                    QRWidth = table.Column<float>(type: "real", nullable: false),
                    MessageToShowInImage = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRSettings", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArabicDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QRSettingId = table.Column<int>(type: "int", nullable: true),
                    IssuedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthRequiredFrom = table.Column<int>(type: "int", nullable: true),
                    AuthRequiredFromUserId = table.Column<int>(type: "int", nullable: true),
                    ShowFirstPage = table.Column<bool>(type: "bit", nullable: false),
                    ShowLastPage = table.Column<bool>(type: "bit", nullable: false),
                    ShowMiddlePages = table.Column<bool>(type: "bit", nullable: false),
                    FirstPagePosLeft = table.Column<float>(type: "real", nullable: false),
                    FirstPagePosBottom = table.Column<float>(type: "real", nullable: false),
                    FirstPagePosWidth = table.Column<float>(type: "real", nullable: false),
                    LastPagePosLeft = table.Column<float>(type: "real", nullable: false),
                    LastPagePosBottom = table.Column<float>(type: "real", nullable: false),
                    LastPagePosWidth = table.Column<float>(type: "real", nullable: false),
                    MiddlePagePosLeft = table.Column<float>(type: "real", nullable: false),
                    MiddlePagePosBottom = table.Column<float>(type: "real", nullable: false),
                    MiddlePagePosWidth = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Files_QRSettings_QRSettingId",
                        column: x => x.QRSettingId,
                        principalTable: "QRSettings",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Files_Users_AuthRequiredFromUserId",
                        column: x => x.AuthRequiredFromUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_AuthRequiredFromUserId",
                table: "Files",
                column: "AuthRequiredFromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_QRSettingId",
                table: "Files",
                column: "QRSettingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "QRSettings");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
