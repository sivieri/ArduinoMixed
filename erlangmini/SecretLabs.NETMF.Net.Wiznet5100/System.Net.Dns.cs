////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Secret Labs LLC
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// This code is licensed under the Apache 2.0 license.
// THIS CODE IS EXPERIMENTAL, PRE-BETA SOFTWARE
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT;

namespace System.Net
{
    public static class Dns
    {
        public static System.Net.IPHostEntry GetHostEntry(string hostNameOrAddress)
        {
            IPAddress[] ipAddressList = {};

            try
            {
                // addresses are supported
                ipAddressList = new IPAddress[] { IPAddress.Parse(hostNameOrAddress) };
            }
            catch
            {
                // hostnames not yet supported
                throw new NotSupportedException();
            }

            IPHostEntry ipHostEntry = new IPHostEntry();
            ipHostEntry.HostName = hostNameOrAddress;
            ipHostEntry.AddressList = ipAddressList;
            return ipHostEntry;
        }
    }
}