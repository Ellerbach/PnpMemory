using nanoFramework.Azure.Devices.Provisioning.Client;
using nanoFramework.Networking;
using nanoFramework.Runtime.Native;
using PnpMemory;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;


const string RegistrationID = "nanoMemMon";
const string IdScope = "xxx";
const string DpsAddress = "global.azure-devices-provisioning.net";
const string SasKey = "thelongkey";
const string Ssid = "ssid";
const string Password = "wifipasswd";

Debug.WriteLine("Hello from nanoFramework!");

// Give 60 seconds to the wifi join to happen
CancellationTokenSource cs = new(60000);
var success = WiFiNetworkHelper.ConnectDhcp(Ssid, Password, requiresDateTime: true, token: cs.Token);
if (!success)
{
    // Something went wrong, you can get details with the ConnectionError property:
    Debug.WriteLine($"Can't connect to the network, error: {WiFiNetworkHelper.Status}");
    if (WiFiNetworkHelper.HelperException != null)
    {
        Debug.WriteLine($"ex: {WiFiNetworkHelper.HelperException}");
    }
}

var azureCA = new X509Certificate(Resource.GetBytes(Resource.BinaryResources.AzureRoot));
var provisioning = ProvisioningDeviceClient.Create(DpsAddress, IdScope, RegistrationID, SasKey, azureCA);
var myDevice = provisioning.Register(new CancellationTokenSource(60000).Token);

if (myDevice.Status != ProvisioningRegistrationStatusType.Assigned)
{
    Debug.WriteLine($"Registration is not assigned: {myDevice.Status}, error message: {myDevice.ErrorMessage}");
    return;
}

rido_pnp_memmon memmon = rido_pnp_memmon.CreateDevice(myDevice.AssignedHub, myDevice.DeviceId, SasKey, azureCert: azureCA);
memmon.RegisterCommandgetRuntimeStats(MyCommandgetRuntimeStats);

memmon.started = DateTime.UtcNow;

again:
while (memmon.enabled)
{
    memmon.workingSet(nanoFramework.Runtime.Native.GC.Run(true));
    Thread.Sleep((int)memmon.interval * 1000);
}

while (!memmon.enabled)
{
    Thread.Sleep(1000);
}

goto again;

Thread.Sleep(Timeout.Infinite);

diagnosticResults MyCommandgetRuntimeStats(int rid, diagnosticsMode diagnosticsMode)
{
    // Do something
    var results = new diagnosticResults();
    results.Map.Add("FreeMemory", nanoFramework.Runtime.Native.GC.Run(true));
    // OK, should be a proper code?
    results.Status = 200;
    return results;
}

