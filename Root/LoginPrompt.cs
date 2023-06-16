using Godot;

namespace SuperShedAdmin.Root;

public partial class LoginPrompt : PanelContainer {

	[Export]
	public virtual LineEdit? UsernameInput { get; set; }

	[Export]
	public virtual LineEdit? PasswordInput { get; set; }

	[Export]
	public virtual Button? PasswordVisibilityButton { get; set; }

	[Export]
	public virtual Label? LoginFailed { get; set; }

	[Signal]
	public delegate void LoginCredentialsSubmittedEventHandler(string username, string password);

	public virtual void OnPasswordVisibilityButtonToggled(bool pressed) =>
		TogglePasswordVisibilityState(pressed);

	public virtual void OnSubmitButtonPressed() {

		EmitSignal(SignalName.LoginCredentialsSubmitted, UsernameInput!.Text, PasswordInput!.Text);

		UsernameInput.Text = null;
		PasswordInput.Text = null;

		SetFailed(false);

	}

	public override void _Ready() {

		TogglePasswordVisibilityState(false);

		SetFailed(false);

	}

	protected virtual void TogglePasswordVisibilityState(bool visible) {

		PasswordVisibilityButton!.Text = visible ? "Hide" : "Show";

		PasswordInput!.Secret = !visible;

	}

	public virtual void SetFailed(bool failed = true) =>
		LoginFailed!.Visible = failed;

}