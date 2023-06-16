using Godot;

namespace SuperShedAdmin.Root;

public partial class LoginPrompt : PanelContainer {

	[Export]
	public virtual LineEdit? UsernameInput { get; set; }

	[Export]
	public virtual LineEdit? PasswordInput { get; set; }

	[Export]
	public virtual Button? PasswordVisibilityButton { get; set; }

	public virtual void OnPasswordVisibilityButtonToggled(bool pressed) =>
		TogglePasswordVisibilityState(pressed);

	public virtual void OnSubmitButtonPressed() { }

	protected virtual void TogglePasswordVisibilityState(bool visible) {

		PasswordVisibilityButton!.Text = visible ? "Hide" : "Show";

		PasswordInput!.Secret = !visible;

	}

	public override void _Ready() {

		TogglePasswordVisibilityState(false);

	}

}