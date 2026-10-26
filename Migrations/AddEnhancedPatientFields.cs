using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxeSmilesDental.Migrations
{
    /// <inheritdoc />
    public partial class AddEnhancedPatientFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter existing columns to be nullable
            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Patients",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ContactNumber",
                table: "Patients",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                table: "Patients",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Patients",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            // Add new basic information columns
            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "Patients",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Nickname",
                table: "Patients",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Sex",
                table: "Patients",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Religion",
                table: "Patients",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "Patients",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "HealthCardNumber",
                table: "Patients",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "Patients",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Company",
                table: "Patients",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AssignedDentist",
                table: "Patients",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // For minors
            migrationBuilder.AddColumn<string>(
                name: "ParentGuardianName",
                table: "Patients",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ParentGuardianOccupation",
                table: "Patients",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // Referral
            migrationBuilder.AddColumn<string>(
                name: "ReferredBy",
                table: "Patients",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ReasonForConsultation",
                table: "Patients",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // Dental History
            migrationBuilder.AddColumn<string>(
                name: "PreviousDentist",
                table: "Patients",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PreviousDentistSpecialty",
                table: "Patients",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDentalVisit",
                table: "Patients",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastDentalProcedure",
                table: "Patients",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // Medical History - Physician
            migrationBuilder.AddColumn<string>(
                name: "PhysicianName",
                table: "Patients",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PhysicianSpecialty",
                table: "Patients",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PhysicianOfficeNumber",
                table: "Patients",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // Medical Questions
            migrationBuilder.AddColumn<bool>(
                name: "InGoodHealth",
                table: "Patients",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UnderMedicalTreatment",
                table: "Patients",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalTreatmentDetails",
                table: "Patients",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "HadSeriousIllness",
                table: "Patients",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeriousIllnessDetails",
                table: "Patients",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "BeenHospitalized",
                table: "Patients",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HospitalizationDetails",
                table: "Patients",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "TakingMedications",
                table: "Patients",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentMedications",
                table: "Patients",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "SmokingHistory",
                table: "Patients",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AlcoholDrugUse",
                table: "Patients",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasAllergies",
                table: "Patients",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AllergyDetails",
                table: "Patients",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BleedingTime",
                table: "Patients",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // For Women Only
            migrationBuilder.AddColumn<bool>(
                name: "IsPregnant",
                table: "Patients",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNursing",
                table: "Patients",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TakingBirthControl",
                table: "Patients",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BloodType",
                table: "Patients",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BloodPressure",
                table: "Patients",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // Medical Conditions
            migrationBuilder.AddColumn<bool>(
                name: "HighBloodPressure",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LowBloodPressure",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HeartDisease",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HeartSurgeryAttack",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ChestPain",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Stroke",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LiverDisease",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "KidneyDisease",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ThyroidProblems",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LungDisorder",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GITDisease",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Tuberculosis",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BloodDyscrasias",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BleedingTendencies",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HeadInjuries",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ArthritisRheumatism",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CancerTumors",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RadiationChemotherapy",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "InfectiveDisease",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExtremeAnxiety",
                table: "Patients",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            // Family History
            migrationBuilder.AddColumn<string>(
                name: "PaternalHistory",
                table: "Patients",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MaternalHistory",
                table: "Patients",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove all added columns
            migrationBuilder.DropColumn(name: "MiddleName", table: "Patients");
            migrationBuilder.DropColumn(name: "Nickname", table: "Patients");
            migrationBuilder.DropColumn(name: "Sex", table: "Patients");
            migrationBuilder.DropColumn(name: "Religion", table: "Patients");
            migrationBuilder.DropColumn(name: "Nationality", table: "Patients");
            migrationBuilder.DropColumn(name: "HealthCardNumber", table: "Patients");
            migrationBuilder.DropColumn(name: "Occupation", table: "Patients");
            migrationBuilder.DropColumn(name: "Company", table: "Patients");
            migrationBuilder.DropColumn(name: "AssignedDentist", table: "Patients");
            migrationBuilder.DropColumn(name: "ParentGuardianName", table: "Patients");
            migrationBuilder.DropColumn(name: "ParentGuardianOccupation", table: "Patients");
            migrationBuilder.DropColumn(name: "ReferredBy", table: "Patients");
            migrationBuilder.DropColumn(name: "ReasonForConsultation", table: "Patients");
            migrationBuilder.DropColumn(name: "PreviousDentist", table: "Patients");
            migrationBuilder.DropColumn(name: "PreviousDentistSpecialty", table: "Patients");
            migrationBuilder.DropColumn(name: "LastDentalVisit", table: "Patients");
            migrationBuilder.DropColumn(name: "LastDentalProcedure", table: "Patients");
            migrationBuilder.DropColumn(name: "PhysicianName", table: "Patients");
            migrationBuilder.DropColumn(name: "PhysicianSpecialty", table: "Patients");
            migrationBuilder.DropColumn(name: "PhysicianOfficeNumber", table: "Patients");
            migrationBuilder.DropColumn(name: "InGoodHealth", table: "Patients");
            migrationBuilder.DropColumn(name: "UnderMedicalTreatment", table: "Patients");
            migrationBuilder.DropColumn(name: "MedicalTreatmentDetails", table: "Patients");
            migrationBuilder.DropColumn(name: "HadSeriousIllness", table: "Patients");
            migrationBuilder.DropColumn(name: "SeriousIllnessDetails", table: "Patients");
            migrationBuilder.DropColumn(name: "BeenHospitalized", table: "Patients");
            migrationBuilder.DropColumn(name: "HospitalizationDetails", table: "Patients");
            migrationBuilder.DropColumn(name: "TakingMedications", table: "Patients");
            migrationBuilder.DropColumn(name: "CurrentMedications", table: "Patients");
            migrationBuilder.DropColumn(name: "SmokingHistory", table: "Patients");
            migrationBuilder.DropColumn(name: "AlcoholDrugUse", table: "Patients");
            migrationBuilder.DropColumn(name: "HasAllergies", table: "Patients");
            migrationBuilder.DropColumn(name: "AllergyDetails", table: "Patients");
            migrationBuilder.DropColumn(name: "BleedingTime", table: "Patients");
            migrationBuilder.DropColumn(name: "IsPregnant", table: "Patients");
            migrationBuilder.DropColumn(name: "IsNursing", table: "Patients");
            migrationBuilder.DropColumn(name: "TakingBirthControl", table: "Patients");
            migrationBuilder.DropColumn(name: "BloodType", table: "Patients");
            migrationBuilder.DropColumn(name: "BloodPressure", table: "Patients");
            migrationBuilder.DropColumn(name: "HighBloodPressure", table: "Patients");
            migrationBuilder.DropColumn(name: "LowBloodPressure", table: "Patients");
            migrationBuilder.DropColumn(name: "HeartDisease", table: "Patients");
            migrationBuilder.DropColumn(name: "HeartSurgeryAttack", table: "Patients");
            migrationBuilder.DropColumn(name: "ChestPain", table: "Patients");
            migrationBuilder.DropColumn(name: "Stroke", table: "Patients");
            migrationBuilder.DropColumn(name: "LiverDisease", table: "Patients");
            migrationBuilder.DropColumn(name: "KidneyDisease", table: "Patients");
            migrationBuilder.DropColumn(name: "ThyroidProblems", table: "Patients");
            migrationBuilder.DropColumn(name: "LungDisorder", table: "Patients");
            migrationBuilder.DropColumn(name: "GITDisease", table: "Patients");
            migrationBuilder.DropColumn(name: "Tuberculosis", table: "Patients");
            migrationBuilder.DropColumn(name: "BloodDyscrasias", table: "Patients");
            migrationBuilder.DropColumn(name: "BleedingTendencies", table: "Patients");
            migrationBuilder.DropColumn(name: "HeadInjuries", table: "Patients");
            migrationBuilder.DropColumn(name: "ArthritisRheumatism", table: "Patients");
            migrationBuilder.DropColumn(name: "CancerTumors", table: "Patients");
            migrationBuilder.DropColumn(name: "RadiationChemotherapy", table: "Patients");
            migrationBuilder.DropColumn(name: "InfectiveDisease", table: "Patients");
            migrationBuilder.DropColumn(name: "ExtremeAnxiety", table: "Patients");
            migrationBuilder.DropColumn(name: "PaternalHistory", table: "Patients");
            migrationBuilder.DropColumn(name: "MaternalHistory", table: "Patients");

            // Revert original column changes (simplified for brevity)
        }
    }
}