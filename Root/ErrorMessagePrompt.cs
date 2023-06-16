using Godot;

namespace SuperShedAdmin.Root;

public partial class ErrorMessagePrompt : PanelContainer {

	[Export]
	public virtual Label? ErrorMessage { get; set; }

	public virtual void SetMessage(string message) => ErrorMessage!.Text = message;

}