// Copyright (c) Microsoft. All rights reserved.

using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.AI;

/// <summary>Describes a location in the associated <see cref="ModelContent"/> based on starting and ending character indices.</summary>
/// <remarks>This <see cref="ModelAnnotatedRegion"/> typically applies to <see cref="TextModelContent"/>.</remarks>
[DebuggerDisplay("[{StartIndex}, {EndIndex})")]
public sealed class TextSpanModelAnnotatedRegion : ModelAnnotatedRegion
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextSpanModelAnnotatedRegion"/> class.
    /// </summary>
    public TextSpanModelAnnotatedRegion()
    {
    }

    /// <summary>
    /// Gets or sets the start character index (inclusive) of the annotated span in the <see cref="ModelContent"/>.
    /// </summary>
    [JsonPropertyName("start")]
    public int? StartIndex { get; set; }

    /// <summary>
    /// Gets or sets the end character index (exclusive) of the annotated span in the <see cref="ModelContent"/>.
    /// </summary>
    [JsonPropertyName("end")]
    public int? EndIndex { get; set; }
}
