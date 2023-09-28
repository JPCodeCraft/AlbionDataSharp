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
    internal class NetworkListener : BackgroundService
    {
        private IPhotonReceiver receiver;
        CaptureDeviceList devices;
        Uploader uploader;
        PlayerState playerState;

        public NetworkListener(Uploader uploader, PlayerState playerState)
        {
            this.uploader = uploader;
            this.playerState = playerState;
        }

        private async Task Cleanup()
        {
            // Close network devices, flush logs, etc.
            if (devices is not null)
            {
                foreach (var device in devices)
                {
                    device.StopCapture();
                    device.Close();
                    Log.Debug("Close... {Device}", device.Description);
                }
            }
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ReceiverBuilder builder = ReceiverBuilder.Create();

            //ADD HANDLERS HERE
            //RESPONSE
            builder.AddResponseHandler(new AuctionGetOffersResponseHandler(uploader, playerState));
            builder.AddResponseHandler(new AuctionGetRequestsResponseHandler(uploader, playerState));
            builder.AddResponseHandler(new AuctionGetItemAverageStatsResponseHandler(uploader, playerState));
            builder.AddResponseHandler(new JoinResponseHandler(playerState));
            builder.AddResponseHandler(new AuctionGetGoldAverageStatsResponseHandler(uploader));
            //REQUEST
            builder.AddRequestHandler(new AuctionGetItemAverageStatsRequestHandler(playerState));

            receiver = builder.Build();

            Log.Debug("Starting...");

            devices = CaptureDeviceList.New();

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

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await Cleanup();
            Log.Information("Stopped {type}!", nameof(NetworkListener));
            await base.StopAsync(stoppingToken);
        }
        private void PacketHandler(object sender, PacketCapture e)
        {
            try
            {
                UdpPacket packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data).Extract<UdpPacket>();
                if (packet != null)
                {
                    //if (PlayerState.Server == Servers.unkown)
                    {
                        var srcIp = (packet.ParentPacket as IPv4Packet)?.SourceAddress?.ToString();
                        if (srcIp == null || string.IsNullOrEmpty(srcIp))
                        {
                            playerState.AlbionServer = AlbionServer.Unknown;
                        }
                        else if (srcIp.Contains("5.188.125."))
                        {
                            playerState.AlbionServer = AlbionServer.West;
                        }
                        else if (srcIp!.Contains("5.45.187."))
                        {
                            playerState.AlbionServer = AlbionServer.East;
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
