using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DemoApp.Models;

/// <summary>
/// Represents a test object order.
/// </summary>
public class TestObjectOrder
{
    /// <summary>
    /// Gets or sets the ID of the test object order.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the test object order ID.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    [MinLength(8, ErrorMessage = "The length of this property must be at least 8 characters long!")]
    [MaxLength(8)]
    [Regex(@"^[a-zA-Z0-9]*$", ErrorMessage = "Erroneous format for TestObjectOrderId!")]
    [Regex(@"^\S*$", ErrorMessage = "No white space allowed!")]
    public string TestObjectOrderId { get; set; }

    /// <summary>
    /// Gets or sets the test object ID from Core.
    /// </summary>
    [JsonIgnore]
    public Guid CoreId { get; set; }

    /// <summary>
    /// Gets or sets the test object name.
    /// </summary>
    public string TestObjectName { get; set; }

    /// <summary>
    /// Gets or sets the test object type.
    /// </summary>
    public string TestObjectType { get; set; }

    /// <summary>
    /// Gets or sets the FYON.
    /// </summary>
    [MaxLength(10, ErrorMessage = "The length of this property must be between 9 and 10 characters!")]
    [Regex(@"^\S*$", ErrorMessage = "No white space allowed!")]
#nullable enable
    public string? Fyon { get; set; }
#nullable disable

    /// <summary>
    /// Gets or sets the VIN.
    /// </summary>
    [Regex(@"^\S*$", ErrorMessage = "No white space allowed!")]
    [MaxLength(20)]
    public string Vin { get; set; }

    /// <summary>
    /// Gets or sets the year model.
    /// </summary>
    [Range(1900, 9999, ErrorMessage = "The value of this property must be between 1900 and 9999!")]
    public int? YearModel { get; set; }

    /// <summary>
    /// Gets or sets the PNO12.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    [Length(12, ErrorMessage = "The length of this property must be exactly 12 characters!")]
    [MaxLength(12)]
    [Regex(@"^\S*$", ErrorMessage = "No white space allowed!")]
    public string Pno12 { get; set; }

    /// <summary>
    /// Gets or sets the build plant.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    [MinLength(2, ErrorMessage = "The length of this property must be between 2 and 3 characters!")]
    [MaxLength(3, ErrorMessage = "The length of this property must be between 2 and 3 characters!")]
    [Regex(@"(\d+)", ErrorMessage = "Erroneous format for BuildPlant!")]
    [Regex(@"^\S*$", ErrorMessage = "No white space allowed!")]
    public string BuildPlant { get; set; }

    /// <summary>
    /// Gets or sets the project.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    [MaxLength(4)]
    public string Project { get; set; }

    /// <summary>
    /// Gets or sets the series.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    [MaxLength(6)]
    public string Series { get; set; }

