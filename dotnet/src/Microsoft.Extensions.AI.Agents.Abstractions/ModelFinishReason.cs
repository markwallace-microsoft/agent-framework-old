// Copyright (c) Microsoft. All rights reserved.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Shared.Diagnostics;

namespace Microsoft.Extensions.AI;

/// <summary>Represents the reason a chat response completed.</summary>
[JsonConverter(typeof(Converter))]
public readonly struct ModelFinishReason : IEquatable<ModelFinishReason>
{
    /// <summary>The finish reason value. If <see langword="null"/> because `default(ModelFinishReason)` was used, the instance will behave like <see cref="Stop"/>.</summary>
    private readonly string? _value;

    /// <summary>Initializes a new instance of the <see cref="ModelFinishReason"/> struct with a string that describes the reason.</summary>
    /// <param name="value">The reason value.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is empty or composed entirely of whitespace.</exception>
    [JsonConstructor]
    public ModelFinishReason(string value)
    {
        this._value = Throw.IfNullOrWhitespace(value);
    }

    /// <summary>Gets the finish reason value.</summary>
    public string Value => this._value ?? Stop.Value;

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ModelFinishReason other && this.Equals(other);

    /// <inheritdoc />
    public bool Equals(ModelFinishReason other) => StringComparer.OrdinalIgnoreCase.Equals(this.Value, other.Value);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(this.Value);

    /// <summary>
    /// Compares two instances.
    /// </summary>
    /// <param name="left">The left argument of the comparison.</param>
    /// <param name="right">The right argument of the comparison.</param>
    /// <returns><see langword="true" /> if the two instances are equal; <see langword="false" /> if they aren't equal.</returns>
    public static bool operator ==(ModelFinishReason left, ModelFinishReason right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Compares two instances.
    /// </summary>
    /// <param name="left">The left argument of the comparison.</param>
    /// <param name="right">The right argument of the comparison.</param>
    /// <returns><see langword="true" /> if the two instances aren't equal; <see langword="false" /> if they are equal.</returns>
    public static bool operator !=(ModelFinishReason left, ModelFinishReason right)
    {
        return !(left == right);
    }

    /// <summary>Gets the <see cref="Value"/> of the finish reason.</summary>
    /// <returns>The <see cref="Value"/> of the finish reason.</returns>
    public override string ToString() => this.Value;

    /// <summary>Gets a <see cref="ModelFinishReason"/> representing the model encountering a natural stop point or provided stop sequence.</summary>
    public static ModelFinishReason Stop { get; } = new("stop");

    /// <summary>Gets a <see cref="ModelFinishReason"/> representing the model reaching the maximum length allowed for the request and/or response (typically in terms of tokens).</summary>
    public static ModelFinishReason Length { get; } = new("length");

    /// <summary>Gets a <see cref="ModelFinishReason"/> representing the model requesting the use of a tool that was defined in the request.</summary>
    public static ModelFinishReason ToolCalls { get; } = new("tool_calls");

    /// <summary>Gets a <see cref="ModelFinishReason"/> representing the model filtering content, whether for safety, prohibited content, sensitive content, or other such issues.</summary>
    public static ModelFinishReason ContentFilter { get; } = new("content_filter");

    /// <summary>Provides a <see cref="JsonConverter{ModelFinishReason}"/> for serializing <see cref="ModelFinishReason"/> instances.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Converter : JsonConverter<ModelFinishReason>
    {
        /// <inheritdoc/>
        public override ModelFinishReason Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            new(reader.GetString()!);

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, ModelFinishReason value, JsonSerializerOptions options) =>
            Throw.IfNull(writer).WriteStringValue(value.Value);
    }
}
