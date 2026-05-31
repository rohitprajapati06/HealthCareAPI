using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHealthcare.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHospitalEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Specialiization",
                table: "DoctorProfiles",
                newName: "Specialization");

            migrationBuilder.RenameColumn(
                name: "ConsultantionFee",
                table: "DoctorProfiles",
                newName: "ConsultationFee");

            migrationBuilder.AlterColumn<string>(
                name: "ContactPhone",
                table: "Hospitals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ContactEmail",
                table: "Hospitals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Hospitals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RohiniCode",
                table: "Hospitals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Hospitals");

            migrationBuilder.DropColumn(
                name: "RohiniCode",
                table: "Hospitals");

            migrationBuilder.RenameColumn(
                name: "Specialization",
                table: "DoctorProfiles",
                newName: "Specialiization");

            migrationBuilder.RenameColumn(
                name: "ConsultationFee",
                table: "DoctorProfiles",
                newName: "ConsultantionFee");

            migrationBuilder.AlterColumn<string>(
                name: "ContactPhone",
                table: "Hospitals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContactEmail",
                table: "Hospitals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
