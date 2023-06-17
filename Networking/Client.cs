using Godot;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text.Json;

using Websocket.Client;

namespace SuperShedAdmin.Networking;

public static class Client {

	public static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new() {

		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

	};

	public static WebsocketClient? WebsocketClient { get; set; }

	public static DisconnectionInfo? DisconnectionInfo { get; set; }

	public static Dictionary<IncomingMessage, List<Action<BinaryReader>>> Listeners { get; set; } = new();

	public static ConnectionStatus ConnectionStatus { get; set; }

	public static event Action<ConnectionStatus>? ConnectionStatusChanged;

	public static event Action<AuthResponse>? AuthResponseReceived;

	public static event Action<string>? NetworkError;

	public static void StartClient() {

		if(WebsocketClient != null) {

			PublishNetworkError("Failed to start Client: Client is already running");

			return;

		}

		WebsocketClient = new(new Uri("ws://localhost:8181/admin"));

		SetConnectionStatus(ConnectionStatus.Connecting);

		WebsocketClient.DisconnectionHappened.Subscribe(disconnectionInfo => {

			if(disconnectionInfo.Type == DisconnectionType.Exit ||
				disconnectionInfo.Type == DisconnectionType.ByUser ||
				disconnectionInfo.Type == DisconnectionType.ByUser) {

				SetConnectionStatus(ConnectionStatus.Disconnected);

				return;

			}

			DisconnectionInfo = disconnectionInfo;

			SetConnectionStatus(ConnectionStatus.Connecting);

		});

		WebsocketClient.ReconnectionHappened.Subscribe(reconnectionInfo => {

			SetConnectionStatus(ConnectionStatus.Connected);

		});

		WebsocketClient.MessageReceived.Subscribe(responseMessage => {

			switch(responseMessage.MessageType) {

				case WebSocketMessageType.Text: {

					AuthResponse? authResponse = JsonSerializer.Deserialize<AuthResponse>(responseMessage.Text, JSON_SERIALIZER_OPTIONS);

					if(authResponse == null) {

						PublishNetworkError($"Failed to deserialize Auth Response: {responseMessage.Text}");

						break;

					}

					RunOnMainThread(() => AuthResponseReceived?.Invoke(authResponse));

					break;

				}

				case WebSocketMessageType.Binary: {

					using MemoryStream memoryStream = new(responseMessage.Binary);

					using BinaryReader binaryReader = new(memoryStream);

					IncomingMessage command = (IncomingMessage) binaryReader.ReadByte();

					Listeners.GetValueOrDefault(command)?
								.ForEach(listener =>
											RunOnMainThread(() => listener.Invoke(binaryReader)));

					break;

				}

			}

		});

		WebsocketClient.Start();

	}

	public static void Authenticate(string username, string password) =>
		Authenticate(new AuthRequest() {

			Username = username,
			Password = password

		});

	public static void Authenticate(string authToken) =>
		Authenticate(new AuthRequest() {

			AuthToken = authToken

		});

	public static void Listen(IncomingMessage incomingMessage, Action<BinaryReader> listener) {

		if(!Listeners.ContainsKey(incomingMessage)) {

			Listeners[incomingMessage] = new();

		}

		Listeners[incomingMessage].Add(listener);

	}

	private static void Authenticate(AuthRequest authRequest) =>
		WebsocketClient!.Send(JsonSerializer.Serialize(authRequest, JSON_SERIALIZER_OPTIONS));

	private static void SetConnectionStatus(ConnectionStatus connectionStatus) {

		ConnectionStatus = connectionStatus;

		RunOnMainThread(() => ConnectionStatusChanged?.Invoke(ConnectionStatus));

	}

	private static void PublishNetworkError(string message) =>
		RunOnMainThread(() => NetworkError?.Invoke(message));

	private static void RunOnMainThread(Action action) =>
		Dispatcher.SynchronizationContext.Send(_ => action(), null);

	public static void DisconnectClient() {

		WebsocketClient!.Stop(WebSocketCloseStatus.NormalClosure, null);

		WebsocketClient!.Dispose();

		WebsocketClient = null;

	}

	public enum IncomingMessage {

		Log,
		Worker,
		WorkerStatus

	}

}