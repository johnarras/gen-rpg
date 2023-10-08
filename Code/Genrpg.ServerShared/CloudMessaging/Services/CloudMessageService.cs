using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using System.Threading;
using System.ComponentModel.DataAnnotations;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Utils;
using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Azure.Relay;
using Genrpg.ServerShared.CloudMessaging.Messages;
using Genrpg.Shared.Logs.Entities;
using Genrpg.ServerShared.DataStores.Constants;
using System.Net.Http;
using System.IO;
using System.Net;
using Genrpg.ServerShared.CloudMessaging.Requests;

namespace Genrpg.ServerShared.CloudMessaging.Services
{

    public interface ICloudMessageService : ISetupService, IDisposable
    {
        string GetFullQueueName(string serverId);
        void SetMessageHandlers<H>(Dictionary<Type, H> handlers) where H : ICloudMessageHandler;
        void SetRequestHandlers<H>(Dictionary<Type, H> handlers) where H : ICloudRequestHandler;
        void SendMessage(string serverId, ICloudMessage cloudMessage);

        /// <summary>
        /// Send request and get a response from one of the "shared" servers.
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="request"></param>
        /// <param name="responseAction">Keep this very very simple and do not modify the character or do anything
        /// really complex because this response will be coming in from outside of the main simulation and
        /// you could mess things up. Might be better to send a message to the object within the
        /// main simulation. </param>
        void SendRequest(string serverId, ICloudRequest request, Action<CloudResponseEnvelope> responseAction);
        Task<CloudResponseEnvelope> SendRequestAsync(string serverId, ICloudRequest request);
    }       

    public class CloudMessageService : ICloudMessageService
    {
        private ServerGameState _serverGameState = null;
        private string _env;
        private string _serverId;
        private CancellationToken _token = CancellationToken.None;

        private ServiceBusClient _queueClient = null;
        private string _sbConnectionString = null;
        private ServiceBusReceiver _queueReceiver;
        private Dictionary<Type, ICloudMessageHandler> _queueHandlers;
        private Dictionary<Type, ICloudRequestHandler> _requestHandlers;
        private string _myQueueName;
        private bool _didSetupQueue = false;

        private string _rlConnectionString = null;
        private string _myRelayName;
        private bool _didSetupRelay = false;
        private string _relayNamespace = null;
        private string _relaySharedAccessKeyName = null;
        private string _relaySharedAccessKey = null;
        private HybridConnectionListener _relayListener;

        public async Task Setup(GameState gs, CancellationToken token)
        {
            _serverGameState = gs as ServerGameState;
            _token = token;
            _env = _serverGameState.config.Env;
            _serverId = _serverGameState.config.ServerId;

            await SetupQueue(_serverGameState, token);

            await SetupRelay(_serverGameState, token);
        }

        public void Dispose()
        {
            _queueClient?.DisposeAsync();

        }

        #region queue
        private async Task SetupQueue(ServerGameState gs, CancellationToken token)
        {           
            _sbConnectionString = gs.config.GetConnectionString(ConnectionNames.ServiceBus);
            _myQueueName = GetFullQueueName(_serverId);
            ServiceBusAdministrationClient adminClient = new ServiceBusAdministrationClient(_sbConnectionString);

            CreateQueueOptions options = new CreateQueueOptions(_myQueueName)
            {
                DefaultMessageTimeToLive = TimeSpan.FromSeconds(20.0f)
            };

            try
            {
                if (!await adminClient.QueueExistsAsync(_myQueueName, token))
                {
                    await adminClient.CreateQueueAsync(options, token);
                }
            }
            catch (Exception e)
            {
                gs.logger.Exception(e, "CloudMessageSetup");
            }

            gs.logger.Message("Created Queue " + _myQueueName);
            _queueClient = new ServiceBusClient(_sbConnectionString);

            _ = Task.Run(() => SetupMessageReceiver(gs.logger, _token));

            _didSetupQueue = true;
            await Task.CompletedTask;
        }

        protected string QueueSuffix()
        {
            return ("." + _env).ToLower();
        }

        public string GetFullQueueName(string serverId)
        {
            return (serverId + QueueSuffix()).ToLower();
        }

        public static string GetQueueNameForMap(string mapId, string mapInstanceId)
        {
            return "map" + mapId + "." + mapInstanceId;
        }

