using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp;
public enum VariantSpecificationType
{
    /// <summary>
    /// Represents a downloadable variant specification.
    /// </summary>
    Downloadable,

    /// <summary>
    /// Represents a standard variant specification.
    /// </summary>
    Standard,
}

/// <summary>
/// VariantSpecificationSource enumeration.
/// </summary>
public enum VariantSpecificationSource
{
    /// <summary>
    /// Represents a KDP variant specification.
    /// </summary>
    Kdp = 1,

    /// <summary>
    /// Represents an OVP variant specification.
    /// </summary>
    Ovp = 2,

    /// <summary>
    /// Represents a snapshot variant specification.
    /// </summary>
    Snapshot = 3,
}

/// <summary>
/// Extension methods for the VariantSpecificationType enumeration.
/// </summary>
public static class VariantSpecificationTypeExtensions
{
    /// <summary>
    /// Converts the variant specification type to a friendly string representation.
    /// </summary>
    /// <param name="type">The variant specification type.</param>
    /// <returns>A friendly string representation of the variant specification type.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the variant specification type is not recognized.</exception>
    public static string ToFriendlyString(this VariantSpecificationType type)
    {
        return type switch
        {
            VariantSpecificationType.Downloadable => "downloadable",
            VariantSpecificationType.Standard => "standard",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    /// <summary>
    /// Converts the variant specification type to a URI path.
    /// </summary>
    /// <param name="type">The variant specification type.</param>
    /// <returns>A URI path representing the variant specification type.</returns>
    public static string ToUriPath(this VariantSpecificationType type)
        => ToFriendlyString(type).ToLower();
}