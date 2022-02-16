using System.Net;
using System.Net.NetworkInformation;
using System.Linq;

namespace ChatServer.Tcp
{
    internal class NetworkInterfaceView
    {
        public NetworkInterfaceView(NetworkInterface ni)
        {
            Name = ni.Name;
            Address = ni.GetIPProperties().UnicastAddresses.First(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Address;
            PhysicalAddress = ni.GetPhysicalAddress();
            Type = ni.NetworkInterfaceType;
            Id = ni.Id;
        }
        public NetworkInterfaceView(string name, IPAddress address)
        {
            Name = name;
            Address = address;
        }
        public string Name { get;}
        public IPAddress Address { get; }
        public PhysicalAddress PhysicalAddress { get; }
        public NetworkInterfaceType Type { get; } = NetworkInterfaceType.Unknown;
        public string Id { get; set; }
        public string HostName { get; }

        public override string ToString()
        {
            return $"{Name} {Address} {Type}";
        }
    }
}
