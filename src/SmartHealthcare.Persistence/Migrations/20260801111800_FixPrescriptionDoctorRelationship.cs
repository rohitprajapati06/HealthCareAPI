using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHealthcare.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixPrescriptionDoctorRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_DoctorProfiles_DoctorProfileId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_DoctorProfileId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "DoctorProfileId",
                table: "Prescriptions");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DoctorId",
                table: "Prescriptions",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_DoctorProfiles_DoctorId",
                table: "Prescriptions",
                column: "DoctorId",
                principalTable: "DoctorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_DoctorProfiles_DoctorId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_DoctorId",
                table: "Prescriptions");

            migrationBuilder.AddColumn<Guid>(
                name: "DoctorProfileId",
                table: "Prescriptions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DoctorProfileId",
                table: "Prescriptions",
                column: "DoctorProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_DoctorProfiles_DoctorProfileId",
                table: "Prescriptions",
                column: "DoctorProfileId",
                principalTable: "DoctorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
