using Godot;

using SuperShedAdmin.Networking;

using System.Collections.Generic;

using static SuperShedAdmin.Root.Root;

namespace SuperShedAdmin.Root.BuildingTab;

public partial class BuildingTab : Control {

	[Export]
	public virtual Button? PrimaryViewButton { get; set; }

	[Export]
	public virtual Button? SecondaryViewButton { get; set; }

	[Export]
	public virtual Panel? PrimaryView { get; set; }

	[Export]
	public virtual Viewport? BuildingModelViewport { get; set; }

	[Export]
	public virtual BuildingModel? BuildingModel { get; set; }

	[Export]
	public virtual PopupMenu? BuildingModelPopupMenu { get; set; }

	[Export]
	public virtual Panel? SidePanel { get; set; }

	[Export]
	public virtual Panel? SecondaryView { get; set; }

	[Export]
	public virtual SpinBox? BuildingWidthInput { get; set; }

	[Export]
	public virtual SpinBox? BuildingLengthInput { get; set; }

	[Export]
	public virtual SpinBox? BuildingHeightInput { get; set; }

	[Export]
	public virtual Button? EditBuildingSizeButton { get; set; }

	[Export]
	public virtual Button? SaveBuildingSizeButton { get; set; }

	[Export]
	public virtual Button? CancelEditingBuildingSizeButton { get; set; }

	public virtual Building? Building { get; set; }

	public virtual Dictionary<string, (Vector2I Position, Vector2I Size, int Shelves, float Spacing)> Racks { get; set; }
		= new();

	public virtual void OnViewToggled(bool primaryView) {

		PrimaryViewButton!.ButtonPressed = primaryView;
		SecondaryViewButton!.ButtonPressed = !primaryView;

		PrimaryView!.Visible = primaryView;
		SecondaryView!.Visible = !primaryView;

	}

	public virtual void OnBuildingModelViewportContainerGuiInput(InputEvent inputEvent) {

		if(inputEvent is not InputEventMouseButton inputEventMouseButton) {

			return;

		}

		if(inputEventMouseButton.ButtonIndex != MouseButton.Right) {

			return;

		}

		if(!inputEventMouseButton.Pressed) {

			return;

		}

		BuildingModelPopupMenu!.Position = new(Mathf.RoundToInt(inputEventMouseButton.GlobalPosition.X),
														Mathf.RoundToInt(inputEventMouseButton.GlobalPosition.Y));

		BuildingModelPopupMenu.Popup();

	}

	public virtual void OnBuildingModelActionPressed(int index) {

		switch(index) {

			case 0: {

				Client.SendCreateRack(Building!.Id);

				break;

			}

		}

	}

	public virtual void OnBuildingSizeChanged(float _) {

		SaveBuildingSizeButton!.Disabled =
			Building!.Size.Equals(new(Mathf.RoundToInt(BuildingWidthInput!.Value),
													Mathf.RoundToInt(BuildingHeightInput!.Value),
													Mathf.RoundToInt(BuildingLengthInput!.Value)));

	}

	public virtual void OnEditBuildingSizeButtonPressed() =>
		ToggleBuildingSizeEditing(true);

	public virtual void OnSaveBuildingSizeButtonPressed() {

		Client.SendUpdateBuilding(Building!.Id,
									Building!.Name,
									Mathf.RoundToInt(BuildingWidthInput!.Value),
									Mathf.RoundToInt(BuildingLengthInput!.Value),
									Mathf.RoundToInt(BuildingHeightInput!.Value));

		ToggleBuildingSizeEditing(false);

	}

	public virtual void OnCancelEditingBuildingSizeButtonPressed() {

		ResetBuildingSizeInputs();

		ToggleBuildingSizeEditing(false);

	}

	public override void _Ready() {

		ResetBuildingSizeInputs();

		BuildingModel!.SetSize(Building!.Size);

	}

	public virtual void ToggleBuildingSizeEditing(bool editing) {

		EditBuildingSizeButton!.Visible = !editing;

		SaveBuildingSizeButton!.Visible = editing;
		CancelEditingBuildingSizeButton!.Visible = editing;

		SaveBuildingSizeButton.Disabled = true;

		BuildingWidthInput!.Editable = editing;
		BuildingLengthInput!.Editable = editing;
		BuildingHeightInput!.Editable = editing;

	}

	public virtual void ResetBuildingSizeInputs() {

		BuildingWidthInput!.Value = Building!.Size.X;
		BuildingLengthInput!.Value = Building!.Size.Z;
		BuildingHeightInput!.Value = Building!.Size.Y;

	}

	public virtual void UpdateRack(string rackId,
									Vector2I position,
									Vector2I size,
									int shelves,
									float spacing) {

		Racks[rackId] = (position, size, shelves, spacing);

		BuildingModel!.UpdateRack(rackId, position, size, shelves, spacing);

	}

	protected override void Dispose(bool disposing) {

		base.Dispose(disposing);

		Client.StopListening(Client.IncomingMessage.Rack);

	}

}