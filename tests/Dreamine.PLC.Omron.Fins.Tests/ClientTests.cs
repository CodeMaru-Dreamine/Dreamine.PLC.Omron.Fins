using Dreamine.PLC.Abstractions.Devices;
using Dreamine.PLC.Omron.Fins.Clients;
using Dreamine.PLC.Omron.Fins.Options;
using Dreamine.PLC.Omron.Fins.Protocol;
using Dreamine.PLC.Omron.Fins.Transport;

namespace Dreamine.PLC.Omron.Fins.Tests;

public sealed class ClientTests
{
    [Fact]
    public void Constructor_ExposesOptionsAndRejectsNullDependencies()
    {
        var options = new OmronFinsConnectionOptions();
        var transport = new FakeOmronFinsTransport();
        var client = CreateClient(options, transport);

        Assert.Same(options, client.Options);
        Assert.Throws<ArgumentNullException>(() => new OmronFinsPlcClient(null!));
        Assert.Throws<ArgumentNullException>(
            () => new OmronFinsPlcClient(options, null!, new OmronFinsFrameBuilder(), new OmronFinsResponseParser()));
    }

    [Fact]
    public void Options_HaveSafeLocalDefaults()
    {
        var options = new OmronFinsConnectionOptions();

        Assert.Equal("127.0.0.1", options.Host);
        Assert.Equal(9600, options.Port);
        Assert.Equal(OmronFinsTransportType.Udp, options.TransportType);
        Assert.Equal(3000, options.ConnectTimeoutMs);
        Assert.Equal(3000, options.ReceiveTimeoutMs);
        Assert.Equal(1, options.RetryCount);
        Assert.Equal((byte)1, options.SourceNode);
    }

    [Fact]
    public async Task FakeTransport_ConnectsQueuesResponsesAndDisposes()
    {
        var transport = new FakeOmronFinsTransport();
        transport.EnqueueResponse([1, 2, 3]);

        Assert.True((await transport.ConnectAsync()).IsSuccess);
        var response = await transport.SendAndReceiveAsync([9, 8], 100, 0);
        Assert.True(response.IsSuccess);
        Assert.Equal(new byte[] { 1, 2, 3 }, response.Value);
        Assert.Equal(new byte[] { 9, 8 }, transport.SentRequests[0]);
        Assert.False((await transport.SendAndReceiveAsync([7], 100, 0)).IsSuccess);
        Assert.True((await transport.DisconnectAsync()).IsSuccess);

        await transport.DisposeAsync();
        Assert.False(transport.IsConnected);
    }

    [Fact]
    public async Task Client_ReadsAndWritesThroughTransport()
    {
        var transport = new FakeOmronFinsTransport();
        transport.EnqueueResponse(ProtocolTests.BuildResponse(0, 1, 0, 1));
        transport.EnqueueResponse(ProtocolTests.BuildResponse(0, 0x00, 0x2A, 0xFF, 0xFE));
        transport.EnqueueResponse(ProtocolTests.BuildResponse(0));
        transport.EnqueueResponse(ProtocolTests.BuildResponse(0));
        await using var client = CreateClient(new OmronFinsConnectionOptions(), transport);

        Assert.True((await client.ConnectAsync()).IsSuccess);
        var bits = await client.ReadBitsAsync(new PlcAddress(PlcDeviceType.W, 10), 3);
        var words = await client.ReadWordsAsync(new PlcAddress(PlcDeviceType.D, 20), 2);
        var writeBits = await client.WriteBitsAsync(new PlcAddress(PlcDeviceType.M, 30), [true, false]);
        var writeWords = await client.WriteWordsAsync(new PlcAddress(PlcDeviceType.D, 40), [(short)7, (short)-8]);

        Assert.Equal(new[] { true, false, true }, bits.Value);
        Assert.Equal(new short[] { 42, -2 }, words.Value);
        Assert.True(writeBits.IsSuccess);
        Assert.True(writeWords.IsSuccess);
        Assert.Equal(4, transport.SentRequests.Count);
        Assert.True((await client.DisconnectAsync()).IsSuccess);
    }

    [Fact]
    public async Task Client_ConvertsTransportAndProtocolErrorsToFailures()
    {
        var transport = new FakeOmronFinsTransport();
        await using var client = CreateClient(new OmronFinsConnectionOptions(), transport);
        Assert.True((await client.ConnectAsync()).IsSuccess);

        var missing = await client.ReadWordsAsync(new PlcAddress(PlcDeviceType.D, 0), 1);
        transport.EnqueueResponse(ProtocolTests.BuildResponse(0x2101));
        var endCode = await client.ReadBitsAsync(new PlcAddress(PlcDeviceType.W, 0), 1);
        transport.EnqueueResponse(ProtocolTests.BuildResponse(0));
        var shortPayload = await client.ReadWordsAsync(new PlcAddress(PlcDeviceType.D, 0), 1);
        var invalidAddress = await client.ReadWordsAsync(new PlcAddress(PlcDeviceType.Unknown, 0), 1);

        Assert.False(missing.IsSuccess);
        Assert.False(endCode.IsSuccess);
        Assert.Equal(0x2101, endCode.ErrorCode);
        Assert.False(shortPayload.IsSuccess);
        Assert.False(invalidAddress.IsSuccess);
    }

    [Fact]
    public async Task Client_RejectsRequestsBeforeConnectAndDisposesTransport()
    {
        var transport = new FakeOmronFinsTransport();
        var client = CreateClient(new OmronFinsConnectionOptions(), transport);

        var result = await client.ReadWordsAsync(new PlcAddress(PlcDeviceType.D, 0), 1);
        await client.DisposeAsync();
        await client.DisposeAsync();

        Assert.False(result.IsSuccess);
        Assert.Contains("not connected", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(transport.IsConnected);
    }

    [Fact]
    public void Constructor_RejectsUnknownTransport()
    {
        var options = new OmronFinsConnectionOptions
        {
            TransportType = (OmronFinsTransportType)99
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => new OmronFinsPlcClient(options));
    }

    private static OmronFinsPlcClient CreateClient(
        OmronFinsConnectionOptions options,
        FakeOmronFinsTransport transport)
    {
        return new OmronFinsPlcClient(
            options,
            transport,
            new OmronFinsFrameBuilder(),
            new OmronFinsResponseParser());
    }
}
