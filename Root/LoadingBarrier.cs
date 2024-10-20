using Godot;

namespace SuperShedAdmin.Root;

public partial class LoadingBarrier : Panel {

#nullable disable
	[Export]
	public virtual Label LoadingMessage { get; set; }
#nullable enable

	public virtual void ShowBarrier(string? message = null) {

		LoadingMessage.Text =
			message ?? LoadingMessage.Text;

		Show();

	}

	public virtual void HideBarrier() {

		Hide();

		LoadingMessage.Text = null;

	}

}