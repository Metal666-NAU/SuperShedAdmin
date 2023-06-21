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

	public static Dictionary<IncomingMessage, Action<BinaryReader>> Listeners { get; set; }
		= new();

	public static ConnectionStatus ConnectionStatus { get; set; }

	public static event Action<ConnectionStatus>? ConnectionStatusChanged;

	public static event Action<AuthResponse>? AuthResponseReceived;

	public static event Action<string>? NetworkError;

	public static void StartClient() {

		if(WebsocketClient != null) {

			PublishNetworkError("Failed to start Client: Client is already running");

			return;

		}

		WebsocketClient = new(new Uri("ws://localhost:8181/admin")) {

			ReconnectTimeout = TimeSpan.FromMinutes(2),
			ErrorReconnectTimeout = TimeSpan.FromSeconds(10),

		};

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

					if(!Listeners.ContainsKey(command)) {

						GD.PushWarning($"No listener found for command {command}, skipping...");

						return;

					}

					RunOnMainThread(() => Listeners[command].Invoke(binaryReader));

					break;

				}

			}

		});

		WebsocketClient.Start();

	}

	public static void Listen(IncomingMessage incomingMessage, Action<BinaryReader> listener) => Listeners[incomingMessage] = listener;

	public static void StopListening(IncomingMessage incomingMessage) =>
		Listeners.Remove(incomingMessage);

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

	public static void SendStartWorkerAuth(string workerId) =>
		Send((byte) OutgoingMessage.StartWorkerAuth, workerId);

	public static void SendCancelWorkerAuth() =>
		Send((byte) OutgoingMessage.CancelWorkerAuth);

	public static void SendRevokeWorkerAuth(string workerId) =>
		Send((byte) OutgoingMessage.RevokeWorkerAuth, workerId);
	public static void SendUpdateBuilding(string buildingId,
											string buildingName,
											int buildingWidth,
											int buildingLength,
											int buildingHeight) =>
		Send((byte) OutgoingMessage.UpdateBuilding,
				buildingId, buildingName, buildingWidth, buildingLength, buildingHeight);

	public static void SendCreateRack(string buildingId) =>
		Send((byte) OutgoingMessage.CreateRack,
				buildingId);

	public static void SendUpdateRack(string rackId,
										int rackX,
										int rackZ,
										int rackWidth,
										int rackLength,
										int rackShelves,
										float rackSpacing,
										float rackRotation) =>
		Send((byte) OutgoingMessage.UpdateRack,
				rackId, rackX, rackZ, rackWidth, rackLength, rackShelves, rackSpacing, rackRotation);

	public static void SendDeleteRack(string rackId) =>
		Send((byte) OutgoingMessage.DeleteRack, rackId);

	private static void Send(byte command, params object[] data) {

		using MemoryStream memoryStream = new();

		using BinaryWriter binaryWriter = new(memoryStream);

		binaryWriter.Write(command);

		foreach(dynamic element in data) {

			binaryWriter.Write(element);

		}

		WebsocketClient!.Send(memoryStream.ToArray());

	}

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
		WorkerStatus,
		WorkerLoginCode,
		WorkerAuthSuccess,
		Building,
		Rack,
		NoRack,
		Product

	}

	public enum OutgoingMessage {

		StartWorkerAuth,
		CancelWorkerAuth,
		RevokeWorkerAuth,
		UpdateBuilding,
		CreateRack,
		UpdateRack,
		DeleteRack

	}

}