## Smart Sockets

This is a very small and super simple socket library.

### SmartSocketServer

This class sets up a UDP broadcaster so clients on the same network can find the server by
a given string name, no fussing about with ip addresses and ports.  It then listens for
new clients to connect and spins off ClientConnected messages so your app can process the
server side of each conversation.  Your application server then can handle any number of
clients at the same time, each client will have their own SmartSocketClient on different ports.
If the client goes away, the ClientDisconnected event is raised so the server can cleanup.

### SmartSocketClient

This class provides a static `FindServerAsync` method to find the SmartSocketServer and
returns a connected SmartSocketClient.  The server will raise a ClientDisconnected event
when this happens so your server application can handle the new client.
From there you can do simple send/receive calls using
```c#
public async Task<SocketMessage> SendReceiveAsync(SocketMessage msg)
```
Or you can do more complicated protocols using `SendAsync` and `ReceiveAsync`.  When this
SmartSocketClient object is disposed it automatically disconnects from the server and
the server raises a ClientDisconnected event.


### SocketMessage

This is a simple base class that shows you how to create a custom message object for exchanging
between client and server.  Messages are serialized using the DataContractSerializer with the
serializer configured to support `PreserveObjectReferences = true` so you can serialize graphs.
The following is the base class for all SmartSocketClient  messages:

```c#
    [DataContract]
    public class SocketMessage
    {
        public SocketMessage(string id, string sender)
        {
            this.Id = id;
            this.Sender = sender;
        }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Sender { get; set; }
    }
```

When you define new custom message types, you need to provide those types and any custom member types
to your `SmartSocketTypeResolver` like this:
```c#
var resolver = new SmartSocketTypeResolver(typeof(ServerMessage), typeof(ClientMessage));
```

### Example Server

Creating a server then is very easy.  First you need to give your server a
unique name.  Ideally you'd add a NewGuid to this name to make it unique.

```c#
const string Name = "TestServer";
```

Now construct the server and register some event handlers.

```c#
SmartSocketServer server = new SmartSocketServer(Name, resolver);
server.ClientConnected += OnClientConnected;
server.ClientDisconnected += OnClientDisconnected;
server.StartListening();
```

When a client connects you start a thread to process messages from
that client like this:

```c#
private void OnClientConnected(object sender, SmartSocketClient e)
{
    e.Error += OnClientError;
    Console.WriteLine("Client '{0}' is connected", e.Name);
    Task.Run(() => HandleClientAsync(e));
}
```

Where your `HandleClientAsync` method might look something like this:

```c#
private async void HandleClientAsync(SmartSocketClient client)
{
    while (client.IsConnected)
    {
        ClientMessage e = await client.ReceiveAsync() as ClientMessage;
        if (e != null)
        {
            Console.WriteLine("Received message '{0}' from '{1}' at '{2}'",
                              e.Id, e.Sender, e.Timestamp);
            await client.SendAsync(new ServerMessage("Server says hi!", Name, DateTime.Now));
        }
    }
}
```

### Example Client

Creating a client is simple.  First you need a CancellationTokenSource for the
async calls, then you call `FindServerAsync`:

```c#
const string name = "client1";
CancellationTokenSource source = new CancellationTokenSource();
var client = await SmartSocketClient.FindServerAsync("TestServer", name, resolver, source.Token);
```

The following then adds an error handler and sends a bunch of messages and
receives the responses for each:

```c#
for (int i = 0; i < 1000; i++)
{
    var msg = new ClientMessage("test", this.name, DateTime.Now);
    SocketMessage response = await client.SendReceiveAsync(msg);
    ServerMessage e = (ServerMessage)response;
    // todo: do something with the server response.
}
```

As you can see the request/response pattern here makes life very simple for your application.  The following is the output you get on the server when you run
this code:

```
>start TestClient.exe
>TestServer.exe
Starting Server:
Press any key to terminate...
Client '127.0.0.1:52941' is connected
Received message 'Howdy partner 0' from 'client1' at '9/1/2019 2:01:13 AM'
Received message 'Howdy partner 1' from 'client1' at '9/1/2019 2:01:13 AM'
Received message 'Howdy partner 2' from 'client1' at '9/1/2019 2:01:13 AM'
Received message 'Howdy partner 3' from 'client1' at '9/1/2019 2:01:13 AM'
Received message 'Howdy partner 4' from 'client1' at '9/1/2019 2:01:13 AM'
Received message 'Howdy partner 5' from 'client1' at '9/1/2019 2:01:13 AM'
Received message 'Howdy partner 6' from 'client1' at '9/1/2019 2:01:13 AM'
Received message 'Howdy partner 7' from 'client1' at '9/1/2019 2:01:13 AM'
Received message 'Howdy partner 8' from 'client1' at '9/1/2019 2:01:13 AM'
Received message 'Howdy partner 9' from 'client1' at '9/1/2019 2:01:13 AM'
Client 'client1' has gone bye bye...
```

The round trip for messages (without any Console.WriteLine) is about 0.5 milliseconds.