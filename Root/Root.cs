using Godot;

using SuperShedAdmin.Networking;

using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperShedAdmin.Root;

public partial class Root : Node {

	[Export]
	public virtual Label? ConnectionStatusLabel { get; set; }

	[Export]
	public virtual Button? ConnectionActionButton { get; set; }

	[Export]
	public virtual TabContainer? BuildingsTabContainer { get; set; }

	[Export]
	public virtual PackedScene? BuildingTab { get; set; }

	[Export]
	public virtual Button? ShowWorkersButton { get; set; }

	[Export]
	public virtual Panel? WorkersPanel { get; set; }

	[Export]
	public virtual ItemList? OnlineWorkersList { get; set; }

	[Export]
	public virtual PopupMenu? OnlineWorkerPopupMenu { get; set; }

	[Export]
	public virtual ItemList? OfflineWorkersList { get; set; }

	[Export]
	public virtual PopupMenu? OfflineWorkerPopupMenu { get; set; }

	[Export]
	public virtual Label? WorkerStats { get; set; }

	[Export]
	public virtual ItemList? LogsList { get; set; }

	[Export]
	public virtual LoadingBarrier? LoadingBarrier { get; set; }

	[Export]
	public virtual PromptBarrier? PromptBarrier { get; set; }

	public virtual List<Worker> Workers { get; set; } = new();
	public virtual List<Building> Buildings { get; set; } = new();

	public virtual void OnLoginCredentialsSubmitted(string username, string password) {

		LoadingBarrier!.ShowBarrier("Logging in...");

		Client.Authenticate(username, password);

	}

	public virtual void OnConnectionActionButtonPressed() {

		switch(Client.ConnectionStatus) {

			case ConnectionStatus.Connected:
			case ConnectionStatus.Connecting: {

				Client.DisconnectClient();

				break;

			}

			case ConnectionStatus.Disconnected: {

				Client.StartClient();

				break;

			}

		}

	}

	public virtual void OnWorkersPanelToggled(bool open) {

		WorkersPanel!.Visible = open;

		ShowWorkersButton!.Visible = !open;

	}

	public virtual void OnOnlineWorkerClicked(int index, Vector2 position, int mouseButton) =>
		OnWorkerClicked(OnlineWorkersList!, OnlineWorkerPopupMenu!, index, position, mouseButton);

	public virtual void OnOfflineWorkerClicked(int index, Vector2 position, int mouseButton) =>
		OnWorkerClicked(OfflineWorkersList!, OfflineWorkerPopupMenu!, index, position, mouseButton);

	public virtual void OnWorkerClicked(ItemList workerList, PopupMenu popupMenu, int index, Vector2 position, int mouseButton) {

		if(mouseButton != (int) MouseButton.Right) {

			return;

		}

		popupMenu!.Position = new Vector2I(Mathf.RoundToInt(position.X + workerList.GlobalPosition.X),
											Mathf.RoundToInt(position.Y + workerList.GlobalPosition.Y));

		popupMenu.SetMeta("workerId", workerList!.GetItemMetadata(index).AsString());

		popupMenu.Popup();

	}

	public virtual void OnOnlineWorkerActionPressed(int index) {

		switch(index) {

			case 0: {

				Client.SendRevokeWorkerAuth(OnlineWorkerPopupMenu!.GetMeta("workerId").AsString());

				break;

			}

		}

	}

	public virtual void OnOfflineWorkerActionPressed(int index) {

		switch(index) {

			case 0: {

				PromptBarrier!.ShowPrompt<WorkerLoginPrompt>().SetLoginCode("Generating Login Code...");

				Client.SendStartWorkerAuth(OfflineWorkerPopupMenu!.GetMeta("workerId").AsString());

				break;

			}

			case 1: {

				Client.SendRevokeWorkerAuth(OfflineWorkerPopupMenu!.GetMeta("workerId").AsString());

				break;

			}

		}

	}

	public virtual void OnWorkerLoginCancelled() => Client.SendCancelWorkerAuth();

	public virtual void OnServerLogsButtonToggled(bool pressed) {

		LogsList!.Visible = pressed;

	}

