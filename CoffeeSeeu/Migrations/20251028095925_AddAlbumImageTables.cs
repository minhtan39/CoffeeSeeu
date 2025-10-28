using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoffeeSeeu.Migrations
{
    /// <inheritdoc />
    public partial class AddAlbumImageTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlbumId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Albums",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Không gian ấm cúng, gần gũi", "Không gian quán" },
                    { 2, "Những người pha chế tài năng", "Đội ngũ" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEE1mypSXN//WoysjrNJi91SZI+YbcgDmV5HeRSGMhZyskFoPKPWRC2JPzJ4Ovr+tdw==");

            migrationBuilder.InsertData(
                table: "Images",
                columns: new[] { "Id", "AlbumId", "Description", "ImagePath" },
                values: new object[,]
                {
                    { 1, 1, "Góc chill tầng 1", "/img/shop-1.jpg" },
                    { 2, 1, "Không gian sân thượng", "/img/shop-2.jpg" },
                    { 3, 1, "Không gian đọc sách", "/img/shop-3.jpg" },
                    { 4, 2, "Minh Anh", "/img/staff-1.jpg" },
                    { 5, 2, "Tuấn Kiệt", "/img/staff-2.jpg" },
                    { 6, 2, "Hồng Nhung", "/img/staff-3.jpg" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Images_AlbumId",
                table: "Images",
                column: "AlbumId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "Albums");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEMX+ZJhJQ5sT4LgO5hPm1iS+MT6zu8j4BglBZ8aIYjN+CniqqqCdMSmfXsBc2rwIhA==");
        }
    }
}
