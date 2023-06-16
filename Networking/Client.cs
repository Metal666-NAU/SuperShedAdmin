using Godot;

using System;
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

	public static void StartClient() {

		if(WebsocketClient != null) {

			GD.PushError("Failed to start Client: Client is already running");

			return;

		}

		WebsocketClient = new(new Uri("ws://localhost:8181"));

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

			Authenticate();

		});

		WebsocketClient.MessageReceived.Subscribe(responseMessage => {

			switch(responseMessage.MessageType) {



			}

		});

		WebsocketClient.Start();

	}

	public static void Authenticate() {

		WebsocketClient!.Send(JsonSerializer.Serialize(new AuthRequest() {

			// TODO:

		}, JSON_SERIALIZER_OPTIONS));

	}

	public static void SetConnectionStatus(ConnectionStatus connectionStatus) {

		ConnectionStatus = connectionStatus;

		ConnectionStatusChanged?.Invoke(ConnectionStatus);

	}

	public static void DisconnectClient() {

		WebsocketClient!.Stop(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, null);

		WebsocketClient!.Dispose();

		WebsocketClient = null;

	}

}