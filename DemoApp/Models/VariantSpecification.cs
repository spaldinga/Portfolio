using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DemoApp.Models;
/// <summary>
/// VariantSpecification class.
/// </summary>
public class VariantSpecification
{
    /// <summary>
    /// Converts the KdpVariantSpecification object to VariantSpecification.
    /// </summary>
    /// <param name="kdpVariantSpecification">The KdpVariantSpecification object.</param>
    /// <returns>The converted VariantSpecification object.</returns>
    public static VariantSpecification VariantSpecificationFactory(KdpVariantSpecification kdpVariantSpecification)
        => new()
        {
            MaturityState = kdpVariantSpecification.MaturityState,
            ConfiguredOn = kdpVariantSpecification.DynamicVehicleData?.ConfiguredOn,
            ConfigurationDate = kdpVariantSpecification.DynamicVehicleData?.ConfigurationDate,
            IsComplete = kdpVariantSpecification.IsComplete,
            IsValid = kdpVariantSpecification.IsValid,
            VariantSpecificationAssignments =
                TranslateToVariantSpecificationAssignments(kdpVariantSpecification.StandardConfiguration?.Assignments)
        };

    /// <summary>
    /// Converts the PrinsVariantSpecification object to VariantSpecification.
    /// </summary>
    /// <param name="prinsVariantSpecification"></param>
    /// <returns></returns>
    public static VariantSpecification VariantSpecificationFactory(PrinsVariantSpecification prinsVariantSpecification)
        => new()
        {
            PackageIdentifier = string.Empty,
            ConsumerSoftwareVersion = string.Empty,
            VcdSpecNumber = string.Empty,
            VcdSpecIssue = string.Empty,
            MaturityState = prinsVariantSpecification.BasedOnMaturityState,
            ConfiguredOn = prinsVariantSpecification.ConfiguredOn,
            ConfigurationDate = prinsVariantSpecification.ConfigurationDate,
            IsComplete = prinsVariantSpecification.IsComplete,
            IsValid = prinsVariantSpecification.IsValid,
            VariantSpecificationAssignments = prinsVariantSpecification.Assignments
                .SelectMany(assignment => assignment.Value ?? Enumerable.Empty<string>(),
                    (assignment, value) => new VariantSpecificationAssignment
                    {
                        VariableCode = assignment.Key,
                        ValueCode = value
                    })
                .ToList()
        };

    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the code.
    /// </summary>
    [MaxLength(8)]
    [JsonPropertyName("code")]
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the configuration type.
    /// </summary>
    [MaxLength(8)]
    [JsonPropertyName("configurationType")]
    public string ConfigurationType { get; set; }

