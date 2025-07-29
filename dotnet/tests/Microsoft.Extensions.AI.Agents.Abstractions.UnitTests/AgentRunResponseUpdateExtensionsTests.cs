// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.AI.Agents.Abstractions.UnitTests;

public class AgentRunResponseUpdateExtensionsTests
{
    [Fact]
    public void ToAgentRunResponseWithInvalidArgsThrows()
    {
        Assert.Throws<ArgumentNullException>("updates", () => ((List<AgentRunResponseUpdate>)null!).ToAgentRunResponse());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ToAgentRunResponseSuccessfullyCreatesResponseAsync(bool useAsync)
    {
        AgentRunResponseUpdate[] updates =
        [
            new(ModelRole.Assistant, "Hello") { ResponseId = "someResponse", MessageId = "12345", CreatedAt = new DateTimeOffset(1, 2, 3, 4, 5, 6, TimeSpan.Zero), AgentId = "agentId" },
            new(new ModelRole("human"), ", ") { AuthorName = "Someone", AdditionalProperties = new Dictionary < string, object ? >() {["a"] = "b" } },
            new(null, "world!") { CreatedAt = new DateTimeOffset(2, 2, 3, 4, 5, 6, TimeSpan.Zero), AdditionalProperties = new Dictionary < string, object ? >() {["c"] = "d" } },

            new() { Contents = [new UsageModelContent(new() { InputTokenCount = 1, OutputTokenCount = 2 })] },
            new() { Contents = [new UsageModelContent(new() { InputTokenCount = 4, OutputTokenCount = 5 })] },
        ];

        AgentRunResponse response = useAsync ?
            updates.ToAgentRunResponse() :
            await YieldAsync(updates).ToAgentRunResponseAsync();
        Assert.NotNull(response);

        Assert.Equal("agentId", response.AgentId);

        Assert.NotNull(response.Usage);
        Assert.Equal(5, response.Usage.InputTokenCount);
        Assert.Equal(7, response.Usage.OutputTokenCount);

        Assert.Equal("someResponse", response.ResponseId);
        Assert.Equal(new DateTimeOffset(2, 2, 3, 4, 5, 6, TimeSpan.Zero), response.CreatedAt);

        ModelMessage message = response.Messages.Single();
        Assert.Equal("12345", message.MessageId);
        Assert.Equal(new ModelRole("human"), message.Role);
        Assert.Equal("Someone", message.AuthorName);
        Assert.Null(message.AdditionalProperties);

        Assert.NotNull(response.AdditionalProperties);
        Assert.Equal(2, response.AdditionalProperties.Count);
        Assert.Equal("b", response.AdditionalProperties["a"]);
        Assert.Equal("d", response.AdditionalProperties["c"]);

        Assert.Equal("Hello, world!", response.Text);
    }

    public static IEnumerable<object[]> ToAgentRunResponseCoalescesVariousSequenceAndGapLengthsMemberData()
    {
        foreach (bool useAsync in new[] { false, true })
        {
            for (int numSequences = 1; numSequences <= 3; numSequences++)
            {
                for (int sequenceLength = 1; sequenceLength <= 3; sequenceLength++)
                {
                    for (int gapLength = 1; gapLength <= 3; gapLength++)
                    {
                        foreach (bool gapBeginningEnd in new[] { false, true })
                        {
                            yield return new object[] { useAsync, numSequences, sequenceLength, gapLength, false };
                        }
                    }
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(ToAgentRunResponseCoalescesVariousSequenceAndGapLengthsMemberData))]
    public async Task ToAgentRunResponseCoalescesVariousSequenceAndGapLengthsAsync(bool useAsync, int numSequences, int sequenceLength, int gapLength, bool gapBeginningEnd)
    {
        List<AgentRunResponseUpdate> updates = [];

        List<string> expected = [];

        if (gapBeginningEnd)
        {
            AddGap();
        }

        for (int sequenceNum = 0; sequenceNum < numSequences; sequenceNum++)
        {
            StringBuilder sb = new();
            for (int i = 0; i < sequenceLength; i++)
            {
                string text = $"{(char)('A' + sequenceNum)}{i}";
                updates.Add(new(null, text));
                sb.Append(text);
            }

            expected.Add(sb.ToString());

            if (sequenceNum < numSequences - 1)
            {
                AddGap();
            }
        }

        if (gapBeginningEnd)
        {
            AddGap();
        }

        void AddGap()
        {
            for (int i = 0; i < gapLength; i++)
            {
                updates.Add(new() { Contents = [new TextModelContent("data:image/png;base64,aGVsbG8=")] });
            }
        }

        AgentRunResponse response = useAsync ? await YieldAsync(updates).ToAgentRunResponseAsync() : updates.ToAgentRunResponse();
        Assert.NotNull(response);

        ModelMessage message = response.Messages.Single();
        Assert.NotNull(message);

        Assert.Equal(expected.Count + (gapLength * ((numSequences - 1) + (gapBeginningEnd ? 2 : 0))), message.Contents.Count);

        TextModelContent[] contents = message.Contents.OfType<TextModelContent>().ToArray();
        Assert.Equal(expected.Count, contents.Length);
        for (int i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i], contents[i].Text);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ToAgentRunResponseCoalescesTextModelContentAndTextReasoningModelContentSeparatelyAsync(bool useAsync)
    {
        AgentRunResponseUpdate[] updates =
        [
            new(null, "A"),
            new(null, "B"),
            new(null, "C"),
            new() { Contents = [new TextReasoningModelContent("D")] },
            new() { Contents = [new TextReasoningModelContent("E")] },
            new() { Contents = [new TextReasoningModelContent("F")] },
            new(null, "G"),
            new(null, "H"),
            new() { Contents = [new TextReasoningModelContent("I")] },
            new() { Contents = [new TextReasoningModelContent("J")] },
            new(null, "K"),
            new() { Contents = [new TextReasoningModelContent("L")] },
            new(null, "M"),
            new(null, "N"),
            new() { Contents = [new TextReasoningModelContent("O")] },
            new() { Contents = [new TextReasoningModelContent("P")] },
        ];

        AgentRunResponse response = useAsync ? await YieldAsync(updates).ToAgentRunResponseAsync() : updates.ToAgentRunResponse();
        ModelMessage message = Assert.Single(response.Messages);
        Assert.Equal(8, message.Contents.Count);
        Assert.Equal("ABC", Assert.IsType<TextModelContent>(message.Contents[0]).Text);
        Assert.Equal("DEF", Assert.IsType<TextReasoningModelContent>(message.Contents[1]).Text);
        Assert.Equal("GH", Assert.IsType<TextModelContent>(message.Contents[2]).Text);
        Assert.Equal("IJ", Assert.IsType<TextReasoningModelContent>(message.Contents[3]).Text);
        Assert.Equal("K", Assert.IsType<TextModelContent>(message.Contents[4]).Text);
        Assert.Equal("L", Assert.IsType<TextReasoningModelContent>(message.Contents[5]).Text);
        Assert.Equal("MN", Assert.IsType<TextModelContent>(message.Contents[6]).Text);
        Assert.Equal("OP", Assert.IsType<TextReasoningModelContent>(message.Contents[7]).Text);
    }

    [Fact]
    public async Task ToAgentRunResponseUsesContentExtractedFromContentsAsync()
    {
        AgentRunResponseUpdate[] updates =
        [
            new(null, "Hello, "),
            new(null, "world!"),
            new() { Contents = [new UsageModelContent(new() { TotalTokenCount = 42 })] },
        ];

        AgentRunResponse response = await YieldAsync(updates).ToAgentRunResponseAsync();

        Assert.NotNull(response);

        Assert.NotNull(response.Usage);
        Assert.Equal(42, response.Usage.TotalTokenCount);

        Assert.Equal("Hello, world!", Assert.IsType<TextModelContent>(Assert.Single(Assert.Single(response.Messages).Contents)).Text);
    }

    private static async IAsyncEnumerable<AgentRunResponseUpdate> YieldAsync(IEnumerable<AgentRunResponseUpdate> updates)
    {
        foreach (AgentRunResponseUpdate update in updates)
        {
            await Task.Yield();
            yield return update;
        }
    }
}
