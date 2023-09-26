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
        CaptureDeviceList devices;
        Uploader uploader;
        PlayerStatus playerStatus;

        public NetworkListener(Uploader uploader, PlayerStatus playerStatus)
        {
            AppDomain.CurrentDomain.ProcessExit += async (s, e) => await Cleanup();
            this.uploader = uploader;
            this.playerStatus = playerStatus;
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
                }
            }
            await Log.CloseAndFlushAsync();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            ReceiverBuilder builder = ReceiverBuilder.Create();

            //ADD HANDLERS HERE
            //RESPONSE
            builder.AddResponseHandler(new AuctionGetOffersResponseHandler(uploader, playerStatus));
            builder.AddResponseHandler(new AuctionGetRequestsResponseHandler(uploader, playerStatus));
            builder.AddResponseHandler(new AuctionGetItemAverageStatsResponseHandler(uploader, playerStatus));
            builder.AddResponseHandler(new JoinResponseHandler(playerStatus));
            builder.AddResponseHandler(new AuctionGetGoldAverageStatsResponseHandler(uploader));
            //REQUEST
            builder.AddRequestHandler(new AuctionGetItemAverageStatsRequestHandler(playerStatus));

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

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
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
                            playerStatus.AlbionServer = AlbionServer.Unknown;
                        }
                        else if (srcIp.Contains("5.188.125."))
                        {
                            playerStatus.AlbionServer = AlbionServer.West;
                        }
                        else if (srcIp!.Contains("5.45.187."))
                        {
                            playerStatus.AlbionServer = AlbionServer.East;
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
