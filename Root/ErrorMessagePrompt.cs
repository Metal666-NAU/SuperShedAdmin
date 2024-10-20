using Godot;

namespace SuperShedAdmin.Root;

public partial class ErrorMessagePrompt : PanelContainer {

#nullable disable
	[Export]
	public virtual Label ErrorMessage { get; set; }
#nullable enable

	public virtual void SetMessage(string message) =>
		ErrorMessage.Text = message;

}