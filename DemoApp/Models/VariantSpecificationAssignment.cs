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
/// VariantSpecificationAssignment class.
/// </summary>
public class VariantSpecificationAssignment
{
    /// <summary>
    /// Gets or sets the ID of the variant specification assignment.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the variable code.
    /// </summary>
    [MaxLength(15)]
    public string VariableCode { get; set; }

    /// <summary>
    /// Gets or sets the value code.
    /// </summary>
    [MaxLength(8)]
    public string ValueCode { get; set; }

    /// <summary>
    /// Gets or sets the variant specification ID.
    /// </summary>
    [JsonIgnore]
    public Guid VariantSpecificationId { get; set; }

    /// <summary>
    /// Creates a deep clone of this assignment with all Ids set to Guid.Empty.
    /// </summary>
    public VariantSpecificationAssignment Clone()
    {
        return new VariantSpecificationAssignment
        {
            Id = Guid.Empty,
            VariableCode = this.VariableCode,
            ValueCode = this.ValueCode,
            VariantSpecificationId = Guid.Empty
        };
    }
}