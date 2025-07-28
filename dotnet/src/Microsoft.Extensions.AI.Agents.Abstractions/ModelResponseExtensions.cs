// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Shared.Diagnostics;

#pragma warning disable S109 // Magic numbers should not be used
#pragma warning disable S1121 // Assignments should not be made from within sub-expressions

namespace Microsoft.Extensions.AI;

/// <summary>
/// Provides extension methods for working with <see cref="ModelResponse"/> and <see cref="ModelResponseUpdate"/> instances.
/// </summary>
public static class ModelResponseExtensions
{
    /// <summary>Adds all of the messages from <paramref name="response"/> into <paramref name="list"/>.</summary>
    /// <param name="list">The destination list to which the messages from <paramref name="response"/> should be added.</param>
    /// <param name="response">The response containing the messages to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="list"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/>.</exception>
    public static void AddMessages(this IList<ModelMessage> list, ModelResponse response)
    {
        _ = Throw.IfNull(list);
        _ = Throw.IfNull(response);

        if (list is List<ModelMessage> listConcrete)
        {
            listConcrete.AddRange(response.Messages);
        }
        else
        {
            foreach (var message in response.Messages)
            {
                list.Add(message);
            }
        }
    }

    /// <summary>Converts the <paramref name="updates"/> into <see cref="ModelMessage"/> instances and adds them to <paramref name="list"/>.</summary>
    /// <param name="list">The destination list to which the newly constructed messages should be added.</param>
    /// <param name="updates">The <see cref="ModelResponseUpdate"/> instances to convert to messages and add to the list.</param>
    /// <exception cref="ArgumentNullException"><paramref name="list"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="updates"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// As part of combining <paramref name="updates"/> into a series of <see cref="ModelMessage"/> instances, the
    /// method may use <see cref="ModelResponseUpdate.MessageId"/> to determine message boundaries, as well as coalesce
    /// contiguous <see cref="ModelContent"/> items where applicable, e.g. multiple
    /// <see cref="TextModelContent"/> instances in a row may be combined into a single <see cref="TextModelContent"/>.
    /// </remarks>
    public static void AddMessages(this IList<ModelMessage> list, IEnumerable<ModelResponseUpdate> updates)
    {
        _ = Throw.IfNull(list);
        _ = Throw.IfNull(updates);

        if (updates is ICollection<ModelResponseUpdate> { Count: 0 })
        {
            return;
        }

        list.AddMessages(updates.ToModelResponse());
    }

    /// <summary>Converts the <paramref name="update"/> into a <see cref="ModelMessage"/> instance and adds it to <paramref name="list"/>.</summary>
    /// <param name="list">The destination list to which the newly constructed message should be added.</param>
    /// <param name="update">The <see cref="ModelResponseUpdate"/> instance to convert to a message and add to the list.</param>
    /// <param name="filter">A predicate to filter which <see cref="ModelContent"/> gets included in the message.</param>
    /// <exception cref="ArgumentNullException"><paramref name="list"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="update"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// If the <see cref="ModelResponseUpdate"/> has no content, or all its content gets excluded by <paramref name="filter"/>, then
    /// no <see cref="ModelMessage"/> will be added to the <paramref name="list"/>.
    /// </remarks>
    public static void AddMessages(this IList<ModelMessage> list, ModelResponseUpdate update, Func<ModelContent, bool>? filter = null)
    {
        _ = Throw.IfNull(list);
        _ = Throw.IfNull(update);
        _ = Throw.IfNull(update.Role);

        var contentsList = filter is null ? update.Contents : update.Contents.Where(filter).ToList();
        if (contentsList.Count > 0)
        {
            list.Add(new(update.Role, contentsList)
            {
                AuthorName = update.AuthorName,
                CreatedAt = update.CreatedAt,
                RawRepresentation = update.RawRepresentation,
                AdditionalProperties = update.AdditionalProperties,
            });
        }
    }

