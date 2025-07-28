// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.Shared.Diagnostics;

namespace Microsoft.Extensions.AI;

/// <summary>Represents the response to a chat request.</summary>
public class ModelResponse
{
    /// <summary>The response messages.</summary>
    private IList<ModelMessage>? _messages;

    /// <summary>Initializes a new instance of the <see cref="ModelResponse"/> class.</summary>
    public ModelResponse()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ModelResponse"/> class.</summary>
    /// <param name="message">The response message.</param>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is <see langword="null"/>.</exception>
    public ModelResponse(ModelMessage message)
    {
        _ = Throw.IfNull(message);

        this.Messages.Add(message);
    }

    /// <summary>Initializes a new instance of the <see cref="ModelResponse"/> class.</summary>
    /// <param name="messages">The response messages.</param>
    public ModelResponse(IList<ModelMessage>? messages)
    {
        this._messages = messages;
    }

    /// <summary>Gets or sets the chat response messages.</summary>
    [AllowNull]
    public IList<ModelMessage> Messages
    {
        get => this._messages ??= new List<ModelMessage>(1);
        set => this._messages = value;
    }

    /// <summary>Gets the text of the response.</summary>
    /// <remarks>
    /// This property concatenates the <see cref="ModelMessage.Text"/> of all <see cref="ModelMessage"/>
    /// instances in <see cref="Messages"/>.
    /// </remarks>
    [JsonIgnore]
    public string Text => this._messages?.ConcatText() ?? string.Empty;

    /// <summary>Gets or sets the ID of the chat response.</summary>
    public string? ResponseId { get; set; }

    /// <summary>Gets or sets an identifier for the state of the conversation.</summary>
    public string? ConversationId { get; set; }

    /// <summary>Gets or sets the model ID used in the creation of the chat response.</summary>
    public string? ModelId { get; set; }

    /// <summary>Gets or sets a timestamp for the chat response.</summary>
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>Gets or sets the reason for the chat response.</summary>
    public ModelFinishReason? FinishReason { get; set; }

    /// <summary>Gets or sets usage details for the chat response.</summary>
    public ModelUsageDetails? Usage { get; set; }

    /// <summary>Gets or sets the raw representation of the chat response from an underlying implementation.</summary>
    /// <remarks>
    /// If a <see cref="ModelResponse"/> is created to represent some underlying object from another object
    /// model, this property can be used to store that original object. This can be useful for debugging or
    /// for enabling a consumer to access the underlying object model if needed.
    /// </remarks>
    [JsonIgnore]
    public object? RawRepresentation { get; set; }

    /// <summary>Gets or sets any additional properties associated with the chat response.</summary>
    public IDictionary<string, object?>? AdditionalProperties { get; set; }

    /// <inheritdoc />
    public override string ToString() => this.Text;

    /// <summary>Creates an array of <see cref="ModelResponseUpdate" /> instances that represent this <see cref="ModelResponse" />.</summary>
    /// <returns>An array of <see cref="ModelResponseUpdate" /> instances that may be used to represent this <see cref="ModelResponse" />.</returns>
    public ModelResponseUpdate[] ToModelResponseUpdates()
    {
        ModelResponseUpdate? extra = null;
        if (this.AdditionalProperties is not null || this.Usage is not null)
        {
            extra = new ModelResponseUpdate
            {
                AdditionalProperties = this.AdditionalProperties
            };

            if (this.Usage is { } usage)
            {
                extra.Contents.Add(new UsageModelContent(usage));
            }
        }

        int messageCount = this._messages?.Count ?? 0;
        var updates = new ModelResponseUpdate[messageCount + (extra is not null ? 1 : 0)];

        int i;
        for (i = 0; i < messageCount; i++)
        {
            ModelMessage message = this._messages![i];
            updates[i] = new ModelResponseUpdate
            {
                AdditionalProperties = message.AdditionalProperties,
                AuthorName = message.AuthorName,
                Contents = message.Contents,
                MessageId = message.MessageId,
                RawRepresentation = message.RawRepresentation,
                Role = message.Role,

                ConversationId = this.ConversationId,
                FinishReason = this.FinishReason,
                ModelId = this.ModelId,
                ResponseId = this.ResponseId,

                CreatedAt = message.CreatedAt ?? this.CreatedAt,
            };
        }

        if (extra is not null)
        {
            updates[i] = extra;
        }

        return updates;
    }
}
