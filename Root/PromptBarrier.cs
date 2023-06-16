using Godot;
using Godot.Collections;

namespace SuperShedAdmin.Root;

public partial class PromptBarrier : Panel {

	[Export]
	public virtual Array<Control>? Prompts { get; set; }

	public override void _Ready() { }

	public virtual void ShowPrompt(Prompt prompt) {

		foreach(Control promptControl in Prompts!) {

			promptControl.Hide();

		}

		Prompts[(int) prompt].Show();

	}

	public enum Prompt {

		Login

	}

}