        public void SetMessageHandlers<H>(Dictionary<Type,H> handlers) where H : ICloudMessageHandler
        {
            Dictionary<Type, ICloudMessageHandler> newDict = new Dictionary<Type, ICloudMessageHandler>();

            foreach (var handlerType in handlers.Keys)
            {
                newDict[handlerType] = handlers[handlerType];
            }

            _queueHandlers = newDict;
        }

        private async Task SetupMessageReceiver(ILogSystem logger, CancellationToken token)
        {
            ServiceBusReceiverOptions options = new ServiceBusReceiverOptions()
            {
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                PrefetchCount = 50,
            };

            _queueReceiver = _queueClient.CreateReceiver(_myQueueName, options);

            while (_queueHandlers == null)
            {
                await Task.Delay(1, token);
            }

            while (true)
            {
                IReadOnlyList<ServiceBusReceivedMessage> messages = await _queueReceiver.ReceiveMessagesAsync(50, TimeSpan.FromSeconds(1.0f), token);

                foreach (ServiceBusReceivedMessage message in messages)
                {
                    CloudMessageEnvelope envelope = SerializationUtils.Deserialize<CloudMessageEnvelope>(Encoding.UTF8.GetString(message.Body));
                    logger.Message("Received message: " + _myQueueName);

                    if (_queueHandlers == null)
                    {
                        throw new Exception("Cloud message handlers not set up");
                    }

                    if (_queueHandlers.TryGetValue(envelope.Message.GetType(), out ICloudMessageHandler handler))
                    {
                        await handler.HandleMessage(_serverGameState, envelope.Message, _token);
                    }
                }
            }
        }

        private ConcurrentDictionary<string, ServiceBusSender> _senders = new ConcurrentDictionary<string, ServiceBusSender>();
        public void SendMessage(string serverId, ICloudMessage cloudMessage)
        {
            if (!_didSetupQueue)
            {
                return;
            }

            if (serverId.IndexOf(QueueSuffix()) < 0)
            {
                serverId = GetFullQueueName(serverId);
            }

            CloudMessageEnvelope envelope = new CloudMessageEnvelope()
            {
                ToServerId = serverId,
                FromServerId = _serverId,
                Message = cloudMessage,
            };

            if (!_senders.TryGetValue(envelope.ToServerId, out ServiceBusSender sender))
            {
                sender = _queueClient.CreateSender(envelope.ToServerId);
                _senders[envelope.ToServerId] = sender;
            }

            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(SerializationUtils.Serialize(envelope));
            _ = Task.Run(() => sender.SendMessageAsync(serviceBusMessage));

        }
        #endregion

        #region relay


        public void SetRequestHandlers<H>(Dictionary<Type, H> handlers) where H : ICloudRequestHandler
        {
            Dictionary<Type, ICloudRequestHandler> newDict = new Dictionary<Type, ICloudRequestHandler>();

            foreach (var handlerType in handlers.Keys)
            {
                newDict[handlerType] = handlers[handlerType];
            }

            _requestHandlers = newDict;
        }



        private async Task SetupRelay(ServerGameState gs, CancellationToken token)
        {
            _ = Task.Run(() => RunRelay(gs, token));
        }

