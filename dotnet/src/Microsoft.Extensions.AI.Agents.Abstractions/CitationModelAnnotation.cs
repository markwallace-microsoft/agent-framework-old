// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Microsoft.Extensions.AI;

/// <summary>
/// Represents an annotation that links content to source references,
/// such as documents, URLs, files, or tool outputs.
/// </summary>
public class CitationModelAnnotation : ModelAnnotation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CitationModelAnnotation"/> class.
    /// </summary>
    public CitationModelAnnotation()
    {
    }

    /// <summary>
    /// Gets or sets the title or name of the source.
    /// </summary>
    /// <remarks>
    /// This could be the title of a document, a title from a web page, a name of a file, or similarly descriptive text.
    /// </remarks>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets a URI from which the source material was retrieved.
    /// </summary>
    public Uri? Url { get; set; }

    /// <summary>Gets or sets a source identifier associated with the annotation.</summary>
    /// <remarks>
    /// This is a provider-specific identifier that can be used to reference the source material by
    /// an ID. This may be a document ID, or a file ID, or some other identifier for the source material
    /// that can be used to uniquely identify it with the provider.
    /// </remarks>
    public string? FileId { get; set; }

    /// <summary>Gets or sets the name of any tool involved in the production of the associated content.</summary>
    public string? ToolName { get; set; }

    /// <summary>
    /// Gets or sets a snippet or excerpt from the source that was cited.
    /// </summary>
    public string? Snippet { get; set; }
}
