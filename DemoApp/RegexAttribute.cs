using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp;
/// <summary>
/// Represents a custom attribute that specifies a regular expression pattern for validation.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public class RegexAttribute : RegularExpressionAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegexAttribute"/> class with the specified regular expression pattern.
    /// </summary>
    /// <param name="pattern">The regular expression pattern.</param>
    public RegexAttribute(string pattern) : base(pattern)
    {
    }
}