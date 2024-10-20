using Godot;

using Metal666.GodotUtilities.Extensions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text.Json;

using Websocket.Client;

namespace SuperShedAdmin.Networking;

public static class Client {

	public const string ServerAddressEnvVariable = "SUPERSHED_SERVER_ADDRESS";

	public static readonly JsonSerializerOptions JsonSerializerOptions = new() {

		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

	};

	public static WebsocketClient? WebsocketClient { get; set; }

	public static DisconnectionInfo? DisconnectionInfo { get; set; }

	public static Dictionary<IncomingMessage, Action<BinaryReader>> Listeners { get; set; } =
		[];

	public static ConnectionStatus ConnectionStatus { get; set; }

	public static event Action<ConnectionStatus>? ConnectionStatusChanged;

	public static event Action<AuthResponse>? AuthResponseReceived;

	public static event Action<string>? NetworkError;

	public static void StartClient() {

		if(WebsocketClient != null) {

			PublishNetworkError("Failed to start Client: Client is already running!");

			return;

		}

		string? serverAddress = System.Environment.GetEnvironmentVariable(ServerAddressEnvVariable);

		if(serverAddress == null) {

			PublishNetworkError($"Failed to start Client: server address not specified! Please set the {ServerAddressEnvVariable} environment variable.");

			return;

		}

		WebsocketClient = new(new Uri($"ws://{serverAddress}/admin")) {

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

					AuthResponse? authResponse =
						JsonSerializer.Deserialize<AuthResponse>(responseMessage.Text!,
																	JsonSerializerOptions);

					if(authResponse == null) {

						PublishNetworkError($"Failed to deserialize Auth Response: {responseMessage.Text}");

						break;

					}

					AuthResponseReceived?.InvokeOnMainThread(authResponse);

					break;

				}

				case WebSocketMessageType.Binary: {

					MemoryStream memoryStream = new(responseMessage.Binary!);
					BinaryReader binaryReader = new(memoryStream);

					IncomingMessage command = (IncomingMessage) binaryReader.ReadByte();

					if(!Listeners.TryGetValue(command, out Action<BinaryReader>? value)) {

						GD.PushWarning($"No listener found for command {command}, skipping...");

						return;

					}

					Threading.InvokeOnMainThread(() => {

						value(binaryReader);

						binaryReader.Dispose();

					});

					break;

				}

			}

		});

		WebsocketClient.Start();

	}

	public static void Listen(IncomingMessage incomingMessage, Action<BinaryReader> listener) =>
		Listeners[incomingMessage] = listener;

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
		WebsocketClient!.Send(JsonSerializer.Serialize(authRequest, JsonSerializerOptions));

	public static void SendStartWorkerAuth(string workerId) =>
		Send((byte) OutgoingMessage.StartWorkerAuth,
				workerId);

	public static void SendCancelWorkerAuth() =>
		Send((byte) OutgoingMessage.CancelWorkerAuth);

	public static void SendRevokeWorkerAuth(string workerId) =>
		Send((byte) OutgoingMessage.RevokeWorkerAuth,
				workerId);
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
		Send((byte) OutgoingMessage.DeleteRack,
				rackId);

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

		ConnectionStatusChanged?.InvokeOnMainThread(ConnectionStatus);

	}

	private static void PublishNetworkError(string message) =>
		NetworkError?.InvokeOnMainThread(message);

	public static void DisconnectClient() {

		WebsocketClient!.Stop(WebSocketCloseStatus.NormalClosure, "");

		WebsocketClient.Dispose();

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