// Copyright (c) Microsoft. All rights reserved.

using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.Shared.Diagnostics;

namespace Microsoft.Extensions.AI;

/// <summary>
/// Represents usage information associated with a chat request and response.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class UsageModelContent : ModelContent
{
    /// <summary>Usage information.</summary>
    private ModelUsageDetails _details;

    /// <summary>Initializes a new instance of the <see cref="UsageModelContent"/> class with an empty <see cref="ModelUsageDetails"/>.</summary>
    public UsageModelContent()
    {
        this._details = new();
    }

    /// <summary>Initializes a new instance of the <see cref="UsageModelContent"/> class with the specified <see cref="ModelUsageDetails"/> instance.</summary>
    /// <param name="details">The usage details to store in this content.</param>
    [JsonConstructor]
    public UsageModelContent(ModelUsageDetails details)
    {
        this._details = Throw.IfNull(details);
    }

    /// <summary>Gets or sets the usage information.</summary>
    public ModelUsageDetails Details
    {
        get => this._details;
        set => this._details = Throw.IfNull(value);
    }

    /// <summary>Gets a string representing this instance to display in the debugger.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"Usage = {this._details.DebuggerDisplay}";
}
