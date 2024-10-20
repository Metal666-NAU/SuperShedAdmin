using Godot;

namespace SuperShedAdmin.Root;

public partial class WorkerLoginPrompt : PanelContainer {

#nullable disable
	[Export]
	public virtual Label LoginCode { get; set; }
#nullable enable

	public virtual void SetLoginCode(string loginCode) =>
		LoginCode.Text = loginCode;

}