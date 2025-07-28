// Copyright (c) Microsoft. All rights reserved.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.AI;

/// <summary>
/// Represents text content in a chat.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class TextModelContent : ModelContent
{
    private string? _text;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextModelContent"/> class.
    /// </summary>
    /// <param name="text">The text content.</param>
    public TextModelContent(string? text)
    {
        this._text = text;
    }

    /// <summary>
    /// Gets or sets the text content.
    /// </summary>
    [AllowNull]
    public string Text
    {
        get => this._text ?? string.Empty;
        set => this._text = value;
    }

    /// <inheritdoc/>
    public override string ToString() => this.Text;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"Text = \"{this.Text}\"";
}
