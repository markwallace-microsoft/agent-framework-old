// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.AI;

/// <summary>
/// Represents content used by a Large Language Model.
/// </summary>
public class ModelContent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelContent"/> class.
    /// </summary>
    public ModelContent()
    {
    }

    /// <summary>
    /// Gets or sets a list of annotations on this content.
    /// </summary>
    public IList<ModelAnnotation>? Annotations { get; set; }

    /// <summary>Gets or sets the raw representation of the content from an underlying implementation.</summary>
    /// <remarks>
    /// If an <see cref="ModelContent"/> is created to represent some underlying object from another object
    /// model, this property can be used to store that original object. This can be useful for debugging or
    /// for enabling a consumer to access the underlying object model, if needed.
    /// </remarks>
    [JsonIgnore]
    public object? RawRepresentation { get; set; }

    /// <summary>Gets or sets additional properties for the content.</summary>
    public IDictionary<string, object?>? AdditionalProperties { get; set; }
}
