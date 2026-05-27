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
<<<<<<< HEAD
    private readonly IOmronFinsTransport _transport;
    private readonly OmronFinsFrameBuilder _frameBuilder;
    private readonly OmronFinsResponseParser _responseParser;
=======
    private IOmronFinsTransport? _transport;
>>>>>>> main

    /// <summary>
    /// Initializes a new instance of the <see cref="OmronFinsPlcClient"/> class.
    /// </summary>
    /// <param name="options">The FINS connection options.</param>
    public OmronFinsPlcClient(OmronFinsConnectionOptions options)
<<<<<<< HEAD
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
=======
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OmronFinsPlcClient"/> class with a custom transport.
    /// </summary>
    /// <param name="options">The FINS connection options.</param>
    /// <param name="transport">The custom transport.</param>
    public OmronFinsPlcClient(OmronFinsConnectionOptions options, IOmronFinsTransport transport)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
    }

    /// <inheritdoc />
    protected override async Task<PlcResult> ConnectCoreAsync(CancellationToken cancellationToken)
    {
        _transport ??= CreateTransport(_options);
        await _transport.OpenAsync(cancellationToken).ConfigureAwait(false);
        return PlcResult.Success();
    }

    /// <inheritdoc />
    protected override async Task<PlcResult> DisconnectCoreAsync(CancellationToken cancellationToken)
    {
        if (_transport is not null)
        {
            await _transport.CloseAsync(cancellationToken).ConfigureAwait(false);
        }

        return PlcResult.Success();
>>>>>>> main
    }

    /// <inheritdoc />
    protected override async Task<PlcResult<bool[]>> ReadBitsCoreAsync(
        PlcAddress address,
        int count,
        CancellationToken cancellationToken)
    {
<<<<<<< HEAD
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
=======
        if (_transport is null)
        {
            return PlcResult<bool[]>.Failure("FINS transport is not connected.");
        }

        var request = OmronFinsFrameBuilder.BuildMemoryAreaRead(_options, address, count, true);
        var response = await _transport.SendAndReceiveAsync(request, cancellationToken).ConfigureAwait(false);
        var payload = OmronFinsResponseParser.ExtractPayload(response);
        return PlcResult<bool[]>.Success(OmronFinsResponseParser.ParseBits(payload));
>>>>>>> main
    }

    /// <inheritdoc />
    protected override async Task<PlcResult<short[]>> ReadWordsCoreAsync(
        PlcAddress address,
        int count,
        CancellationToken cancellationToken)
    {
<<<<<<< HEAD
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
=======
        if (_transport is null)
        {
            return PlcResult<short[]>.Failure("FINS transport is not connected.");
        }

        var request = OmronFinsFrameBuilder.BuildMemoryAreaRead(_options, address, count, false);
        var response = await _transport.SendAndReceiveAsync(request, cancellationToken).ConfigureAwait(false);
        var payload = OmronFinsResponseParser.ExtractPayload(response);
        return PlcResult<short[]>.Success(OmronFinsResponseParser.ParseWords(payload));
>>>>>>> main
    }

    /// <inheritdoc />
    protected override async Task<PlcResult> WriteBitsCoreAsync(
        PlcAddress address,
        IReadOnlyList<bool> values,
        CancellationToken cancellationToken)
    {
<<<<<<< HEAD
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
=======
        if (_transport is null)
        {
            return PlcResult.Failure("FINS transport is not connected.");
        }

        var request = OmronFinsFrameBuilder.BuildMemoryAreaWriteBits(_options, address, values);
        var response = await _transport.SendAndReceiveAsync(request, cancellationToken).ConfigureAwait(false);
        _ = OmronFinsResponseParser.ExtractPayload(response);
        return PlcResult.Success();
>>>>>>> main
    }

    /// <inheritdoc />
    protected override async Task<PlcResult> WriteWordsCoreAsync(
        PlcAddress address,
        IReadOnlyList<short> values,
        CancellationToken cancellationToken)
    {
<<<<<<< HEAD
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
=======
        if (_transport is null)
        {
            return PlcResult.Failure("FINS transport is not connected.");
        }

        var request = OmronFinsFrameBuilder.BuildMemoryAreaWriteWords(_options, address, values);
        var response = await _transport.SendAndReceiveAsync(request, cancellationToken).ConfigureAwait(false);
        _ = OmronFinsResponseParser.ExtractPayload(response);
        return PlcResult.Success();
>>>>>>> main
    }

    private static IOmronFinsTransport CreateTransport(OmronFinsConnectionOptions options)
    {
<<<<<<< HEAD
        ArgumentNullException.ThrowIfNull(options);

=======
>>>>>>> main
        return options.TransportType switch
        {
            OmronFinsTransportType.Tcp => new TcpOmronFinsTransport(options),
            OmronFinsTransportType.Udp => new UdpOmronFinsTransport(options),
<<<<<<< HEAD
            _ => throw new ArgumentOutOfRangeException(nameof(options), options.TransportType, "Unsupported Omron FINS transport type.")
=======
            _ => throw new NotSupportedException($"Unsupported FINS transport type: {options.TransportType}.")
>>>>>>> main
        };
    }
}
