using Godot;

using System;
using System.Net.WebSockets;
using System.Text.Json;

using Websocket.Client;

namespace SuperShedAdmin.Networking;

public static class Client {

	public static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new() {

		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

	};

	public static WebsocketClient? WebsocketClient { get; set; }

	public static DisconnectionInfo? DisconnectionInfo { get; set; }

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

}