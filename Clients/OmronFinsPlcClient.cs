using Dreamine.PLC.Abstractions.Devices;
using Dreamine.PLC.Abstractions.Results;
using Dreamine.PLC.Core.Clients;
using Dreamine.PLC.Omron.Fins.Options;
using Dreamine.PLC.Omron.Fins.Protocol;
using Dreamine.PLC.Omron.Fins.Transport;

namespace Dreamine.PLC.Omron.Fins.Clients;

/// <summary>
/// Provides an Omron FINS PLC client implementation for the Dreamine PLC stack.
/// </summary>
public sealed class OmronFinsPlcClient : PlcClientBase
{
    private readonly OmronFinsConnectionOptions _options;
    private readonly IOmronFinsTransport _transport;
    private readonly OmronFinsFrameBuilder _frameBuilder;
    private readonly OmronFinsResponseParser _responseParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="OmronFinsPlcClient"/> class.
    /// </summary>
    /// <param name="options">The FINS connection options.</param>
    public OmronFinsPlcClient(OmronFinsConnectionOptions options)
        : this(options, CreateTransport(options), new OmronFinsFrameBuilder(), new OmronFinsResponseParser())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OmronFinsPlcClient"/> class.
    /// </summary>
    /// <param name="options">The FINS connection options.</param>
    /// <param name="transport">The FINS transport.</param>
    /// <param name="frameBuilder">The FINS frame builder.</param>
    /// <param name="responseParser">The FINS response parser.</param>
    public OmronFinsPlcClient(
        OmronFinsConnectionOptions options,
        IOmronFinsTransport transport,
        OmronFinsFrameBuilder frameBuilder,
        OmronFinsResponseParser responseParser)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _frameBuilder = frameBuilder ?? throw new ArgumentNullException(nameof(frameBuilder));
        _responseParser = responseParser ?? throw new ArgumentNullException(nameof(responseParser));
    }

    /// <summary>
    /// Gets the Omron FINS connection options.
    /// </summary>
    public OmronFinsConnectionOptions Options => _options;

    /// <inheritdoc />
    protected override Task<PlcResult> ConnectCoreAsync(CancellationToken cancellationToken)
    {
        return _transport.ConnectAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override Task<PlcResult> DisconnectCoreAsync(CancellationToken cancellationToken)
    {
        return _transport.DisconnectAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<PlcResult<bool[]>> ReadBitsCoreAsync(
        PlcAddress address,
        int count,
        CancellationToken cancellationToken)
    {
        byte[] request;
        try
        {
            request = _frameBuilder.BuildMemoryAreaRead(_options, address, count, bitAccess: true);
        }
        catch (Exception ex)
        {
            return PlcResult<bool[]>.Failure(ex.Message);
        }

        var responseResult = await _transport.SendAndReceiveAsync(
            request,
            _options.ReceiveTimeoutMs,
            _options.RetryCount,
            cancellationToken).ConfigureAwait(false);

        if (!responseResult.IsSuccess || responseResult.Value is null)
        {
            return PlcResult<bool[]>.Failure(responseResult.Message ?? "Failed to receive FINS bit read response.", responseResult.ErrorCode);
        }

        var payloadResult = _responseParser.ExtractPayload(responseResult.Value);
        if (!payloadResult.IsSuccess || payloadResult.Value is null)
        {
            return PlcResult<bool[]>.Failure(payloadResult.Message ?? "Failed to parse FINS bit read payload.", payloadResult.ErrorCode);
        }

        return _responseParser.ParseBits(payloadResult.Value, count);
    }

    /// <inheritdoc />
    protected override async Task<PlcResult<short[]>> ReadWordsCoreAsync(
        PlcAddress address,
        int count,
        CancellationToken cancellationToken)
    {
        byte[] request;
        try
        {
            request = _frameBuilder.BuildMemoryAreaRead(_options, address, count, bitAccess: false);
        }
        catch (Exception ex)
        {
            return PlcResult<short[]>.Failure(ex.Message);
        }

        var responseResult = await _transport.SendAndReceiveAsync(
            request,
            _options.ReceiveTimeoutMs,
            _options.RetryCount,
            cancellationToken).ConfigureAwait(false);

        if (!responseResult.IsSuccess || responseResult.Value is null)
        {
            return PlcResult<short[]>.Failure(responseResult.Message ?? "Failed to receive FINS word read response.", responseResult.ErrorCode);
        }

        var payloadResult = _responseParser.ExtractPayload(responseResult.Value);
        if (!payloadResult.IsSuccess || payloadResult.Value is null)
        {
            return PlcResult<short[]>.Failure(payloadResult.Message ?? "Failed to parse FINS word read payload.", payloadResult.ErrorCode);
        }

        return _responseParser.ParseWords(payloadResult.Value, count);
    }

    /// <inheritdoc />
    protected override async Task<PlcResult> WriteBitsCoreAsync(
        PlcAddress address,
        IReadOnlyList<bool> values,
        CancellationToken cancellationToken)
    {
        byte[] request;
        try
        {
            request = _frameBuilder.BuildMemoryAreaWriteBits(_options, address, values);
        }
        catch (Exception ex)
        {
            return PlcResult.Failure(ex.Message);
        }

        var responseResult = await _transport.SendAndReceiveAsync(
            request,
            _options.ReceiveTimeoutMs,
            _options.RetryCount,
            cancellationToken).ConfigureAwait(false);

        if (!responseResult.IsSuccess || responseResult.Value is null)
        {
            return PlcResult.Failure(responseResult.Message ?? "Failed to receive FINS bit write response.", responseResult.ErrorCode);
        }

        var payloadResult = _responseParser.ExtractPayload(responseResult.Value);
        return payloadResult.IsSuccess
            ? PlcResult.Success()
            : PlcResult.Failure(payloadResult.Message ?? "Failed to parse FINS bit write response.", payloadResult.ErrorCode);
    }

    /// <inheritdoc />
    protected override async Task<PlcResult> WriteWordsCoreAsync(
        PlcAddress address,
        IReadOnlyList<short> values,
        CancellationToken cancellationToken)
    {
        byte[] request;
        try
        {
            request = _frameBuilder.BuildMemoryAreaWriteWords(_options, address, values);
        }
        catch (Exception ex)
        {
            return PlcResult.Failure(ex.Message);
        }

        var responseResult = await _transport.SendAndReceiveAsync(
            request,
            _options.ReceiveTimeoutMs,
            _options.RetryCount,
            cancellationToken).ConfigureAwait(false);

        if (!responseResult.IsSuccess || responseResult.Value is null)
        {
            return PlcResult.Failure(responseResult.Message ?? "Failed to receive FINS word write response.", responseResult.ErrorCode);
        }

        var payloadResult = _responseParser.ExtractPayload(responseResult.Value);
        return payloadResult.IsSuccess
            ? PlcResult.Success()
            : PlcResult.Failure(payloadResult.Message ?? "Failed to parse FINS word write response.", payloadResult.ErrorCode);
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync().ConfigureAwait(false);
        await _transport.DisposeAsync().ConfigureAwait(false);
    }

    private static IOmronFinsTransport CreateTransport(OmronFinsConnectionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.TransportType switch
        {
            OmronFinsTransportType.Tcp => new TcpOmronFinsTransport(options),
            OmronFinsTransportType.Udp => new UdpOmronFinsTransport(options),
            _ => throw new ArgumentOutOfRangeException(nameof(options), options.TransportType, "Unsupported Omron FINS transport type.")
        };
    }
}
