// Copyright (c) Microsoft. All rights reserved.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Shared.Diagnostics;

namespace Microsoft.Extensions.AI;

/// <summary>
/// Describes the intended purpose of a message within a model interaction.
/// </summary>
[JsonConverter(typeof(Converter))]
[DebuggerDisplay("{Value,nq}")]
public readonly struct ModelRole : IEquatable<ModelRole>, IModelRole
{
    /// <summary>Gets the role that instructs or sets the behavior of the system.</summary>
    public static ModelRole System { get; } = new("system");

    /// <summary>Gets the role that provides responses to system-instructed, user-prompted input.</summary>
    public static ModelRole Assistant { get; } = new("assistant");

    /// <summary>Gets the role that provides user input for model interactions.</summary>
    public static ModelRole User { get; } = new("user");

    /// <summary>Gets the role that provides additional information and references in response to tool use requests.</summary>
    public static ModelRole Tool { get; } = new("tool");

    /// <summary>
    /// Gets the value associated with this <see cref="ModelRole"/>.
    /// </summary>
    /// <remarks>
    /// The value will be serialized into the "role" message field of the Chat Message format.
    /// </remarks>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelRole"/> struct with the provided value.
    /// </summary>
    /// <param name="value">The value to associate with this <see cref="ModelRole"/>.</param>
    [JsonConstructor]
    public ModelRole(string value)
    {
        this.Value = Throw.IfNullOrWhitespace(value);
    }

    /// <summary>
    /// Returns a value indicating whether two <see cref="ModelRole"/> instances are equivalent, as determined by a
    /// case-insensitive comparison of their values.
    /// </summary>
    /// <param name="left">The first <see cref="ModelRole"/> instance to compare.</param>
    /// <param name="right">The second <see cref="ModelRole"/> instance to compare.</param>
    /// <returns><see langword="true"/> if left and right are both <see langword="null"/> or have equivalent values; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(ModelRole left, ModelRole right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Returns a value indicating whether two <see cref="ModelRole"/> instances are not equivalent, as determined by a
    /// case-insensitive comparison of their values.
    /// </summary>
    /// <param name="left">The first <see cref="ModelRole"/> instance to compare. </param>
    /// <param name="right">The second <see cref="ModelRole"/> instance to compare. </param>
    /// <returns><see langword="true"/> if left and right have different values; <see langword="false"/> if they have equivalent values or are both <see langword="null"/>.</returns>
    public static bool operator !=(ModelRole left, ModelRole right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is ModelRole otherRole && this.Equals(otherRole);

    /// <inheritdoc/>
    public bool Equals(ModelRole other)
        => string.Equals(this.Value, other.Value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override int GetHashCode()
        => StringComparer.OrdinalIgnoreCase.GetHashCode(this.Value);

    /// <inheritdoc/>
    public override string ToString() => this.Value;

    /// <summary>Provides a <see cref="JsonConverter{ModelRole}"/> for serializing <see cref="ModelRole"/> instances.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Converter : JsonConverter<ModelRole>
    {
        /// <inheritdoc />
        public override ModelRole Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            new(reader.GetString()!);

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, ModelRole value, JsonSerializerOptions options) =>
            Throw.IfNull(writer).WriteStringValue(value.Value);
    }
}
