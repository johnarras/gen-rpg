using Azure.ResourceManager.ServiceBus;
using Genrpg.ServerShared.CloudComms.Requests.Entities;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.DataStores.Constants;
using Genrpg.Shared.Utils;
using Microsoft.Azure.Relay;
using MongoDB.Driver.Core.Servers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.ResourceManager.Relay;
using Azure.Messaging.ServiceBus.Administration;

namespace Genrpg.ServerShared.CloudComms.Requests.Managers
{
    internal class CloudRequestManager
    {
        private string _rlConnectionString = null;
        private string _myRelayName;
        private string _relayNamespace = null;
        private string _relaySharedAccessKeyName = null;
        private string _relaySharedAccessKey = null;
        private HybridConnectionListener _relayListener;
        private Dictionary<Type, IRequestHandler> _requestHandlers;
        private ServerGameState _serverGameState = null;
        private string _env = null;
        private string _serverId = null;

        private ICloudCommsService _cloudCommsService = null;

        public async Task Init(ServerGameState gs, ICloudCommsService cloudCommsService, string serverId, string env, CancellationToken token)
        {
            _env = env;
            _serverId = serverId;
            _serverGameState = gs;
            _cloudCommsService = cloudCommsService;
            await Task.CompletedTask;
            _ = Task.Run(() => RunRelay(gs, token));
        }

        public void SetRequestHandlers<H>(Dictionary<Type, H> handlers) where H : IRequestHandler
        {
            Dictionary<Type, IRequestHandler> newDict = new Dictionary<Type, IRequestHandler>();

            foreach (var handlerType in handlers.Keys)
            {
                newDict[handlerType] = handlers[handlerType];
            }

            _requestHandlers = newDict;
        }

        private async Task RunRelay(ServerGameState gs, CancellationToken token)
        {



            _rlConnectionString = _serverGameState.config.GetConnectionString(ConnectionNames.AzureRelay);

            _myRelayName = _cloudCommsService.GetFullServerName(_serverId);
            // Dynamic HybridConnection ArmClient with Active Directory keys Azure.ResourceManager.Relay...later

            string sharedAccessKeyNameId = "SharedAccessKeyName=";
            string sharedAccessKeyId = "SharedAccessKey=";

            _relayNamespace = StrUtils.StringBetweenTokens(_rlConnectionString, "sb://", ";");
            _relaySharedAccessKeyName = StrUtils.StringBetweenTokens(_rlConnectionString, sharedAccessKeyNameId, ";");
            _relaySharedAccessKey = StrUtils.StringBetweenTokens(_rlConnectionString, sharedAccessKeyId, ";");

            if (string.IsNullOrEmpty(_rlConnectionString))
            {
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

                ResponseEnvelope responseEnvelope = new ResponseEnvelope();
                try
                {
                    StreamReader inputReader = new StreamReader(context.Request.InputStream);
                    inputReader.BaseStream.Seek(0, SeekOrigin.Begin);
                    string content = inputReader.ReadToEnd();

                    RequestEnvelope requestEnvelope = SerializationUtils.Deserialize<RequestEnvelope>(content);

                    context.Response.StatusCode = HttpStatusCode.OK;

                    IRequest request = requestEnvelope.Request;

                    if (request == null)
                    {
                        responseEnvelope.ErrorText = "Missing CloudRequest";
                    }

                    if (_requestHandlers == null)
                    {
                        responseEnvelope.ErrorText = "This server has no ICloudRequestHandlers";
                    }

                    if (_requestHandlers.TryGetValue(request.GetType(), out IRequestHandler handler))
                    {
                        responseEnvelope.Response = await handler.HandleRequest(gs, request, responseEnvelope, token);
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

            while (true)
            {
                await Task.Delay(1000, token);
            }
        }


        public void SendRequest(string serverId, IRequest request, Action<ResponseEnvelope> responseAction)
        {
            _ = Task.Run(async () =>
            {
                ResponseEnvelope response = await SendRequestAsync(serverId, request);
                responseAction(response);
            });

        }

        private HttpClient _httpClient = new HttpClient();
        public async Task<ResponseEnvelope> SendRequestAsync(string serverId, IRequest request)
        {
            try
            {
                TokenProvider tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(_relaySharedAccessKeyName, _relaySharedAccessKey);
                Uri uri = new Uri(string.Format("https://{0}{1}", _relayNamespace, GetFullRequestEndpointName(serverId)));
                string token = (await tokenProvider.GetTokenAsync(uri.AbsoluteUri, TimeSpan.FromHours(1))).TokenString;

                RequestEnvelope envelope = new RequestEnvelope()
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

                return SerializationUtils.Deserialize<ResponseEnvelope>(responseText);
            }
            catch (Exception e)
            {
                Console.WriteLine("EXC: " + e.Message + " " + e.StackTrace);
                return new ResponseEnvelope() { ErrorText = e.Message };
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

    }
}
