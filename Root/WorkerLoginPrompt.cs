using Godot;

namespace SuperShedAdmin.Root;

public partial class WorkerLoginPrompt : PanelContainer {

	[Export]
	public virtual Label? LoginCode { get; set; }

	public virtual void SetLoginCode(string loginCode) => LoginCode!.Text = loginCode;

}