    /// <summary>
    /// Gets or sets the structure week.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    [Length(5, ErrorMessage = "The length of this property must be exactly 5 characters long!")]
    [MaxLength(5)]
    [Regex(@"^(\d{2,2})[w]((^\d|([0-4]\d)|(5[0123])))$", ErrorMessage = "Erroneous format for StructureWeek! Allowed examples: 21w01 or 21w34.")]
    [Regex(@"^\S*$", ErrorMessage = "No white space allowed!")]
    public string StructureWeek { get; set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    public string Status { get; set; }

    /// <summary>
    /// Gets or sets the exterior.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    [MaxLength(5)]
    public string Exterior { get; set; }

    /// <summary>
    /// Gets or sets the interior.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    [MaxLength(5)]
    public string Interior { get; set; }

    /// <summary>
    /// Gets or sets the order description.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    public string OrderDescription { get; set; }

    /// <summary>
    /// Gets or sets the part specifications.
    /// </summary>
    [JsonIgnore]
    public List<PartSpecification> PartSpecifications { get; set; }

    /// <summary>
    /// Gets or sets the variant specification SPA2.
    /// </summary>
    [JsonIgnore]
    public VariantSpecificationSpa2 VariantSpecificationSpa2 { get; set; }

    /// <summary>
    /// Gets or sets the last update of part specifications.
    /// </summary>
    public DateTime? LastUpdatePartSpecifications { get; protected set; }

    /// <summary>
    /// Gets or sets the build date.
    /// </summary>
    public DateTime? BuildDate { get; set; }

    /// <summary>
    /// Gets or sets the change logs.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<ChangeLog> ChangeLogs { get; set; }

#pragma warning disable CS8632
    /// <summary>
    /// Gets or sets the broadcast content versions.
    /// </summary>
    [JsonIgnore]
    public List<BroadcastContentVersion> BroadcastContentVersions { get; set; }
#pragma warning restore CS8632

    /// <summary>
    /// Gets the mix number formatted as a string.
    /// Note: MIXNO &amp; SEQNO will be the same starting from 200 000
    ///       The uniqueness of the numbers are of importance
    /// </summary>
    [NotMapped]
    [Regex(@"^\d+$", ErrorMessage = "Value must be an integer!")]
    public string MixNumber => MixNum.ToString().PadLeft(7, '0');

    /// <summary>
    /// Gets or sets the mix number.
    /// </summary>
    [Column]
    [JsonIgnore]
    public int? MixNum { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the test object order is complete car.
    /// </summary>
    public bool? CompleteCar { get; set; }

    /// <summary>
    /// Legacy variant specification versions.
    /// </summary>
    public List<LegacyVariantSpecificationVersion> LegacyVariantSpecificationVersions { get; set; }

    /// <summary>
    /// Variant specification versions.
    /// </summary>
    public List<VariantSpecificationVersion> VariantSpecificationVersions { get; set; }

    /// <summary>
    /// Sets the last update of part specifications.
    /// </summary>
    public List<SoftwarePartVersion> SoftwarePartVersions { get; set; }

    /// <summary>
    /// Sets the last update of part specifications.
    /// </summary>
    /// <param name="date">The last update date.</param>
    public void SetLastUpdatePartSpecifications(DateTime date)
    {
        LastUpdatePartSpecifications = date;
    }

    /// <summary>
    /// Tire Pressure Front Left.
    /// </summary>
    [MaxLength(8)]
    public string FrontLeft { get; set; }

    /// <summary>
    /// Tire Pressure Front Right.
    /// </summary>
    [MaxLength(8)]
    public string FrontRight { get; set; }

    /// <summary>
    /// Tire Pressure Rear Left.
    /// </summary>
    [MaxLength(8)]
    public string RearLeft { get; set; }

    /// <summary>
    /// Tire Pressure Rear Right.
    /// </summary>
    [MaxLength(8)]
    public string RearRight { get; set; }

    /// <summary>
    /// Factory order number to integer.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException"></exception>
    public int FactoryOrderNumberToInteger()
    {
        if (string.IsNullOrEmpty(Fyon))
        {
            throw new ArgumentException("FactoryOrderNumber cannot be null or empty");
        }

        if (!int.TryParse(Fyon, out var parseFactoryOrderNumber))
        {
            throw new FormatException($"FactoryOrderNumber '{Fyon}' is not a valid integer.");
        }

        return parseFactoryOrderNumber;
    }

    /// <summary>
    /// Gets the used legacy variant specification version.
    /// </summary>
    /// <returns></returns>
    public List<LegacyVariantSpecification> UsedLegacyVariantSpecificationsOrDefault() =>
        LegacyVariantSpecificationVersions?.FirstOrDefault(v =>
                v.TestObjectOrderId == TestObjectOrderId)
            ?.LegacyVariantSpecifications;

    /// <summary>
    /// Gets the used variant specification version.
    /// </summary>
    public VariantSpecification.VariantSpecification UseVariantSpecificationOrDefault() =>
        VariantSpecificationVersions?.FirstOrDefault(v => v.Used)?.VariantSpecification;
}
