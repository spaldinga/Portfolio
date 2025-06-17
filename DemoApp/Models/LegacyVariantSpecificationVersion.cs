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
/// Represents a legacy variant specification version.
/// </summary>
public class LegacyVariantSpecificationVersion
{
    /// <summary>
    /// Gets or sets the unique identifier of the legacy variant specification version.
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
    /// Gets or sets the user associated with the legacy variant specification version.
    /// </summary>
    [MaxLength(64)]
    public string User { get; set; }

    /// <summary>
    /// Gets or sets the description of the legacy variant specification version.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    [MaxLength(500)]
    public string Description { get; set; } = "Initial Version";

    /// <summary>
    /// Gets or sets the type of the legacy variant specification version.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    public LegacyVariantSpecificationType Type { get; set; } = LegacyVariantSpecificationType.Used;

    /// <summary>
    /// Gets or sets the structure week.
    /// </summary>
    [Length(6, ErrorMessage = "The length of this property must be exactly 6 characters long!")]
    [MaxLength(6)]
    [Regex(@"^\S*$", ErrorMessage = "No white space allowed!")]
    public string StructureWeek { get; set; }

    /// <summary>
    /// Gets or sets the version of the legacy variant specification version.
    /// </summary>
    [MaxLength(15)]
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the creation date and time of the legacy variant specification version.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the list of legacy variant specifications associated with the legacy variant specification version.
    /// </summary>
    public List<LegacyVariantSpecification> LegacyVariantSpecifications { get; set; }

    /// <summary>
    /// Gets or sets the test object order associated with the legacy variant specification version.
    /// </summary>
    [JsonIgnore]
    public TestObjectOrder TestObjectOrder { get; set; }
}