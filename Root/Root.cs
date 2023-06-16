using Godot;

using SuperShedAdmin.Networking;

namespace SuperShedAdmin.Root;

public partial class Root : Node {

	[Export]
	public virtual Label? ConnectionStatusLabel { get; set; }

	[Export]
	public virtual Button? ConnectionActionButton { get; set; }

	[Export]
	public virtual ItemList? LogsList { get; set; }

	[Export]
	public virtual LoadingBarrier? LoadingBarrier { get; set; }

	[Export]
	public virtual PromptBarrier? PromptBarrier { get; set; }

	public virtual void OnLoginCredentialsSubmitted(string username, string password) {

		LoadingBarrier!.ShowBarrier("Logging in...");

		Client.Authenticate(username, password);

	}

	public virtual void OnServerLogsButtonToggled(bool pressed) {

		LogsList!.Visible = pressed;

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

			}

		};

		Client.AuthResponseReceived += authResponse => {

			LoadingBarrier!.HideBarrier();

			if(authResponse.Success == true) {

				return;

			}

			PromptBarrier!.ShowPrompt<LoginPrompt>().SetFailed();

		};

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

	protected override void Dispose(bool disposing) {

		base.Dispose(disposing);

		Client.DisconnectClient();

	}

}