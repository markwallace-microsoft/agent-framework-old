// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.AI;

/// <summary>
/// Represents a message in a chat conversation, including the role of the author, the content of the message, and additional metadata.
/// </summary>
public class ModelMessage
{
    private IList<ModelContent>? _contents;
    private string? _authorName;

    /// <summary>Initializes a new instance of the <see cref="ModelMessage"/> class.</summary>
    [JsonConstructor]
    public ModelMessage()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ModelMessage"/> class.</summary>
    /// <param name="role">The role of the author of the message.</param>
    /// <param name="content">The text content of the message.</param>
    public ModelMessage(ModelRole role, string? content)
        : this(role, content is null ? [] : [new TextModelContent(content)])
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ModelMessage"/> class.</summary>
    /// <param name="role">The role of the author of the message.</param>
    /// <param name="contents">The contents for this message.</param>
    public ModelMessage(IModelRole role, IList<ModelContent>? contents)
    {
        this.Role = role;
        this._contents = contents;
    }

    /// <summary>Clones the <see cref="ModelMessage"/> to a new <see cref="ModelMessage"/> instance.</summary>
    /// <returns>A shallow clone of the original message object.</returns>
    /// <remarks>
    /// This is a shallow clone. The returned instance is different from the original, but all properties
    /// refer to the same objects as the original.
    /// </remarks>
    public ModelMessage Clone() =>
        new()
        {
            AdditionalProperties = this.AdditionalProperties,
            _authorName = this._authorName,
            _contents = this._contents,
            CreatedAt = this.CreatedAt,
            RawRepresentation = this.RawRepresentation,
            Role = this.Role,
            MessageId = this.MessageId,
        };

    /// <summary>Gets or sets the name of the author of the message.</summary>
    public string? AuthorName
    {
        get => this._authorName;
        set => this._authorName = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>Gets or sets a timestamp for the chat message.</summary>
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>Gets or sets the role of the author of the message.</summary>
    public IModelRole Role { get; set; } = ModelRole.User;

    /// <summary>Gets the text of this message.</summary>
    /// <remarks>
    /// This property concatenates the text of all <see cref="ModelContent"/> objects that represent text in <see cref="Contents"/>.
    /// </remarks>
    [JsonIgnore]
    public string Text => this.Contents.ConcatText();

    /// <summary>Gets or sets the chat message content items.</summary>
    [AllowNull]
    public IList<ModelContent> Contents
    {
        get => this._contents ??= [];
        set => this._contents = value;
    }

    /// <summary>Gets or sets the ID of the chat message.</summary>
    public string? MessageId { get; set; }

    /// <summary>Gets or sets the raw representation of the chat message from an underlying implementation.</summary>
    /// <remarks>
    /// If a <see cref="ModelMessage"/> is created to represent some underlying object from another object
    /// model, this property can be used to store that original object. This can be useful for debugging or
    /// for enabling a consumer to access the underlying object model if needed.
    /// </remarks>
    [JsonIgnore]
    public object? RawRepresentation { get; set; }

    /// <summary>Gets or sets any additional properties associated with the message.</summary>
    public IDictionary<string, object?>? AdditionalProperties { get; set; }

    /// <inheritdoc/>
    public override string ToString() => this.Text;

    /// <summary>Gets a <see cref="ModelContent"/> object to display in the debugger display.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ModelContent? ContentForDebuggerDisplay => this._contents is { Count: > 0 } ? this._contents[0] : null;

    /// <summary>Gets an indication for the debugger display of whether there's more content.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string EllipsesForDebuggerDisplay => this._contents is { Count: > 1 } ? ", ..." : string.Empty;
}
