using Godot;
using Godot.Collections;

using System.Linq;

namespace SuperShedAdmin.Root;

public partial class PromptBarrier : Panel {

	[Export]
	public virtual Array<PanelContainer>? Prompts { get; set; }

	public virtual TPrompt ShowPrompt<TPrompt>()
		where TPrompt : PanelContainer {

		TPrompt prompt = Prompts!.OfType<TPrompt>().Single();

		prompt.Show();

		Show();

		return prompt;

	}

	public virtual void HidePrompt() {

		Hide();

		foreach(PanelContainer promptControl in Prompts!) {

			promptControl.Hide();

		}

	}

}