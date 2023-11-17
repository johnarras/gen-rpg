using Genrpg.Shared.Logs.Entities;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.Networking.Constants;
using Genrpg.Shared.Networking.Interfaces;
using Genrpg.Shared.Networking.MapApiSerializers;
using Genrpg.Shared.Networking.Messages;
using Genrpg.Shared.Pings.Messages;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Networking.Entities.TCP
{

    public delegate void MapApiMessageHandler(List<IMapApiMessage> messages, CancellationToken token, object optionalData);

    public abstract class BaseTcpConn : IConnection
    {
        private bool _removeMe { get; set; }

        private CancellationToken _token;
        protected ILogSystem _logger;
        private IMapApiSerializer _serializer;
        private MapApiMessageHandler _messageHandler;

        private TcpClient _client { get; set; }
        private NetworkStream _stream { get; set; }
        private byte[] _readBuffer { get; set; }

        private ConnMessageCounts _counts = new ConnMessageCounts();

        private DateTime _lastMessage = DateTime.UtcNow;
        private DateTime _startTime = DateTime.UtcNow;

        protected object _extraData = null;

        // This is concurrent so the game can send messages to it as needed.
        private ConcurrentQueue<IMapApiMessage> _outputQueue = new ConcurrentQueue<IMapApiMessage>();

        public BaseTcpConn(IMapApiSerializer serializer, MapApiMessageHandler messageHandler, ILogSystem logger, CancellationToken token, object extraData)
         
        {
            _logger = logger;
            _serializer = serializer;
            _messageHandler = messageHandler;
            _token = token;
            _extraData = extraData;
        }

        public ConnMessageCounts GetCounts()
        {
            _counts.Seconds = (long)Math.Max(1, (DateTime.UtcNow - _startTime).TotalSeconds);
            return _counts;
        }

        public bool RemoveMe()
        {
            return _removeMe;
        }

        public void ForceClose()
        {
            Shutdown(null, GetType().Name);
        }

        protected virtual void InitTcpClient(TcpClient client)
        {
            _client = client;
            _stream = _client.GetStream();
            _client.SendTimeout = ConnectionConstants.TimeoutMS;
            _client.ReceiveTimeout = ConnectionConstants.TimeoutMS;
            _client.SendBufferSize = ConnectionConstants.StartBufSize;
            _client.ReceiveBufferSize = ConnectionConstants.StartBufSize;
            _readBuffer = new byte[ConnectionConstants.StartBufSize];
            _removeMe = false;
            _counts = new ConnMessageCounts();
            _startTime = DateTime.UtcNow;

            Task.Run(() => WriteLoop());
            Task.Run(() => ReadLoop());
            Task.Run(() => PollOtherEnd());
        }

        protected virtual async Task PollOtherEnd()
        {
            Ping ping = new Ping();
            while (true)
            {
                await Task.Delay(ConnectionConstants.TimeoutCheckMS, _token).ConfigureAwait(false);
                if (_token.IsCancellationRequested)
                {
                    return;
                }
                if ((DateTime.UtcNow - _lastMessage).TotalSeconds < ConnectionConstants.TimeoutCheckMS / 1000.0)
                {
                    continue;
                }
                AppendOutput(ping);
                if (_removeMe)
                {
                    break;
                }
            }
        }


        public virtual void Shutdown(Exception? e, string message)
        {
            if (_removeMe || _client == null)
            {
                return;
            }
            _removeMe = true;
            _client.Client.Shutdown(SocketShutdown.Both);
            _client.Client.Disconnect(false);
            _client.Close();
            _client.Dispose();
            _client = null;

            if (e != null)
            {
                throw e;
            }
        }

        protected virtual void AppendOutput(IMapApiMessage message)
        {
            if (_removeMe)
            {
                return;
            }
            _counts.MessagesSent++;
            _outputQueue.Enqueue(message);
        }

        protected async Task WriteLoop()
        {
            List<IMapApiMessage> messages = new List<IMapApiMessage>();
            while (true)
            {
                try
                {
                    if (_removeMe)
                    {
                        break;
                    }

                    messages.Clear();

                    while (_outputQueue.TryDequeue(out IMapApiMessage newMessage))
                    {
                        // Because of trying lockless read on MapObjectManager nearby objects, sometimes
                        // an object will appear twice in the resulting list (distinct is expensive)
                        // so make one last attempt to dedupe here.
                        if (!messages.Contains(newMessage))
                        {
                            messages.Add(newMessage);
                        }
                    }

                    if (messages.Count < 1)
                    {
                        await Task.Delay(1).ConfigureAwait(false);
                        continue;
                    }

                    byte[] messageBytes = _serializer.Serialize(messages);

                    int totalLength = messageBytes.Length + 4;

                    // Add header to buffer.
                    byte[] headerBytes = BitConverter.GetBytes(messageBytes.Length);
                    _stream.Write(headerBytes, 0, headerBytes.Length);
                    _counts.BytesSent += totalLength;
                    // Get remaining bytes we can send now.
                    _stream.Write(messageBytes, 0, messageBytes.Length);
                    _counts.BytesSent += messageBytes.Length;
                    await _stream.FlushAsync(_token).ConfigureAwait(false);

                    if (_token.IsCancellationRequested)
                    {
                        return;
                    }

                    _lastMessage = DateTime.UtcNow;
                }
                catch (Exception e)
                {
                    Shutdown(e, "WriteLoop");
                }
            }
        }

        protected async Task ReadLoop()
        {

            byte[] header = new byte[ConnectionConstants.HeaderSize];

            try
            {
                while (true)
                {
                    if (_removeMe)
                    {
                        break;
                    }

                    _readBuffer[0] = 0;

                    while (true)
                    {
                        if (_removeMe)
                        {
                            break;
                        }

                        int headerBytesRead = await _stream.ReadAsync(_readBuffer, 0, ConnectionConstants.HeaderSize, _token);
                        if (_token.IsCancellationRequested)
                        {
                            return;
                        }
                        if (headerBytesRead != ConnectionConstants.HeaderSize)
                        {
                            Shutdown(null, "Failed to read header bytes: " + headerBytesRead);
                            break;
                        }

                        Buffer.BlockCopy(_readBuffer, 0, header, 0, ConnectionConstants.HeaderSize);

                        int newMessageLength = BitConverter.ToInt32(header, 0);

                        if (_readBuffer.Length < newMessageLength)
                        {
                            int newLength = _readBuffer.Length;
                            while (newLength < newMessageLength)
                            {
                                newLength *= 2;
                            }
                            _readBuffer = new byte[newLength];
                        }

                        int totalBytesRead = 0;

                        while (totalBytesRead < newMessageLength)
                        {
                            int currBytesRead = await _stream.ReadAsync(_readBuffer, totalBytesRead, newMessageLength - totalBytesRead, _token).ConfigureAwait(false);
                            if (_token.IsCancellationRequested)
                            {
                                return;
                            }
                            totalBytesRead += currBytesRead;
                        }

                        _counts.BytesReceived += totalBytesRead + ConnectionConstants.HeaderSize;

                        if (totalBytesRead != newMessageLength)
                        {
                            _logger.Info("Different bytes read: " + totalBytesRead + " vs " + newMessageLength);
                        }
                        else
                        {
                            OnReceiveBytes(_readBuffer, newMessageLength);
                        }
                        _lastMessage = DateTime.UtcNow;
                    }
                }
            }
            catch (Exception e)
            {
                Shutdown(e, "readLoop");
            }
        }


        public virtual void AddMessage(IMapApiMessage message)
        {
            AppendOutput(message);
        }
       
        protected virtual void OnReceiveBytes(byte[] receivedBytes, int byteCount)
        {
            List<IMapApiMessage> messageList = _serializer.Deserialize(receivedBytes, byteCount);
            _counts.MessagesReceived += messageList.Count;
            _messageHandler?.Invoke(messageList, _token, _extraData);
        }
    }
}