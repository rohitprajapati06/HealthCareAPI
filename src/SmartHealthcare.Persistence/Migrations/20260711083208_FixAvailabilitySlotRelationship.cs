using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHealthcare.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixAvailabilitySlotRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AvailabilitySlots_DoctorProfiles_DoctorProfileId",
                table: "AvailabilitySlots");

            migrationBuilder.DropIndex(
                name: "IX_AvailabilitySlots_DoctorProfileId",
                table: "AvailabilitySlots");

            migrationBuilder.DropColumn(
                name: "DoctorProfileId",
                table: "AvailabilitySlots");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilitySlots_DoctorId",
                table: "AvailabilitySlots",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AvailabilitySlots_DoctorProfiles_DoctorId",
                table: "AvailabilitySlots",
                column: "DoctorId",
                principalTable: "DoctorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AvailabilitySlots_DoctorProfiles_DoctorId",
                table: "AvailabilitySlots");

            migrationBuilder.DropIndex(
                name: "IX_AvailabilitySlots_DoctorId",
                table: "AvailabilitySlots");

            migrationBuilder.AddColumn<Guid>(
                name: "DoctorProfileId",
                table: "AvailabilitySlots",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilitySlots_DoctorProfileId",
                table: "AvailabilitySlots",
                column: "DoctorProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_AvailabilitySlots_DoctorProfiles_DoctorProfileId",
                table: "AvailabilitySlots",
                column: "DoctorProfileId",
                principalTable: "DoctorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
