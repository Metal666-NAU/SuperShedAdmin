using Godot;

namespace SuperShedAdmin.Root;

public partial class LoadingBarrier : Panel {

	[Export]
	public virtual Label? LoadingMessage { get; set; }

	public virtual void ShowBarrier(string? message = null) {

		LoadingMessage!.Text = message ?? LoadingMessage!.Text;

		Show();

	}

	public virtual void HideBarrier() {

		Hide();

		LoadingMessage!.Text = null;

	}

}