        private async Task RunRelay(ServerGameState gs, CancellationToken token)
        {

            _rlConnectionString = _serverGameState.config.GetConnectionString(ConnectionNames.AzureRelay);

            _myRelayName = GetFullQueueName(_serverId);
            // Dynamic HybridConnection ArmClient with Active Directory keys Azure.ResourceManager.Relay...later

            string serviceBusSuffix = ".servicebus.windows.net/";
            string sharedAccessKeyNameId = "SharedAccessKeyName=";
            string sharedAccessKeyId = "SharedAccessKey=";

            _relayNamespace = StrUtils.StringBetweenTokens(_rlConnectionString, "sb://", ";");
            _relaySharedAccessKeyName = StrUtils.StringBetweenTokens(_rlConnectionString, sharedAccessKeyNameId, ";");
            _relaySharedAccessKey = StrUtils.StringBetweenTokens(_rlConnectionString, sharedAccessKeyId, ";");


            if (string.IsNullOrEmpty(_rlConnectionString))
            {
                _didSetupRelay = false;
                return;
            }

            while (_requestHandlers == null)
            {
                await Task.Delay(1, token);
            }

            string fullConnectionString = _rlConnectionString + ";EntityPath=" + _myRelayName;
            // Create listener
            _relayListener = new HybridConnectionListener(fullConnectionString);
            _relayListener.Connecting += (o, e) => { Console.WriteLine(_myRelayName + " Connecting"); };
            _relayListener.Offline += (o, e) => { Console.WriteLine(_myRelayName + " Offline"); };
            _relayListener.Online += (o, e) => { Console.WriteLine(_myRelayName + " Online"); };
            _relayListener.RequestHandler = async (context) =>
            {

                CloudResponseEnvelope responseEnvelope = new CloudResponseEnvelope();
                try
                {
                    StreamReader inputReader = new StreamReader(context.Request.InputStream);
                    inputReader.BaseStream.Seek(0, SeekOrigin.Begin);
                    string content = inputReader.ReadToEnd();

                    CloudRequestEnvelope requestEnvelope = SerializationUtils.Deserialize<CloudRequestEnvelope>(content);

                    context.Response.StatusCode = HttpStatusCode.OK;

                    ICloudRequest request = requestEnvelope.Request;

                    if (request == null)
                    {
                        responseEnvelope.ErrorText = "Missing CloudRequest";
                    }

                    if (_requestHandlers == null)
                    {
                        responseEnvelope.ErrorText = "This server has no ICloudRequestHandlers";
                    }

                    if (_requestHandlers.TryGetValue(request.GetType(), out ICloudRequestHandler handler))
                    {
                        responseEnvelope.Response = await handler.HandleRequest(gs, request, token);
                    }
                    else
                    {
                        responseEnvelope.ErrorText = "Missing ICloudRequestHandler for type: " + request.GetType().Name;
                    }
                }
                catch (Exception e)
                {
                    responseEnvelope.ErrorText = e.Message;
                }
                StreamWriter outputWriter = new StreamWriter(context.Response.OutputStream);
                outputWriter.Write(SerializationUtils.Serialize(responseEnvelope));
                await outputWriter.FlushAsync();
                // The context MUST be closed here
                context.Response.Close();
            };

            token.Register(() => { _relayListener.CloseAsync(token).Wait(); });

            await _relayListener.OpenAsync(token);

            _didSetupRelay = true;

            while (true)
            {
                await Task.Delay(1000, token);
            }
        }


        public void SendRequest(string serverId, ICloudRequest request, Action<CloudResponseEnvelope> responseAction)
        {
            _ = Task.Run(async () =>
            {
                CloudResponseEnvelope response = await SendRequestAsync(serverId, request);
                responseAction(response);
            });

        }

        private HttpClient _httpClient = new HttpClient();
        public async Task<CloudResponseEnvelope> SendRequestAsync(string serverId, ICloudRequest request)
        {
            try
            {
                TokenProvider tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(_relaySharedAccessKeyName, _relaySharedAccessKey);
                Uri uri = new Uri(string.Format("https://{0}{1}", _relayNamespace, GetFullRequestEndpointName(serverId)));
                string token = (await tokenProvider.GetTokenAsync(uri.AbsoluteUri, TimeSpan.FromHours(1))).TokenString;

                CloudRequestEnvelope envelope = new CloudRequestEnvelope()
                {
                    Request = request,
                };

                string envelopeString = SerializationUtils.Serialize(envelope);

                HttpRequestMessage httpRequest = new HttpRequestMessage()
                {
                    RequestUri = uri,
                    Method = HttpMethod.Get,
                    Content = new StringContent(envelopeString),
                };

                httpRequest.Headers.Add("ServiceBusAuthorization", token);
                HttpResponseMessage response = await _httpClient.SendAsync(httpRequest);
                string responseText = await response.Content.ReadAsStringAsync();

                return SerializationUtils.Deserialize<CloudResponseEnvelope>(responseText);
            }
            catch (Exception e)
            {
                Console.WriteLine("EXC: " + e.Message + " " + e.StackTrace);
                return new CloudResponseEnvelope() { ErrorText = e.Message };
            }
        }


        protected string RequestSuffix()
        {
            return ("." + _env).ToLower();
        }

        public string GetFullRequestEndpointName(string serverId)
        {
            return (serverId + RequestSuffix()).ToLower();
        }


        #endregion

    }
}