    /// <summary>
    /// Gets or sets the configuration ID.
    /// </summary>
    [MaxLength(50)]
    [JsonPropertyName("configurationId")]
    public string ConfigurationId { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [MaxLength(50)]
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the object is deleted.
    /// </summary>
    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the maturity state.
    /// </summary>
    [MaxLength(15)]
    [JsonPropertyName("maturityState")]
    public string MaturityState { get; set; }

    /// <summary>
    /// Gets or sets the variability model code.
    /// </summary>
    [MaxLength(4)]
    [JsonPropertyName("variabilityModelCode")]
    public string VariabilityModelCode { get; set; }

    /// <summary>
    /// Gets or sets the variability model state.
    /// </summary>
    [MaxLength(15)]
    [JsonPropertyName("variabilityModelState")]
    public string VariabilityModelState { get; set; }

    /// <summary>
    /// Gets or sets the configuration date.
    /// </summary>
    [MaxLength(26)]
    [JsonPropertyName("configurationDate")]
    public string ConfigurationDate { get; set; }

    /// <summary>
    /// Gets or sets the configured on date.
    /// </summary>
    [MaxLength(26)]
    [JsonPropertyName("configuredOn")]
    public string ConfiguredOn { get; set; }

    /// <summary>
    /// Gets or sets the package identifier.
    /// </summary>
    [JsonPropertyName("packageIdentifier")]
    public string PackageIdentifier { get; set; } = "00000000000000000001";

    /// <summary>
    /// Gets or sets the consumer software version.
    /// </summary>
    [JsonPropertyName("consumerSoftwareVersion")]
    public string ConsumerSoftwareVersion { get; set; } = "00.00.000";

    /// <summary>
    /// Gets or sets the VCD spec number.
    /// </summary>
    [JsonPropertyName("vcdSpecNumber")]
    public string VcdSpecNumber { get; set; } = "33171622";

    /// <summary>
    /// Gets or sets the VCD spec issue.
    /// </summary>
    [JsonPropertyName("vcdSpecIssue")]
    public string VcdSpecIssue { get; set; } = "011";

    /// <summary>
    /// Gets or sets a value indicating whether the object is complete.
    /// </summary>
    [JsonPropertyName("isComplete")]
    public bool IsComplete { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the object is valid.
    /// </summary>
    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the object is immobilizer.
    /// </summary>
    [JsonPropertyName("isImmobilizerEnabled")]
    public bool IsImmobilizerEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the variant specification assignments.
    /// </summary>
    [JsonPropertyName("variantSpecificationAssignments")]
    public List<VariantSpecificationAssignment> VariantSpecificationAssignments { get; set; }

    /// <summary>
    /// Gets or sets the variant specification version.
    /// </summary>
    [JsonIgnore]
    public VariantSpecificationVersion Version { get; set; }

    /// <summary>
    /// Gets or sets the variant specification version ID.
    /// </summary>
    public Guid VariantSpecificationVersionId { get; set; }

    /// <summary>
    /// Converts the VariantSpecification object to KdpVariantSpecification.
    /// </summary>
    /// <returns>The converted KdpVariantSpecification object.</returns>
    public KdpVariantSpecification ToKdpVariantSpecification()
        => new()
        {
            MaturityState = MaturityState,
            DynamicVehicleData = new KdpDynamicVehicleData
            {
                ConfiguredOn = ConfiguredOn,
                ConfigurationDate = ConfigurationDate
            },
            IsComplete = IsComplete,
            IsValid = IsValid,
            StandardConfiguration = new KdpConfiguration
            {
                Assignments = GetKdpAssignments(VariantSpecificationAssignments)
            }
        };

    /// <summary>
    /// Gets the KdpAssignments from the VariantSpecificationAssignments.
    /// </summary>
    /// <param name="variantSpecificationAssignments">The list of VariantSpecificationAssignments.</param>
    /// <returns>The list of KdpAssignments.</returns>
    private static List<KdpAssignment> GetKdpAssignments(
        List<VariantSpecificationAssignment> variantSpecificationAssignments)
    {
        if (variantSpecificationAssignments == null)
        {
            return new List<KdpAssignment>();
        }

        return variantSpecificationAssignments
            .GroupBy(x => x.VariableCode)
            .Select(group => new KdpAssignment
            {
                VariableCode = group.Key,
                ValueCode = group.Select(x => x.ValueCode).ToList()
            })
            .ToList();
    }

    /// <summary>
    /// Gets the VariantSpecificationAssignments from the KdpAssignments.
    /// </summary>
    /// <param name="kdpAssignments">The list of KdpAssignments.</param>
    /// <returns>The list of VariantSpecificationAssignments.</returns>
    private static List<VariantSpecificationAssignment> TranslateToVariantSpecificationAssignments(
        List<KdpAssignment> kdpAssignments)
    {
        if (kdpAssignments == null)
        {
            return new List<VariantSpecificationAssignment>();
        }

        return kdpAssignments
            .SelectMany(assignment => assignment.ValueCode ?? Enumerable.Empty<string>(),
                (assignment, valueCode) => new VariantSpecificationAssignment
                {
                    VariableCode = assignment.VariableCode,
                    ValueCode = valueCode
                })
            .ToList();
    }

    /// <summary>
    /// Translates the list of VariantSpecificationAssignments to a compact string representation.
    /// </summary>
    /// <returns>A string representing the compact assignments.</returns>
    public string GetCompactAssignments()
    {
        if (VariantSpecificationAssignments == null)
        {
            return "{}";
        }

        var groupedAssignments = VariantSpecificationAssignments
            .GroupBy(x => x.VariableCode)
            .ToDictionary(
                group => group.Key,
                group => group.Select(x => x.ValueCode).ToList()
            );

        return JsonSerializer.Serialize(groupedAssignments);
    }

    /// <summary>
    /// Creates a deep clone of this VariantSpecification with all Ids set to Guid.Empty.
    /// </summary>
    public VariantSpecification Clone()
    {
        var clone = new VariantSpecification
        {
            Id = Guid.Empty,
            Code = this.Code,
            ConfigurationType = this.ConfigurationType,
            ConfigurationId = this.ConfigurationId,
            Description = this.Description,
            IsDeleted = this.IsDeleted,
            MaturityState = this.MaturityState,
            VariabilityModelCode = this.VariabilityModelCode,
            VariabilityModelState = this.VariabilityModelState,
            ConfigurationDate = this.ConfigurationDate,
            ConfiguredOn = this.ConfiguredOn,
            PackageIdentifier = this.PackageIdentifier,
            ConsumerSoftwareVersion = this.ConsumerSoftwareVersion,
            VcdSpecNumber = this.VcdSpecNumber,
            VcdSpecIssue = this.VcdSpecIssue,
            IsComplete = this.IsComplete,
            IsValid = this.IsValid,
            IsImmobilizerEnabled = this.IsImmobilizerEnabled,
            VariantSpecificationVersionId = Guid.Empty,
            VariantSpecificationAssignments = this.VariantSpecificationAssignments?.Where(a => a != null)
                    .Select(a => a.Clone())
                    .ToList(),
            IncompleteVariableCodes = null
        };

        clone.Version = null;

        return clone;
    }
}

/// <summary>
/// Extension methods for the VariantSpecification class.
/// </summary>
public static class VariantSpecificationExtensions
{
#nullable enable
    /// <summary>
    /// Checks if the VariantSpecification or its assignments are null or empty.
    /// </summary>
    /// <param name="variantSpecification">The VariantSpecification object.</param>
    /// <returns>True if the VariantSpecification or its assignments are null or empty; otherwise, false.</returns>
    public static bool IsNullOrEmptyAssignments(this VariantSpecification? variantSpecification) =>
        variantSpecification?.VariantSpecificationAssignments == null || !variantSpecification.VariantSpecificationAssignments.Any();
#nullable disable
}
