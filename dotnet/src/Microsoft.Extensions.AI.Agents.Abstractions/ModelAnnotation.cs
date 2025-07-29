// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.AI;

/// <summary>
/// Represents an annotation on content.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(CitationModelAnnotation), typeDiscriminator: "citation")]
public class ModelAnnotation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelAnnotation"/> class.
    /// </summary>
    public ModelAnnotation()
    {
    }

    /// <summary>Gets or sets any target regions for the annotation, pointing to where in the associated <see cref="ModelContent"/> this annotation applies.</summary>
    public IList<ModelAnnotatedRegion>? AnnotatedRegions { get; set; }

    /// <summary>Gets or sets the raw representation of the annotation from an underlying implementation.</summary>
    /// <remarks>
    /// If an <see cref="ModelAnnotation"/> is created to represent some underlying object from another object
    /// model, this property can be used to store that original object. This can be useful for debugging or
    /// for enabling a consumer to access the underlying object model, if needed.
    /// </remarks>
    [JsonIgnore]
    public object? RawRepresentation { get; set; }

    /// <summary>
    /// Gets or sets additional metadata specific to the provider or source type.
    /// </summary>
    public IDictionary<string, object?>? AdditionalProperties { get; set; }
}
