using Godot;

namespace SuperShedAdmin.Root.Login;

public partial class Login : Control {

	[Export]
	public virtual LineEdit? UsernameInput { get; set; }

	[Export]
	public virtual LineEdit? PasswordInput { get; set; }

	[Export]
	public virtual Button? PasswordVisibilityButton { get; set; }

	public virtual void OnPasswordVisibilityButtonToggled(bool pressed) =>
		TogglePasswordVisibilityState(pressed);

	public virtual void OnSubmitButtonPressed() =>
		(GetNode(Root.NODE_PATH) as Root)!.GoToHomePage();

	public override void _Ready() {

		TogglePasswordVisibilityState(false);

	}

	public virtual void TogglePasswordVisibilityState(bool visible) {

		PasswordVisibilityButton!.Text = visible ? "Hide" : "Show";

		PasswordInput!.Secret = !visible;

	}

}