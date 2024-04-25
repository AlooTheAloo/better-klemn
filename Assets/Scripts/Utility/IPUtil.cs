using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;

// Pris � partir de
// https://stackoverflow.com/questions/51975799/how-to-get-ip-address-of-device-in-unity-2018

public struct InterfaceInfo
{
    public string Name { get; set; }
    public string IP { get; set; }
}

public class IPUtil
{
    public static InterfaceInfo? GetIP(ADDRESSFAM Addfam)
    {
        InterfaceInfo info = new();
        //Return null if ADDRESSFAM is Ipv6 but Os does not support it
        if (Addfam == ADDRESSFAM.IPv6 && !Socket.OSSupportsIPv6)
        {
            return null;
        }

        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
            NetworkInterfaceType _type1 = NetworkInterfaceType.Ethernet;
            NetworkInterfaceType _type2 = NetworkInterfaceType.GigabitEthernet;


            if (((item.NetworkInterfaceType == _type1) || (item.NetworkInterfaceType == _type2)) && item.OperationalStatus == OperationalStatus.Up && !item.Description.ToLower().Contains("virtual"))
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    
                    //IPv4
                    if (Addfam == ADDRESSFAM.IPv4)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            info.Name = item.Name;
                            info.IP = ip.Address.ToString();
                        }
                    }

                    //IPv6
                    else if (Addfam == ADDRESSFAM.IPv6)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            info.Name = item.Name;
                            info.IP = ip.Address.ToString();
                        }
                    }
                }
            }
        }
        return info;
    }
}

public enum ADDRESSFAM
{
    IPv4, IPv6
}