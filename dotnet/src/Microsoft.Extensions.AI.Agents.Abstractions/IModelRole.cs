// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Extensions.AI;

/// <summary>
/// Describes the intended purpose of a message within a chat interaction.
/// </summary>
public interface IModelRole
{
    /// <summary>
    /// Gets the value associated with this <see cref="IModelRole"/>.
    /// </summary>
    /// <remarks>
    /// The value will be serialized into the "role" message field of the Chat Message format.
    /// </remarks>
    string Value { get; }
}