	public override void _Ready() {

		UpdateConnectionStatus(ConnectionStatus.Disconnected);

		Client.NetworkError += message => PromptBarrier!.ShowPrompt<ErrorMessagePrompt>().SetMessage(message);

		Client.ConnectionStatusChanged += connectionStatus => {

			UpdateConnectionStatus(connectionStatus);

			switch(connectionStatus) {

				case ConnectionStatus.Connected: {

					if(Settings.Instance.AuthToken != null) {

						Client.Authenticate(Settings.Instance.AuthToken);

					}

					else {

						PromptBarrier!.ShowPrompt<LoginPrompt>();

					}

					break;

				}

				case ConnectionStatus.Connecting:
				case ConnectionStatus.Disconnected: {

					LogsList!.Clear();
					Workers!.Clear();
					Buildings!.Clear();

					UpdateWorkerLists();

					foreach(Node node in BuildingsTabContainer!.GetChildren()) {

						node.QueueFree();

					}

					PromptBarrier!.HidePrompt();

					break;

				}

			}

		};

		Client.AuthResponseReceived += authResponse => {

			LoadingBarrier!.HideBarrier();

			if(authResponse.Success == true) {

				Settings.Instance.AuthToken = authResponse.AuthToken;

				return;

			}

			PromptBarrier!.ShowPrompt<LoginPrompt>().SetFailed();

		};

		Client.Listen(Client.IncomingMessage.Log, data => {

			string message = data.ReadString();

			Color color = new();

			switch(data.ReadByte()) {

				case 0: {

					color = Colors.White;

					break;

				}

				case 1: {

					color = Colors.Green;

					break;

				}

				case 2: {

					color = Colors.Red;

					break;

				}

			}

			LogsList!.SetItemCustomFgColor(LogsList.AddItem(message), color);

			VScrollBar scrollBar = LogsList.GetVScrollBar();

			scrollBar.Value = scrollBar.MaxValue;

		});

		Client.Listen(Client.IncomingMessage.Worker, data => {

			string workerId = data.ReadString();
			string workerName = data.ReadString();

			Workers.Add(new(workerId, workerName, false));

			UpdateWorkerLists();

		});

		Client.Listen(Client.IncomingMessage.WorkerStatus, data => {

			string workerId = data.ReadString();
			bool isOnline = data.ReadBoolean();

			int index = Workers.FindIndex(worker => worker.Id.Equals(workerId));

			if(index == -1) {

				GD.PushError("Failed to update Worker Status: Worker is not in the Workers list.");

				return;

			}

			Workers[index] = Workers[index] with { IsOnline = isOnline };

			UpdateWorkerLists();

		});

		Client.Listen(Client.IncomingMessage.WorkerLoginCode,
						data => PromptBarrier!.GetPrompt<WorkerLoginPrompt>()
																.SetLoginCode(data.ReadString()));

		Client.Listen(Client.IncomingMessage.WorkerAuthSuccess,
						data => PromptBarrier!.HidePrompt());

		Client.Listen(Client.IncomingMessage.Building, data => {

			string buildingId = data.ReadString();
			string buildingName = data.ReadString();
			int buildingWidth = data.ReadInt32();
			int buildingLength = data.ReadInt32();
			int buildingHeight = data.ReadInt32();

			Building building = new(buildingId,
											buildingName,
											new(buildingWidth,
															buildingHeight,
															buildingLength));

			Building? existingBuilding =
				Buildings.SingleOrDefault(building => building.Id == buildingId);

			if(existingBuilding != null) {

				BuildingTab.BuildingTab existingBuildingTab =
					BuildingsTabContainer!.GetChildren()
											.Cast<BuildingTab.BuildingTab>()
											.Single(buildingTab =>
														buildingTab.Building == existingBuilding);

				existingBuildingTab.Name = Guid.NewGuid().ToString();

				existingBuildingTab.QueueFree();

				Buildings.Remove(existingBuilding);

			}

			Buildings.Add(building);

			BuildingTab.BuildingTab buildingTab = BuildingTab!.Instantiate<BuildingTab.BuildingTab>();

			buildingTab.Name = building.Name;
			buildingTab.Building = building;

			BuildingsTabContainer!.AddChild(buildingTab);

			BuildingsTabContainer.SetTabMetadata(BuildingsTabContainer.GetTabIdxFromControl(buildingTab),
													building.Id);

		});

		Client.Listen(Client.IncomingMessage.Rack, data => {

			string rackId = data.ReadString();
			string buildingId = data.ReadString();
			int rackX = data.ReadInt32();
			int rackZ = data.ReadInt32();
			int rackWidth = data.ReadInt32();
			int rackLength = data.ReadInt32();
			int rackShelves = data.ReadInt32();
			float rackSpacing = data.ReadSingle();
			float rackRotation = data.ReadSingle();

			BuildingTab.BuildingTab? buildingTab =
				BuildingsTabContainer!.GetChildren()
										.Cast<BuildingTab.BuildingTab>()
										.FirstOrDefault(buildingTab =>
															buildingTab.Building!.Id.Equals(buildingId));

			if(buildingTab == null) {

				GD.PushError("Failed to create/update Rack: Building Tab not found!");

				return;

			}

			buildingTab.UpdateRack(rackId,
									new(rackX, rackZ),
									new(rackWidth, rackLength),
									rackShelves,
									rackSpacing,
									rackRotation);

		});

		Client.StartClient();

	}

	public virtual void UpdateConnectionStatus(ConnectionStatus connectionStatus) {

		ConnectionStatusLabel!.Text = $"Connection status: {connectionStatus}";

		Color fontColor = new();

		switch(connectionStatus) {

			case ConnectionStatus.Connected: {

				fontColor = Colors.Green;

				ConnectionActionButton!.Text = "Disconnect?";

				break;

			}

			case ConnectionStatus.Disconnected: {

				fontColor = Colors.Red;

				ConnectionActionButton!.Text = "Connect?";

				break;

			}

			case ConnectionStatus.Connecting: {

				fontColor = Colors.Yellow;

				ConnectionActionButton!.Text = "Cancel?";

				break;

			}

		}

		ConnectionStatusLabel.AddThemeColorOverride("font_color", fontColor);

	}

	public virtual void UpdateWorkerLists() {

		OnlineWorkersList!.Clear();
		OfflineWorkersList!.Clear();

		foreach(Worker worker in Workers) {

			ItemList itemList = worker.IsOnline ? OnlineWorkersList : OfflineWorkersList;

			itemList.SetItemMetadata(itemList.AddItem(worker.Name), worker.Id);
		}

		WorkerStats!.Text = $"Workers: {Workers.Count}, Online: {Workers.Count(worker => worker.IsOnline)}";

	}

	protected override void Dispose(bool disposing) {

		base.Dispose(disposing);

		Client.DisconnectClient();

	}

	public record Worker(string Id, string Name, bool IsOnline);

	public record Building(string Id, string Name, Vector3I Size);

}