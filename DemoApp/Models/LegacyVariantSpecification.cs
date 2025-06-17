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
/// Represents a legacy variant specification.
/// </summary>
public class LegacyVariantSpecification
{
    /// <summary>
    /// Gets or sets the ID of the legacy variant specification.
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
    /// Gets or sets the variant family.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    [Length(2, ErrorMessage = "The length of this property must be exactly 2 characters long!")]
    [MaxLength(2)]
    [Regex(@"^\S*$", ErrorMessage = "No white space allowed!")]
    public string VariantFamily { get; set; }

    /// <summary>
    /// Gets or sets the variant number.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    [Length(2, ErrorMessage = "The length of this property must be exactly 2 characters long!")]
    [MaxLength(2)]
    [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed!")]
    public string VariantNumber { get; set; }

    /// <summary>
    /// Gets or sets the variant designation.
    /// </summary>
    [Required(ErrorMessage = "Required property")]
    [MinLength(1, ErrorMessage = "The length of this property must be between 1 and 8 characters!")]
    [MaxLength(8)]
    public string VariantDesignation { get; set; }

    /// <summary>
    /// Gets or sets the variant specification version ID.
    /// </summary>
    [JsonIgnore]
    public Guid LegacyVariantSpecificationVersionId { get; set; }

    /// <summary>
    /// Gets or sets the variant specification version.
    /// </summary>
    [JsonIgnore]
    public LegacyVariantSpecificationVersion LegacyVariantSpecificationVersion { get; set; }
}
