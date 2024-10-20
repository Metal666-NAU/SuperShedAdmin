using Godot;
using Godot.Collections;

using System.Linq;

namespace SuperShedAdmin.Root;

public partial class PromptBarrier : Panel {

#nullable disable
	[Export]
	public virtual Array<PanelContainer> Prompts { get; set; }
#nullable enable

	public virtual TPrompt ShowPrompt<TPrompt>()
		where TPrompt : PanelContainer {

		TPrompt prompt = GetPrompt<TPrompt>();

		prompt.Show();

		Show();

		return prompt;

	}

	public virtual TPrompt GetPrompt<TPrompt>()
		where TPrompt : PanelContainer =>
			Prompts.OfType<TPrompt>()
					.Single();

	public virtual void HidePrompt() {

		Hide();

		foreach(PanelContainer promptControl in Prompts) {

			promptControl.Hide();

		}

	}

}