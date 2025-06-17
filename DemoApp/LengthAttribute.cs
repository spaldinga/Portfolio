using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp;

/// <summary>
/// Specifies the maximum length of a string or collection.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class LengthAttribute : ValidationAttribute
{
    private readonly int _length;

    /// <summary>
    /// Initializes a new instance of the <see cref="LengthAttribute"/> class with the specified length.
    /// </summary>
    /// <param name="length">The maximum length allowed.</param>
    public LengthAttribute(int length)
    {
        _length = length;
    }

    /// <summary>
    /// Determines whether the specified value is valid.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns><c>true</c> if the value is valid; otherwise, <c>false</c>.</returns>
    public override bool IsValid(object value)
    {
        // Check the lengths for legality
        EnsureLegalLengths();

        int length;

        // Automatically pass if value is null. RequiredAttribute should be used to assert a value is not null.
        if (value == null)
        {
            return true;
        }

        if (value is string str)
        {
            length = str.Length;
        }
        else if (CountPropertyHelper.TryGetCount(value, out var count))
        {
            length = count;
        }
        else
        {
            throw new InvalidCastException("Invalid value type!");
        }

        return length == _length;
    }

    private void EnsureLegalLengths()
    {
        if (_length < 0)
        {
            throw new InvalidOperationException("No object provided!");
        }
    }
}

internal static class CountPropertyHelper
{
    /// <summary>
    /// Tries to get the count of the specified value.
    /// </summary>
    /// <param name="value">The value to get the count from.</param>
    /// <param name="count">When this method returns, contains the count of the value if it is a collection; otherwise, -1.</param>
    /// <returns><c>true</c> if the count was successfully retrieved; otherwise, <c>false</c>.</returns>
    public static bool TryGetCount(object value, out int count)
    {
        if (value is ICollection collection)
        {
            count = collection.Count;
            return true;
        }

        var property = value.GetType().GetRuntimeProperty("Count");
        if (property != null && property.CanRead && property.PropertyType == typeof(int))
        {
            count = (int)property.GetValue(value)!;
            return true;
        }

        count = -1;
        return false;
    }
}