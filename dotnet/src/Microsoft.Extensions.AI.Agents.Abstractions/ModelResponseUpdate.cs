// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.AI;

/// <summary>
/// Represents a single streaming response chunk from a large language model.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ModelResponseUpdate"/> is so named because it represents updates
/// that layer on each other to form a single chat response. Conceptually, this combines the roles of
/// <see cref="ModelResponse"/> and <see cref="ModelMessage"/> in streaming output.
/// </para>
/// <para>
/// The relationship between <see cref="ModelResponse"/> and <see cref="ModelResponseUpdate"/> is
/// codified in the <see cref="ModelResponseExtensions.ToModelResponseAsync"/> and
/// <see cref="ModelResponse.ToModelResponseUpdates"/>, which enable bidirectional conversions
/// between the two. Note, however, that the provided conversions may be lossy, for example if multiple
/// updates all have different <see cref="RawRepresentation"/> objects whereas there's only one slot for
/// such an object available in <see cref="ModelResponse.RawRepresentation"/>. Similarly, if different
/// updates provide different values for properties like <see cref="ModelId"/>,
/// only one of the values will be used to populate <see cref="ModelResponse.ModelId"/>.
/// </para>
/// </remarks>
[DebuggerDisplay("[{Role}] {ContentForDebuggerDisplay}{EllipsesForDebuggerDisplay,nq}")]
public class ModelResponseUpdate
{
    /// <summary>The response update content items.</summary>
    private IList<ModelContent>? _contents;

    /// <summary>The name of the author of the update.</summary>
    private string? _authorName;

    /// <summary>Initializes a new instance of the <see cref="ModelResponseUpdate"/> class.</summary>
    [JsonConstructor]
    public ModelResponseUpdate()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ModelResponseUpdate"/> class.</summary>
    /// <param name="role">The role of the author of the update.</param>
    /// <param name="content">The text content of the update.</param>
    public ModelResponseUpdate(IModelRole? role, string? content)
        : this(role, content is null ? null : [new TextModelContent(content)])
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ModelResponseUpdate"/> class.</summary>
    /// <param name="role">The role of the author of the update.</param>
    /// <param name="contents">The contents of the update.</param>
    public ModelResponseUpdate(IModelRole? role, IList<ModelContent>? contents)
    {
        this.Role = role;
        this._contents = contents;
    }

    /// <summary>Gets or sets the name of the author of the response update.</summary>
    public string? AuthorName
    {
        get => this._authorName;
        set => this._authorName = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>Gets or sets the role of the author of the response update.</summary>
    public IModelRole? Role { get; set; }

    /// <summary>Gets the text of this update.</summary>
    /// <remarks>
    /// This property concatenates the text of all <see cref="TextModelContent"/> objects in <see cref="Contents"/>.
    /// </remarks>
    [JsonIgnore]
    public string Text => this._contents is not null ? this._contents.ConcatText() : string.Empty;

    /// <summary>Gets or sets the chat response update content items.</summary>
    [AllowNull]
    public IList<ModelContent> Contents
    {
        get => this._contents ??= [];
        set => this._contents = value;
    }

    /// <summary>Gets or sets the raw representation of the response update from an underlying implementation.</summary>
    /// <remarks>
    /// If a <see cref="ModelResponseUpdate"/> is created to represent some underlying object from another object
    /// model, this property can be used to store that original object. This can be useful for debugging or
    /// for enabling a consumer to access the underlying object model if needed.
    /// </remarks>
    [JsonIgnore]
    public object? RawRepresentation { get; set; }

    /// <summary>Gets or sets additional properties for the update.</summary>
    public IDictionary<string, object?>? AdditionalProperties { get; set; }

    /// <summary>Gets or sets the ID of the response of which this update is a part.</summary>
    public string? ResponseId { get; set; }

    /// <summary>Gets or sets the ID of the message of which this update is a part.</summary>
    /// <remarks>
    /// A single streaming response may be composed of multiple messages, each of which may be represented
    /// by multiple updates. This property is used to group those updates together into messages.
    ///
    /// Some providers may consider streaming responses to be a single message, and in that case
    /// the value of this property may be the same as the response ID.
    ///
    /// This value is used when <see cref="ModelResponseExtensions.ToModelResponseAsync(IAsyncEnumerable{ModelResponseUpdate}, System.Threading.CancellationToken)"/>
    /// groups <see cref="ModelResponseUpdate"/> instances into <see cref="ModelMessage"/> instances.
    /// The value must be unique to each call to the underlying provider, and must be shared by
    /// all updates that are part of the same logical message within a streaming response.
    /// </remarks>
    public string? MessageId { get; set; }

    /// <summary>Gets or sets an identifier for the state of the conversation of which this update is a part.</summary>
    public string? ConversationId { get; set; }

    /// <summary>Gets or sets a timestamp for the response update.</summary>
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>Gets or sets the finish reason for the operation.</summary>
    public ModelFinishReason? FinishReason { get; set; }

    /// <summary>Gets or sets the model ID associated with this response update.</summary>
    public string? ModelId { get; set; }

    /// <inheritdoc/>
    public override string ToString() => this.Text;

    /// <summary>Gets a <see cref="ModelContent"/> object to display in the debugger display.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ModelContent? ContentForDebuggerDisplay => this._contents is { Count: > 0 } ? this._contents[0] : null;

    /// <summary>Gets an indication for the debugger display of whether there's more content.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string EllipsesForDebuggerDisplay => this._contents is { Count: > 1 } ? ", ..." : string.Empty;
}
