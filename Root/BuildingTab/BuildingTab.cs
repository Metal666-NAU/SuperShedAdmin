using Godot;

using SuperShedAdmin.Networking;

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
	public virtual BuildingModel? BuildingModel { get; set; }

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

	public virtual void OnViewToggled(bool primaryView) {

		PrimaryViewButton!.ButtonPressed = primaryView;
		SecondaryViewButton!.ButtonPressed = !primaryView;

		PrimaryView!.Visible = primaryView;
		SecondaryView!.Visible = !primaryView;

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

}