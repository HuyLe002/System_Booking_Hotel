using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBookingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Rooms",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Floor",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Rooms",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Floor", "ImageUrl" },
                values: new object[] { "Phòng tiêu chuẩn tầng 1, hướng sân vườn", 1, null });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Floor", "ImageUrl" },
                values: new object[] { "Phòng cao cấp tầng 2, có ban công", 2, null });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Floor", "ImageUrl" },
                values: new object[] { "Phòng suite tầng 3, nhìn toàn cảnh thành phố", 3, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Floor",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Rooms");
        }
    }
}
