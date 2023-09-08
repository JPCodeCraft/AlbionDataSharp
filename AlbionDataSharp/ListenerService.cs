using Albion.Network;
using AlbionDataSharp.Handlers;
using AlbionDataSharp.Nats;
using AlbionDataSharp.Status;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PacketDotNet;
using SharpPcap;

namespace AlbionDataSharp
{
    public class ListenerService : IHostedService
    {
        private readonly ILogger<ListenerService> _logger;
        private readonly INatsManager _natsManager;
        private readonly IPlayerStatus _playerStatus;

        private IPhotonReceiver receiver;

        public ListenerService(ILogger<ListenerService> logger, IPlayerStatus playerStatus, INatsManager natsManager)
        {
            _logger = logger;
            _natsManager = natsManager;
            _playerStatus = playerStatus;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            ReceiverBuilder builder = ReceiverBuilder.Create();

            //ADD HANDLERS HERE
            builder.AddResponseHandler(new AuctionGetOffersResponseHandler(_natsManager));
            builder.AddResponseHandler(new AuctionGetRequestsResponseHandler(_natsManager));
            builder.AddResponseHandler(new AuctionGetItemAverageStatsResponseHandler(_natsManager));
            builder.AddResponseHandler(new JoinResponseHandler());
            builder.AddRequestHandler(new AuctionGetItemAverageStatsRequestHandler());

            receiver = builder.Build();

            _logger.LogInformation("Starting the photon receiver...");

            CaptureDeviceList devices = CaptureDeviceList.New();

            foreach (var device in devices)
            {
                new Thread(() =>
                {
                    _logger.LogInformation($"Open... {device.Description}");

                    device.OnPacketArrival += new PacketArrivalEventHandler(PacketHandler);
                    device.Open(new DeviceConfiguration
                    {
                        Mode = DeviceModes.MaxResponsiveness,
                        ReadTimeout = 5000
                    });
                    device.Filter = "(host 5.45.187 or host 5.188.125) and udp port 5056";
                    device.StartCapture();
                })
                .Start();
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        private void PacketHandler(object sender, PacketCapture e)
        {
            try
            {
                UdpPacket packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data).Extract<UdpPacket>();
                if (packet != null)
                {
                    var srcIp = (packet.ParentPacket as IPv4Packet)?.SourceAddress?.ToString();
                    if (srcIp == null || string.IsNullOrEmpty(srcIp))
                    {
                        _playerStatus.Server = Servers.Unknown;
                    }
                    else if (srcIp.Contains("5.188.125."))
                    {
                        _playerStatus.Server = Servers.West;
                    }
                    else if (srcIp!.Contains("5.45.187."))
                    {
                        _playerStatus.Server = Servers.East;
                    }
                    receiver.ReceivePacket(packet.PayloadData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

        }

    }
}
