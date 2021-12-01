using PnpMemory;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

Debug.WriteLine("Hello from nanoFramework!");

rido_pnp_memmon memmon = rido_pnp_memmon.CreateDevice("iot", "device", "sass", azureCert: new X509Certificate(Resource.GetBytes(Resource.BinaryResources.AzureRoot)));
memmon.RegisterCommandgetRuntimeStats(MyCommandgetRuntimeStats);

while (memmon.enabled)
{
    memmon.workingSet(nanoFramework.Runtime.Native.GC.Run(true));
    Thread.Sleep((int)memmon.interval.TotalMilliseconds);
}

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

