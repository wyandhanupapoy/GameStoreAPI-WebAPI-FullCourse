using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameStore.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixInfinityDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Games\" SET \"CreatedAt\" = NOW(), \"UpdatedAt\" = NOW() WHERE \"CreatedAt\" = '-infinity';");
            migrationBuilder.Sql("UPDATE \"Genres\" SET \"CreatedAt\" = NOW(), \"UpdatedAt\" = NOW() WHERE \"CreatedAt\" = '-infinity';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
