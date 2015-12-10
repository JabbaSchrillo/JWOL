using System.Net.Sockets;
using System.Net;
using System;

namespace JWOL
{
    class Program
    {
        /// <summary>
        /// Converts an mac string to an byte array
        /// </summary>
        /// <param name="mac">The physical MAC address to convert. The string can have on of the fallowing formats.
        /// <list type="bullet">
        /// <item>
        /// <description>AA:BB:CC:DD:EE:FF</description>
        /// </item>
        /// <item>
        /// <description>AA-BB-CC-DD-EE-FF</description>
        /// </item>
        /// <item>
        /// <description>AABB.CCDD.EEFF</description>
        /// </item>
        /// <item>
        /// <description>AABBCCDDEEFF</description>
        /// </item>
        /// </list>
        /// </param>
        /// <exception cref="ArgumentNullException">thrown if <paramref name="mac"/> is null </exception>
        /// <exception cref="ArgumentException">thrown if <paramref name="mac"/> is not valid</exception>
        public static byte[] GetMacArray(string mac)
        {
            if (string.IsNullOrEmpty(mac)) throw new ArgumentNullException("mac");
            byte[] ret = new byte[6];
            try
            {
                string[] tmp = mac.Split(':', '-');
                if (tmp.Length != 6)
                {
                    tmp = mac.Split('.');
                    if (tmp.Length == 3)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            ret[i * 2] = byte.Parse(tmp[i].Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                            ret[i * 2 + 1] = byte.Parse(tmp[i].Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                        }
                    }
                    else
                        for (int i = 0; i < 12; i += 2)
                            ret[i / 2] = byte.Parse(mac.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
                }
                else
                    for (int i = 0; i < 6; i++)
                        ret[i] = byte.Parse(tmp[i], System.Globalization.NumberStyles.HexNumber);
            }
            catch
            {
                throw new ArgumentException("Argument doesn't have the correct format: " + mac, "mac");
            }
            return ret;
        }

        /// <summary>
        /// Sends a Wake-On-Lan packet to the specified MAC address.
        /// </summary>
        /// <param name="mac">Physical MAC address to send WOL packet to.</param>
        private static void WakeOnLan(byte[] mac)
        {
            // WOL packet is sent over UDP 255.255.255.0:40000.
            UdpClient client = new UdpClient();
            client.Connect(IPAddress.Broadcast, 40000);

            // WOL packet contains a 6-bytes trailer and 16 times a 6-bytes sequence containing the MAC address.
            byte[] packet = new byte[17 * 6];

            // Trailer of 6 times 0xFF.
            for (int i = 0; i < 6; i++)
                packet[i] = 0xFF;

            // Body of magic packet contains 16 times the MAC address.
            for (int i = 1; i <= 16; i++)
                for (int j = 0; j < 6; j++)
                    packet[i * 6 + j] = mac[j];

            // Send WOL packet.
            client.Send(packet, packet.Length);
        }


        static void Main(string[] args)
        {
            WakeOnLan(GetMacArray("E0-CB-4E-9B-02-69"));
        }
    }
}