    /// <summary>Converts the <paramref name="updates"/> into <see cref="ModelMessage"/> instances and adds them to <paramref name="list"/>.</summary>
    /// <param name="list">The list to which the newly constructed messages should be added.</param>
    /// <param name="updates">The <see cref="ModelResponseUpdate"/> instances to convert to messages and add to the list.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task"/> representing the completion of the operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="list"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="updates"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// As part of combining <paramref name="updates"/> into a series of <see cref="ModelMessage"/> instances, tne
    /// method may use <see cref="ModelResponseUpdate.MessageId"/> to determine message boundaries, as well as coalesce
    /// contiguous <see cref="ModelContent"/> items where applicable, e.g. multiple
    /// <see cref="TextModelContent"/> instances in a row may be combined into a single <see cref="TextModelContent"/>.
    /// </remarks>
    public static Task AddMessagesAsync(
        this IList<ModelMessage> list, IAsyncEnumerable<ModelResponseUpdate> updates, CancellationToken cancellationToken = default)
    {
        _ = Throw.IfNull(list);
        _ = Throw.IfNull(updates);

        return AddMessagesAsync(list, updates, cancellationToken);

        static async Task AddMessagesAsync(
            IList<ModelMessage> list, IAsyncEnumerable<ModelResponseUpdate> updates, CancellationToken cancellationToken) =>
            list.AddMessages(await updates.ToModelResponseAsync(cancellationToken).ConfigureAwait(false));
    }

    /// <summary>Combines <see cref="ModelResponseUpdate"/> instances into a single <see cref="ModelResponse"/>.</summary>
    /// <param name="updates">The updates to be combined.</param>
    /// <returns>The combined <see cref="ModelResponse"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="updates"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// As part of combining <paramref name="updates"/> into a single <see cref="ModelResponse"/>, the method will attempt to reconstruct
    /// <see cref="ModelMessage"/> instances. This includes using <see cref="ModelResponseUpdate.MessageId"/> to determine
    /// message boundaries, as well as coalescing contiguous <see cref="ModelContent"/> items where applicable, e.g. multiple
    /// <see cref="TextModelContent"/> instances in a row may be combined into a single <see cref="TextModelContent"/>.
    /// </remarks>
    public static ModelResponse ToModelResponse(
        this IEnumerable<ModelResponseUpdate> updates)
    {
        _ = Throw.IfNull(updates);

        ModelResponse response = new();

        foreach (var update in updates)
        {
            ProcessUpdate(update, response);
        }

        FinalizeResponse(response);

        return response;
    }

    /// <summary>Combines <see cref="ModelResponseUpdate"/> instances into a single <see cref="ModelResponse"/>.</summary>
    /// <param name="updates">The updates to be combined.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The combined <see cref="ModelResponse"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="updates"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// As part of combining <paramref name="updates"/> into a single <see cref="ModelResponse"/>, the method will attempt to reconstruct
    /// <see cref="ModelMessage"/> instances. This includes using <see cref="ModelResponseUpdate.MessageId"/> to determine
    /// message boundaries, as well as coalescing contiguous <see cref="ModelContent"/> items where applicable, e.g. multiple
    /// <see cref="TextModelContent"/> instances in a row may be combined into a single <see cref="TextModelContent"/>.
    /// </remarks>
    public static Task<ModelResponse> ToModelResponseAsync(
        this IAsyncEnumerable<ModelResponseUpdate> updates, CancellationToken cancellationToken = default)
    {
        _ = Throw.IfNull(updates);

        return ToModelResponseAsync(updates, cancellationToken);

        static async Task<ModelResponse> ToModelResponseAsync(
            IAsyncEnumerable<ModelResponseUpdate> updates, CancellationToken cancellationToken)
        {
            ModelResponse response = new();

            await foreach (var update in updates.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                ProcessUpdate(update, response);
            }

            FinalizeResponse(response);

            return response;
        }
    }

    /// <summary>Coalesces sequential <see cref="ModelContent"/> content elements.</summary>
    internal static void CoalesceTextModelContent(List<ModelContent> contents)
    {
        Coalesce<TextModelContent>(contents, static text => new(text));
        //Coalesce<TextReasoningContent>(contents, static text => new(text));

        // This implementation relies on TContent's ToString returning its exact text.
        static void Coalesce<TContent>(List<ModelContent> contents, Func<string, TContent> fromText)
            where TContent : ModelContent
        {
            StringBuilder? coalescedText = null;

            // Iterate through all of the items in the list looking for contiguous items that can be coalesced.
            int start = 0;
            while (start < contents.Count - 1)
            {
                // We need at least two TextModelContents in a row to be able to coalesce. We also avoid touching contents
                // that have annotations, as we want to ensure the annotations (and in particular any start/end indices
                // into the text content) remain accurate.
                if (!TryAsCoalescable(contents[start], out var firstText))
                {
                    start++;
                    continue;
                }

                if (!TryAsCoalescable(contents[start + 1], out var secondText))
                {
                    start += 2;
                    continue;
                }

                // Append the text from those nodes and continue appending subsequent TextModelContents until we run out.
                // We null out nodes as their text is appended so that we can later remove them all in one O(N) operation.
                coalescedText ??= new();
                _ = coalescedText.Clear().Append(firstText).Append(secondText);
                contents[start + 1] = null!;
                int i = start + 2;
                for (; i < contents.Count && TryAsCoalescable(contents[i], out TContent? next); i++)
                {
                    _ = coalescedText.Append(next);
                    contents[i] = null!;
                }

                // Store the replacement node. We inherit the properties of the first text node. We don't
                // currently propagate additional properties from the subsequent nodes. If we ever need to,
                // we can add that here.
                var newContent = fromText(coalescedText.ToString());
                contents[start] = newContent;
                newContent.AdditionalProperties = firstText.AdditionalProperties != null
                    ? new Dictionary<string, object?>(firstText.AdditionalProperties)
                    : null;

                start = i;

                static bool TryAsCoalescable(ModelContent content, [NotNullWhen(true)] out TContent? coalescable)
                {
                    if (content is TContent && (content is not TextModelContent tc || tc.Annotations is not { Count: > 0 }))
                    {
                        coalescable = (TContent)content;
                        return true;
                    }

                    coalescable = null!;
                    return false;
                }
            }

            // Remove all of the null slots left over from the coalescing process.
            _ = contents.RemoveAll(u => u is null);
        }
    }

