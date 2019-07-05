using UnityEngine;
using System.Collections;
using System.Net.NetworkInformation;
using System.Net;

using Data;

/// <summary>
/// Access device data
/// </summary>
public class DeviceUtil
{
    #region Device Properties
    private string ip = "";
    private string deviceUniqueIdentifier = "";
    private string deviceName = "";
    private string opSystemName = "";

    private Platform platform;
    #endregion

    private static DeviceUtil instance;

    public static DeviceUtil Instance
    {
        get
        {
            if ( instance == null )
            {
                instance = new DeviceUtil();
                instance.Initialize();
            }
            return instance;
        }
    }

    // Use this for initialization
    public void Initialize()
    {
        // Init ip address
#if ( UNITY_IOS || UNITY_EDITOR_OSX )
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        foreach ( NetworkInterface adapter in adapters )
        {
            if ( adapter.Supports( NetworkInterfaceComponent.IPv4 ) )
            {
                UnicastIPAddressInformationCollection uniCast = adapter.GetIPProperties().UnicastAddresses;
                if ( uniCast.Count > 0 )
                {
                    foreach ( UnicastIPAddressInformation uni in uniCast )
                    {
                        if ( uni.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork )
                        {
                            ip = uni.Address.ToString();
                        }
                    }
                }
            }
        }
#else
        // NetworkInterface.GetAllNetworkInterfaces in mono to andriod will throw a excption
        // Get ip 
        IPHostEntry ipEntry = Dns.GetHostEntry( System.Net.Dns.GetHostName() );
        for ( int i = 0; i < ipEntry.AddressList.Length; i++ )
        {
            if ( ipEntry.AddressList[i].AddressFamily.ToString() == "InterNetwork" )
            {
                ip = ipEntry.AddressList[i].ToString();
            }
        }
#endif

        deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
        deviceName = SystemInfo.deviceName;
        opSystemName = SystemInfo.operatingSystem;

        platform = Platform.editor;

        if ( Application.isEditor )
        {
#if UNITY_ANDROID
            platform =  Platform.android;
#elif UNITY_IPHONE
            platform = Platform.ios;
#endif
        }
        else
        {
            RuntimePlatform runtimePlatform = Application.platform;
            if ( runtimePlatform == RuntimePlatform.Android || opSystemName.Contains( "Android" ) )
            {
                platform = Platform.android;
            }
            else if ( runtimePlatform == RuntimePlatform.IPhonePlayer || opSystemName.Contains( "iPhone" ) )
            {
                platform = Platform.ios;
            }
        }
    }

    public byte GetPlatform()
    {
        return (byte)platform;
    }

    public string GetDeviceIP()
    {
        return ip;
    }

    public string GetDeviceUniqueIdentifier()
    {
        return deviceUniqueIdentifier;
    }

    public string GetDeviceName()
    {
        return deviceName;
    }

    public string GetOpSystemName()
    {
        return opSystemName;
    }

    /// <summary>
    /// Check if the network is connected
    /// </summary>
    /// <returns></returns>
    public static bool CheckNetworkState()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    /// <summary>
    /// Get Network State(0-3G/4G  1-WIFI)
    /// </summary>
    /// <returns></returns>
    public static int GetNetworkState()
    {
        if ( Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork )
            return 0;
        if ( Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork )
            return 1;
        return -1;
    }

    /// <summary>
    /// Get the battery value (the range is 0-100, -1 is error)
    /// </summary>
    /// <returns></returns>
    public static int GetBatteryValue()
    {
#if UNITY_EDITOR
        return 100;
#elif UNITY_STANDALONE_WIN
        return 100;
#elif UNITY_IOS
        return 100;
#elif UNITY_ANDROID
        try
        {
            string capacityString = System.IO.File.ReadAllText( "/sys/class/power_supply/battery/capacity" );

            return int.Parse( capacityString );
        }
        catch ( System.Exception e )
        {
            Debug.LogError( " Error getting Android battery value , the error is  ==>[" + e.Message + "]" );
            return -1;
        }
#endif
    }

}
