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
    private IOmronFinsTransport? _transport;

    /// <summary>
    /// Initializes a new instance of the <see cref="OmronFinsPlcClient"/> class.
    /// </summary>
    /// <param name="options">The FINS connection options.</param>
    public OmronFinsPlcClient(OmronFinsConnectionOptions options)
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
    }

    /// <inheritdoc />
    protected override async Task<PlcResult<bool[]>> ReadBitsCoreAsync(
        PlcAddress address,
        int count,
        CancellationToken cancellationToken)
    {
        if (_transport is null)
        {
            return PlcResult<bool[]>.Failure("FINS transport is not connected.");
        }

        var request = OmronFinsFrameBuilder.BuildMemoryAreaRead(_options, address, count, true);
        var response = await _transport.SendAndReceiveAsync(request, cancellationToken).ConfigureAwait(false);
        var payload = OmronFinsResponseParser.ExtractPayload(response);
        return PlcResult<bool[]>.Success(OmronFinsResponseParser.ParseBits(payload));
    }

    /// <inheritdoc />
    protected override async Task<PlcResult<short[]>> ReadWordsCoreAsync(
        PlcAddress address,
        int count,
        CancellationToken cancellationToken)
    {
        if (_transport is null)
        {
            return PlcResult<short[]>.Failure("FINS transport is not connected.");
        }

        var request = OmronFinsFrameBuilder.BuildMemoryAreaRead(_options, address, count, false);
        var response = await _transport.SendAndReceiveAsync(request, cancellationToken).ConfigureAwait(false);
        var payload = OmronFinsResponseParser.ExtractPayload(response);
        return PlcResult<short[]>.Success(OmronFinsResponseParser.ParseWords(payload));
    }

    /// <inheritdoc />
    protected override async Task<PlcResult> WriteBitsCoreAsync(
        PlcAddress address,
        IReadOnlyList<bool> values,
        CancellationToken cancellationToken)
    {
        if (_transport is null)
        {
            return PlcResult.Failure("FINS transport is not connected.");
        }

        var request = OmronFinsFrameBuilder.BuildMemoryAreaWriteBits(_options, address, values);
        var response = await _transport.SendAndReceiveAsync(request, cancellationToken).ConfigureAwait(false);
        _ = OmronFinsResponseParser.ExtractPayload(response);
        return PlcResult.Success();
    }

    /// <inheritdoc />
    protected override async Task<PlcResult> WriteWordsCoreAsync(
        PlcAddress address,
        IReadOnlyList<short> values,
        CancellationToken cancellationToken)
    {
        if (_transport is null)
        {
            return PlcResult.Failure("FINS transport is not connected.");
        }

        var request = OmronFinsFrameBuilder.BuildMemoryAreaWriteWords(_options, address, values);
        var response = await _transport.SendAndReceiveAsync(request, cancellationToken).ConfigureAwait(false);
        _ = OmronFinsResponseParser.ExtractPayload(response);
        return PlcResult.Success();
    }

    private static IOmronFinsTransport CreateTransport(OmronFinsConnectionOptions options)
    {
        return options.TransportType switch
        {
            OmronFinsTransportType.Tcp => new TcpOmronFinsTransport(options),
            OmronFinsTransportType.Udp => new UdpOmronFinsTransport(options),
            _ => throw new NotSupportedException($"Unsupported FINS transport type: {options.TransportType}.")
        };
    }
}
