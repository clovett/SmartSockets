// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using LovettSoftware.SmartSockets;
using System;
using System.Runtime.Serialization;

namespace LovettSoftware.SmartSockets.Interface
{
    [DataContract]
    public class ClientMessage : SocketMessage
    {
        public ClientMessage(string id, string name, DateTime timestamp)
            : base(id, name)
        {
            Timestamp = timestamp;
        }

        [DataMember]
        public DateTime Timestamp { get; set; }
    }

    [DataContract]
    public class ServerMessage : SocketMessage
    {
        public ServerMessage(string id, string name, DateTime timestamp)
            : base(id, name)
        {
            Timestamp = timestamp;
        }

        [DataMember]
        public DateTime Timestamp { get; set; }
    }
}

