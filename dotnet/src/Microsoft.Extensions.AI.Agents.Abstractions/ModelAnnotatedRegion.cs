// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace Microsoft.Extensions.AI;

/// <summary>Describes the portion of an associated <see cref="ModelContent"/> to which an annotation applies.</summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(TextSpanModelAnnotatedRegion), typeDiscriminator: "textSpan")]
public class ModelAnnotatedRegion
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelAnnotatedRegion"/> class.
    /// </summary>
    public ModelAnnotatedRegion()
    {
    }
}
