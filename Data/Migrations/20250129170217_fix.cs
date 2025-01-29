using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Pharmacies_PharmacyId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Patients_PatientId",
                table: "Purchases");

            migrationBuilder.AlterColumn<int>(
                name: "PatientId",
                table: "Purchases",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PharmacyId",
                table: "Inventories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Pharmacies_PharmacyId",
                table: "Inventories",
                column: "PharmacyId",
                principalTable: "Pharmacies",
                principalColumn: "PharmacyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Patients_PatientId",
                table: "Purchases",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Pharmacies_PharmacyId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Patients_PatientId",
                table: "Purchases");

            migrationBuilder.AlterColumn<int>(
                name: "PatientId",
                table: "Purchases",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PharmacyId",
                table: "Inventories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Pharmacies_PharmacyId",
                table: "Inventories",
                column: "PharmacyId",
                principalTable: "Pharmacies",
                principalColumn: "PharmacyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Patients_PatientId",
                table: "Purchases",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
