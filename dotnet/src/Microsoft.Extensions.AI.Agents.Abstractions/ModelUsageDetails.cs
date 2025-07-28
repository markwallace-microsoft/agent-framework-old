// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Shared.Diagnostics;

namespace Microsoft.Extensions.AI;

/// <summary>Provides usage details about a request/response.</summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class ModelUsageDetails
{
    /// <summary>Gets or sets the number of tokens in the input.</summary>
    public long? InputTokenCount { get; set; }

    /// <summary>Gets or sets the number of tokens in the output.</summary>
    public long? OutputTokenCount { get; set; }

    /// <summary>Gets or sets the total number of tokens used to produce the response.</summary>
    public long? TotalTokenCount { get; set; }

    /// <summary>Gets or sets a dictionary of additional usage counts.</summary>
    /// <remarks>
    /// All values set here are assumed to be summable. For example, when middleware makes multiple calls to an underlying
    /// service, it may sum the counts from multiple results to produce an overall <see cref="ModelUsageDetails"/>.
    /// </remarks>
    public IDictionary<string, object?>? AdditionalCounts { get; set; }

    /// <summary>Adds usage data from another <see cref="ModelUsageDetails"/> into this instance.</summary>
    /// <param name="usage">The source <see cref="ModelUsageDetails"/> with which to augment this instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="usage"/> is <see langword="null"/>.</exception>
    public void Add(ModelUsageDetails usage)
    {
        _ = Throw.IfNull(usage);

        this.InputTokenCount = NullableSum(this.InputTokenCount, usage.InputTokenCount);
        this.OutputTokenCount = NullableSum(this.OutputTokenCount, usage.OutputTokenCount);
        this.TotalTokenCount = NullableSum(this.TotalTokenCount, usage.TotalTokenCount);

        if (usage.AdditionalCounts is { } countsToAdd)
        {
            if (this.AdditionalCounts is null)
            {
                this.AdditionalCounts = new Dictionary<string, object?>(countsToAdd);
            }
            else
            {
                foreach (var kvp in countsToAdd)
                {
                    this.AdditionalCounts[kvp.Key] = this.AdditionalCounts.TryGetValue(kvp.Key, out var existingValue) ?
                        ((long)kvp.Value! + (long)existingValue!) :
                        kvp.Value;
                }
            }
        }
    }

    /// <summary>Gets a string representing this instance to display in the debugger.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal string DebuggerDisplay
    {
        get
        {
            List<string> parts = [];

            if (this.InputTokenCount is { } input)
            {
                parts.Add($"{nameof(this.InputTokenCount)} = {input}");
            }

            if (this.OutputTokenCount is { } output)
            {
                parts.Add($"{nameof(this.OutputTokenCount)} = {output}");
            }

            if (this.TotalTokenCount is { } total)
            {
                parts.Add($"{nameof(this.TotalTokenCount)} = {total}");
            }

            if (this.AdditionalCounts is { } additionalCounts)
            {
                foreach (var entry in additionalCounts)
                {
                    parts.Add($"{entry.Key} = {entry.Value}");
                }
            }

            return string.Join(", ", parts);
        }
    }

    private static long? NullableSum(long? a, long? b) => (a.HasValue || b.HasValue) ? (a ?? 0) + (b ?? 0) : null;
}
