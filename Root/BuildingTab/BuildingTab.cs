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
	public virtual Panel? RackSettingsPanel { get; set; }

	[Export]
	public virtual SpinBox? RackXInput { get; set; }

	[Export]
	public virtual SpinBox? RackZInput { get; set; }

	[Export]
	public virtual SpinBox? RackWidthInput { get; set; }

	[Export]
	public virtual SpinBox? RackLengthInput { get; set; }

	[Export]
	public virtual SpinBox? RackShelvesInput { get; set; }

	[Export]
	public virtual SpinBox? RackSpacingInput { get; set; }

	[Export]
	public virtual SpinBox? RackRotationInput { get; set; }

	[Export]
	public virtual Button? SaveRackSettingsButton { get; set; }

	[Export]
	public virtual Button? DeleteRackButton { get; set; }

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

	public virtual Dictionary<string, (Vector2I Position, Vector2I Size, int Shelves, float Spacing, float Rotation)> Racks { get; set; }
		= new();

	public virtual bool ReallyWantsToDeleteRack { get; set; }

	public virtual void OnViewToggled(bool primaryView) {

		PrimaryViewButton!.ButtonPressed = primaryView;
		SecondaryViewButton!.ButtonPressed = !primaryView;

		PrimaryView!.Visible = primaryView;
		SecondaryView!.Visible = !primaryView;

	}

	public virtual void OnBuildingModelViewportContainerGuiInput(InputEvent inputEvent) {

		if(inputEvent is InputEventMouseButton inputEventMouseButton) {

			switch(inputEventMouseButton.ButtonIndex) {

				case MouseButton.Right: {

					if(!inputEventMouseButton.Pressed) {

						break;

					}

					BuildingModelPopupMenu!.Position =
						new(Mathf.RoundToInt(inputEventMouseButton.GlobalPosition.X),
									Mathf.RoundToInt(inputEventMouseButton.GlobalPosition.Y));

					BuildingModelPopupMenu.Popup();

					break;

				}

				case MouseButton.WheelUp: {

					BuildingModel!.Zoom(true);

					break;

				}

				case MouseButton.WheelDown: {

					BuildingModel!.Zoom(false);

					break;

				}

				case MouseButton.Middle: {

					BuildingModel!.ToggleGrabCamera();

					if(inputEventMouseButton.Pressed) {

						Input.MouseMode = Input.MouseModeEnum.Captured;

					}

					else {

						Input.MouseMode = Input.MouseModeEnum.Visible;

					}

					break;

				}

			}

		}

		if(inputEvent is InputEventMouseMotion inputEventMouseMotion) {

			BuildingModel!.MoveCamera(inputEventMouseMotion.Relative);

		}

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

	public virtual void OnRackSettingsChanged(float _) {

		string? selectedRack = BuildingModel!.SelectedRack;

		if(selectedRack == null) {

			GD.PushError("Can't handle Rack settings update: no Rack is selected!");

			return;

		}

		(Vector2I Position, Vector2I Size, int Shelves, float Spacing, float Rotation)
			= Racks[selectedRack];

		Vector2I inputPosition = new(Mathf.RoundToInt(RackXInput!.Value),
											Mathf.RoundToInt(RackZInput!.Value));
		Vector2I inputSize = new(Mathf.RoundToInt(RackWidthInput!.Value),
										Mathf.RoundToInt(RackLengthInput!.Value));
		int inputShelves = Mathf.RoundToInt(RackShelvesInput!.Value);
		float inputSpacing = (float) RackSpacingInput!.Value;
		float inputRotation = (float) RackRotationInput!.Value;

		SaveRackSettingsButton!.Disabled =
			Position.Equals(inputPosition) &&
			Size.Equals(inputSize) &&
			Shelves.Equals(inputShelves) &&
			Spacing.Equals(inputSpacing) &&
			Rotation.Equals(inputRotation);

		BuildingModel.UpdateRack(selectedRack,
									inputPosition,
									inputSize,
									inputShelves,
									inputSpacing,
									inputRotation,
									true);

	}

	public virtual void OnSaveRackSettingsButtonPressed() {

		Client.SendUpdateRack(BuildingModel!.SelectedRack!,
								Mathf.RoundToInt(RackXInput!.Value),
								Mathf.RoundToInt(RackZInput!.Value),
								Mathf.RoundToInt(RackWidthInput!.Value),
								Mathf.RoundToInt(RackLengthInput!.Value),
								Mathf.RoundToInt(RackShelvesInput!.Value),
								(float) RackSpacingInput!.Value,
								(float) RackRotationInput!.Value);

		BuildingModel!.DeselectRack();

		RackSettingsPanel!.Hide();

	}

	public virtual void OnCancelEditingRackSettingsButtonPressed() {

		RackSettingsPanel!.Hide();

		(Vector2I Position, Vector2I Size, int Shelves, float Spacing, float Rotation)
			= Racks[BuildingModel!.SelectedRack!];

		BuildingModel.UpdateRack(BuildingModel.SelectedRack!,
									Position,
									Size,
									Shelves,
									Spacing,
									Rotation,
									false);

	}

	public virtual void OnDeleteRackButtonPressed() {

		if(ReallyWantsToDeleteRack) {

			Client.SendDeleteRack(BuildingModel!.SelectedRack!);

			RackSettingsPanel!.Hide();

			return;

		}

		ReallyWantsToDeleteRack = true;

		DeleteRackButton!.Text = "Delete! :O";

	}

	public override void _Ready() {

		ResetBuildingSizeInputs();

		BuildingModel!.SetSize(Building!.Size);

		RackSettingsPanel!.Hide();

		BuildingModel.RackSelected += () => {

			RackSettingsPanel!.Show();

			ReallyWantsToDeleteRack = false;

			DeleteRackButton!.Text = "Delete?";

			(Vector2I Position, Vector2I Size, int Shelves, float Spacing, float Rotation) =
				Racks[BuildingModel!.SelectedRack!];

			RackXInput!.SetValueNoSignal(Position.X);
			RackZInput!.SetValueNoSignal(Position.Y);

			RackWidthInput!.SetValueNoSignal(Size.X);
			RackLengthInput!.SetValueNoSignal(Size.Y);

			RackShelvesInput!.SetValueNoSignal(Shelves);
			RackSpacingInput!.SetValueNoSignal(Spacing);

			RackRotationInput!.SetValueNoSignal(Rotation);

		};

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
									float spacing,
									float rotation) {

		Racks[rackId] = (position, size, shelves, spacing, rotation);

		BuildingModel!.UpdateRack(rackId, position, size, shelves, spacing, rotation);

	}

	public virtual void RemoveRack(string rackId) {

		Racks.Remove(rackId);

		BuildingModel!.RemoveRack(rackId);

	}

}