    /// <summary>Finalizes the <paramref name="response"/> object.</summary>
    private static void FinalizeResponse(ModelResponse response)
    {
        int count = response.Messages.Count;
        for (int i = 0; i < count; i++)
        {
            CoalesceTextModelContent((List<ModelContent>)response.Messages[i].Contents);
        }
    }

    /// <summary>Processes the <see cref="ModelResponseUpdate"/>, incorporating its contents into <paramref name="response"/>.</summary>
    /// <param name="update">The update to process.</param>
    /// <param name="response">The <see cref="ModelResponse"/> object that should be updated based on <paramref name="update"/>.</param>
    private static void ProcessUpdate(ModelResponseUpdate update, ModelResponse response)
    {
        // If there is no message created yet, or if the last update we saw had a different
        // message ID than the newest update, create a new message.
        ModelMessage message;
        var isNewMessage = false;
        if (response.Messages.Count == 0)
        {
            isNewMessage = true;
        }
        else if (update.MessageId is { Length: > 0 } updateMessageId
            && response.Messages[response.Messages.Count - 1].MessageId is string lastMessageId
            && updateMessageId != lastMessageId)
        {
            isNewMessage = true;
        }

        if (isNewMessage)
        {
            message = new(update.Role!, []);
            response.Messages.Add(message);
        }
        else
        {
            message = response.Messages[response.Messages.Count - 1];
        }

        // Some members on ModelResponseUpdate map to members of ModelMessage.
        // Incorporate those into the latest message; in cases where the message
        // stores a single value, prefer the latest update's value over anything
        // stored in the message.

        if (update.AuthorName is not null)
        {
            message.AuthorName = update.AuthorName;
        }

        if (update.CreatedAt is not null)
        {
            message.CreatedAt = update.CreatedAt;
        }

        if (update.Role is IModelRole role)
        {
            message.Role = role;
        }

        if (update.MessageId is { Length: > 0 })
        {
            // Note that this must come after the message checks earlier, as they depend
            // on this value for change detection.
            message.MessageId = update.MessageId;
        }

        foreach (var content in update.Contents)
        {
            switch (content)
            {
                // Usage content is treated specially and propagated to the response's Usage.
                case UsageModelContent usage:
                    (response.Usage ??= new()).Add(usage.Details);
                    break;

                default:
                    message.Contents.Add(content);
                    break;
            }
        }

        // Other members on a ModelResponseUpdate map to members of the ModelResponse.
        // Update the response object with those, preferring the values from later updates.

        if (update.ResponseId is { Length: > 0 })
        {
            response.ResponseId = update.ResponseId;
        }

        if (update.ConversationId is not null)
        {
            response.ConversationId = update.ConversationId;
        }

        if (update.CreatedAt is not null)
        {
            response.CreatedAt = update.CreatedAt;
        }

        if (update.FinishReason is not null)
        {
            response.FinishReason = update.FinishReason;
        }

        if (update.ModelId is not null)
        {
            response.ModelId = update.ModelId;
        }

        if (update.AdditionalProperties is not null)
        {
            if (response.AdditionalProperties is null)
            {
                response.AdditionalProperties = new Dictionary<string, object?>(update.AdditionalProperties);
            }
            else
            {
                foreach (var kvp in update.AdditionalProperties)
                {
                    response.AdditionalProperties[kvp.Key] = kvp.Value;
                }
            }
        }
    }
}
