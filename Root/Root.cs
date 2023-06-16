using Godot;

namespace SuperShedAdmin.Root;

public partial class Root : Node {

	public const string NODE_PATH = "/root/Root";

	[Export]
	public virtual PackedScene? LoginScene { get; set; }

	[Export]
	public virtual PackedScene? HomeScene { get; set; }

	[Export]
	public virtual Control? PageMountPoint { get; set; }

	[Export]
	public virtual Panel? LoadingBarrier { get; set; }

	[Export]
	public virtual Label? LoadingMessage { get; set; }

	public virtual Node? CurrentPage { get; set; }

	public override void _Ready() {

		SwitchPage(Settings.Instance.AuthToken == null ? LoginScene! : HomeScene!);

	}

	public virtual void GoToLoginPage() => SwitchPage(LoginScene!);
	public virtual void GoToHomePage() => SwitchPage(HomeScene!);

	protected virtual void SwitchPage(PackedScene scene) {

		CurrentPage?.QueueFree();

		CurrentPage = scene.Instantiate();

		PageMountPoint!.AddChild(CurrentPage);

	}

	public virtual void StartLoading(string message) {

		LoadingMessage!.Text = message;

		LoadingBarrier!.Show();

	}

	public virtual void FinishLoading() {

		LoadingBarrier!.Hide();

		LoadingMessage!.Text = null;

	}

}