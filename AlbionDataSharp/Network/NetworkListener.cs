using Albion.Network;
using AlbionDataSharp.Config;
using AlbionDataSharp.Network.Handlers;
using AlbionDataSharp.State;
using Microsoft.Extensions.Hosting;
using PacketDotNet;
using Serilog;
using SharpPcap;

namespace AlbionDataSharp.Network
{
    internal class NetworkListener : IHostedService
    {
        private IPhotonReceiver receiver;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            ReceiverBuilder builder = ReceiverBuilder.Create();

            //ADD HANDLERS HERE
            builder.AddResponseHandler(new AuctionGetOffersResponseHandler());
            builder.AddResponseHandler(new AuctionGetRequestsResponseHandler());
            builder.AddResponseHandler(new AuctionGetItemAverageStatsResponseHandler());
            builder.AddResponseHandler(new JoinResponseHandler());
            builder.AddRequestHandler(new AuctionGetItemAverageStatsRequestHandler());

            receiver = builder.Build();

            Log.Debug("Starting...");

            CaptureDeviceList devices = CaptureDeviceList.New();

            foreach (var device in devices)
            {
                new Thread(() =>
                {
                    Log.Debug("Open... {Device}", device.Description);

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

            Log.Information("Listening to Albion network packages!");

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Log.CloseAndFlushAsync();
        }
        private void PacketHandler(object sender, PacketCapture e)
        {
            try
            {
                UdpPacket packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data).Extract<UdpPacket>();
                if (packet != null)
                {
                    //if (PlayerStatus.Server == Servers.unkown)
                    {
                        var srcIp = (packet.ParentPacket as IPv4Packet)?.SourceAddress?.ToString();
                        if (srcIp == null || string.IsNullOrEmpty(srcIp))
                        {
                            PlayerStatus.Server = AlbionServer.Unknown;
                        }
                        else if (srcIp.Contains("5.188.125."))
                        {
                            PlayerStatus.Server = AlbionServer.West;
                        }
                        else if (srcIp!.Contains("5.45.187."))
                        {
                            PlayerStatus.Server = AlbionServer.East;
                        }
                    }
                    receiver.ReceivePacket(packet.PayloadData);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex.Message);
            }

        }

    }
}
