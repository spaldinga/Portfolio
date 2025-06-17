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
/// VariantSpecificationVersion class.
/// </summary>
public class VariantSpecificationVersion
{
    // Version
    /// <summary>
    /// Gets or sets the ID of the variant specification version.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user who created the variant specification version.
    /// </summary>
    [MaxLength(64)]
    public string User { get; set; }

    /// <summary>
    /// Gets or sets the description of the variant specification version.
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = "Initial Version";

    /// <summary>
    /// Gets or sets the date and time when the variant specification version was created.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the structure week.
    /// </summary>
    [Length(6, ErrorMessage = "The length of this property must be exactly 6 characters long!")]
    [MaxLength(6)]
    [Regex(@"^\S*$", ErrorMessage = "No white space allowed!")]
    public string StructureWeek { get; set; }

    /// <summary>
    /// Gets or sets the version number of the variant specification version.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the source of the variant specification version.
    /// </summary>
    public VariantSpecificationSource Source { get; set; } = VariantSpecificationSource.Kdp;

    /// <summary>
    /// Gets or sets a value indicating whether the variant specification version is used.
    /// </summary>
    public bool Used { get; set; }

    /// <summary>
    /// Gets or sets the variant specification associated with the version.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    public VariantSpecification VariantSpecification { get; set; }

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
    /// Gets or sets the test object order associated with the variant specification version.
    /// </summary>
    [JsonIgnore]
    public TestObjectOrder TestObjectOrder { get; set; }

    /// <summary>
    /// Summary of the variant specification version.
    /// </summary>
    [JsonIgnore]
    public string Summary => $"Version: {Version}, Created On: {CreatedOn.Date}, Description: {Description}";

    /// <summary>
    /// Gets or sets the Parent Legacy Variant Version.
    /// </summary>
    [JsonIgnore]
    public LegacyVariantSpecificationVersion ParentLegacyVariantVersion { get; set; }

    /// <summary>
    /// Gets or sets the Parent Variant Specification Version.
    /// </summary>
    [JsonIgnore]
    public VariantSpecificationVersion ParentVariantSpecificationVersionGuid { get; set; }

    /// <summary>
    /// Gets the concatenated string representation of the version number and its description,
    /// formatted as "Version - Description".
    /// </summary>
    public string VersionWithDescription => $"{Version} - {Description}";

    /// <summary>
    /// Determines if the VariantSpecification contains a specific assignment with
    /// VariableCode "C5X1" and ValueCode "0AW".
    /// </summary>
    /// <returns>
    /// Returns <c>true</c> if such an assignment exists, <c>false</c> if it does not,
    /// and <c>null</c> if the VariantSpecification or its assignments are <c>null</c>.
    /// </returns>
    public bool? IsVcuV2() => VariantSpecification
        ?.VariantSpecificationAssignments
        ?.Any(x => x.VariableCode == "C5X1" && x.ValueCode == "0AW");

    /// <summary>
    /// Creates a deep clone of this VariantSpecificationVersion with all Ids set to Guid.Empty and navigation properties set to null.
    /// </summary>
    public VariantSpecificationVersion Clone()
    {
        return new VariantSpecificationVersion
        {
            Id = Guid.Empty,
            CreatedOn = DateTime.Now,
            StructureWeek = this.StructureWeek,
            Source = this.Source,
            Used = false,
            VariantSpecification = this.VariantSpecification?.Clone(),
            TestObjectOrderId = this.TestObjectOrderId,
            TestObjectOrder = null,
            ParentLegacyVariantVersion = this.ParentLegacyVariantVersion,
            ParentVariantSpecificationVersionGuid = this,
        };
    }
}
