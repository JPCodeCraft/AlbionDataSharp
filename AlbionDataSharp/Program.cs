using Albion.Network;
using AlbionDataSharp.Handlers;
using PacketDotNet;
using SharpPcap;
using System.Runtime.InteropServices;

namespace AlbionDataSharp
{
    public class Program
    {
        private static IPhotonReceiver receiver;

        private static void Main(string[] args)
        {
            ReceiverBuilder builder = ReceiverBuilder.Create();

            //ADD HANDLERS HERE
            builder.AddResponseHandler(new AuctionGetOffersResponseHandler());

            receiver = builder.Build();

            Console.WriteLine("Start");

            CaptureDeviceList devices = CaptureDeviceList.New();

            foreach (var device in devices)
            {
                new Thread(() =>
                {
                    Console.WriteLine($"Open... {device.Description}");

                    device.OnPacketArrival += new PacketArrivalEventHandler(PacketHandler);
                    device.Open(DeviceModes.Promiscuous, 1000);
                    device.StartCapture();
                })
                .Start();
            }

            Console.Read();
        }

        private static void PacketHandler(object sender, PacketCapture e)
        {
            UdpPacket packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data).Extract<UdpPacket>();
            if (packet != null && (packet.SourcePort == 5056 || packet.DestinationPort == 5056))
            {
                receiver.ReceivePacket(packet.PayloadData);
            }
        }
    }
}

