// Copyright (c) Microsoft. All rights reserved.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.AI;

/// <summary>
/// Represents text reasoning content in a chat.
/// </summary>
/// <remarks>
/// <see cref="TextReasoningModelContent"/> is distinct from <see cref="TextModelContent"/>. <see cref="TextReasoningModelContent"/>
/// represents "thinking" or "reasoning" performed by the model and is distinct from the actual output text from
/// the model, which is represented by <see cref="TextModelContent"/>. Neither types derives from the other.
/// </remarks>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class TextReasoningModelContent : ModelContent
{
    private string? _text;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextReasoningModelContent"/> class.
    /// </summary>
    /// <param name="text">The text reasoning content.</param>
    public TextReasoningModelContent(string? text)
    {
        this._text = text;
    }

    /// <summary>
    /// Gets or sets the text reasoning content.
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
    private string DebuggerDisplay => $"Reasoning = \"{this.Text}\"";
}
