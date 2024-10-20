using Godot;

using Metal666.GodotUtilities.Extensions;

using Net.Codecrete.QrCodeGenerator;

using SuperShedAdmin.Networking;

using System.Collections.Generic;
using System.Linq;

using static SuperShedAdmin.Root.Root;

namespace SuperShedAdmin.Root.BuildingTab;

public partial class BuildingTab : Control {

#nullable disable
	[Export]
	public virtual Button PrimaryViewButton { get; set; }

	[Export]
	public virtual Button SecondaryViewButton { get; set; }

	[Export]
	public virtual Panel PrimaryView { get; set; }

	[Export]
	public virtual Viewport BuildingModelViewport { get; set; }

	[Export]
	public virtual BuildingModel BuildingModel { get; set; }

	[Export]
	public virtual Button StopObservingRackButton { get; set; }

	[Export]
	public virtual Panel RackSettingsPanel { get; set; }

	[Export]
	public virtual LineEdit RackIdOutput { get; set; }

	[Export]
	public virtual SpinBox RackXInput { get; set; }

	[Export]
	public virtual SpinBox RackZInput { get; set; }

	[Export]
	public virtual SpinBox RackWidthInput { get; set; }

	[Export]
	public virtual SpinBox RackLengthInput { get; set; }

	[Export]
	public virtual SpinBox RackShelvesInput { get; set; }

	[Export]
	public virtual SpinBox RackSpacingInput { get; set; }

	[Export]
	public virtual SpinBox RackRotationInput { get; set; }

	[Export]
	public virtual Button SaveRackSettingsButton { get; set; }

	[Export]
	public virtual Button DeleteRackButton { get; set; }

	[Export]
	public virtual Panel SecondaryView { get; set; }

	[Export]
	public virtual PopupMenu GroupByPopup { get; set; }

	[Export]
	public virtual Godot.Tree ProductsTree { get; set; }

	[Export]
	public virtual VBoxContainer ProductQRCodePanel { get; set; }

	[Export]
	public virtual TextureRect ProductQRCodeTexture { get; set; }

	[Export]
	public virtual LineEdit ProductIdOutput { get; set; }

	[Export]
	public virtual SpinBox BuildingWidthInput { get; set; }

	[Export]
	public virtual SpinBox BuildingLengthInput { get; set; }

	[Export]
	public virtual SpinBox BuildingHeightInput { get; set; }

	[Export]
	public virtual Button EditBuildingSizeButton { get; set; }

	[Export]
	public virtual Button SaveBuildingSizeButton { get; set; }

	[Export]
	public virtual Button CancelEditingBuildingSizeButton { get; set; }

	public virtual Building Building { get; set; }
#nullable enable

	public virtual Vector3I BuildingSize =>
		new Vector3(BuildingWidthInput.ValueF(),
					BuildingHeightInput.ValueF(),
					BuildingLengthInput.ValueF()).RoundToInt();
	public virtual Vector2I RackPosition =>
		new Vector2(RackXInput.ValueF(),
					RackZInput.ValueF()).RoundToInt();
	public virtual Vector2I RackSize =>
		new Vector2(RackWidthInput.ValueF(),
					RackLengthInput.ValueF()).RoundToInt();

	public virtual Dictionary<string, RackData> Racks { get; set; } =
		[];
	public virtual Dictionary<string, ProductData> Products { get; set; } =
		[];

	public virtual ProductGroups GroupProductsBy { get; set; }

	public virtual bool ReallyWantsToDeleteRack { get; set; }

	#region Signal Handlers
	public virtual void OnViewToggled(bool primaryView) {

		PrimaryViewButton.ButtonPressed = primaryView;
		SecondaryViewButton.ButtonPressed = !primaryView;

		PrimaryView.Visible = primaryView;
		SecondaryView.Visible = !primaryView;

	}

	public virtual void OnBuildingModelViewportContainerGuiInput(InputEvent inputEvent) {

		if(inputEvent is InputEventMouseButton inputEventMouseButton) {

			switch(inputEventMouseButton.ButtonIndex) {

				case MouseButton.WheelUp: {

					BuildingModel.Zoom(true);

					break;

				}

				case MouseButton.WheelDown: {

					BuildingModel.Zoom(false);

					break;

				}

				case MouseButton.Middle: {

					BuildingModel.ToggleGrabCamera();

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

			BuildingModel.MoveCamera(inputEventMouseMotion.Relative);

		}

	}

	public virtual void OnCreateRackButtonPressed() =>
		Client.SendCreateRack(Building.Id);

	public virtual void OnBuildingSizeChanged(float _) {

		SaveBuildingSizeButton.Disabled =
			Building.Size
					.Equals(BuildingSize);

	}

	public virtual void OnEditBuildingSizeButtonPressed() =>
		ToggleBuildingSizeEditing(true);

	public virtual void OnSaveBuildingSizeButtonPressed() {

		Vector3I buildingSize = BuildingSize;

		Client.SendUpdateBuilding(Building.Id,
									Building.Name,
									buildingSize.X,
									buildingSize.Z,
									buildingSize.Y);

		ToggleBuildingSizeEditing(false);

		Building =
			Building with {

				Size = buildingSize

			};

		BuildingModel.SetSize(buildingSize);

	}

	public virtual void OnCancelEditingBuildingSizeButtonPressed() {

		ResetBuildingSizeInputs();

		ToggleBuildingSizeEditing(false);

	}

	public virtual void OnRackSettingsChanged(float _) {

		string? selectedRack = BuildingModel.SelectedRack;

		if(selectedRack == null) {

			GD.PushError("Can't handle Rack settings update: no Rack is selected!");

			return;

		}

		(Vector2I Position, Vector2I Size, int Shelves, float Spacing, float Rotation) =
			Racks[selectedRack];

		Vector2I inputPosition = RackPosition;
		Vector2I inputSize = RackSize;
		int inputShelves = RackShelvesInput.ValueI();
		float inputSpacing = RackSpacingInput.ValueF();
		float inputRotation = RackRotationInput.ValueF();

		SaveRackSettingsButton.Disabled =
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

		Vector2I inputPosition = RackPosition;
		Vector2I inputSize = RackSize;

		Client.SendUpdateRack(BuildingModel.SelectedRack!,
								inputPosition.X,
								inputPosition.Y,
								inputSize.X,
								inputSize.Y,
								RackShelvesInput.ValueI(),
								RackSpacingInput.ValueF(),
								RackRotationInput.ValueF());

		BuildingModel.DeselectRack();

		RackSettingsPanel.Hide();

	}

	public virtual void OnCancelEditingRackSettingsButtonPressed() {

		RackSettingsPanel.Hide();

		(Vector2I Position, Vector2I Size, int Shelves, float Spacing, float Rotation) =
			Racks[BuildingModel.SelectedRack!];

		Rack.Rack rack =
			BuildingModel.UpdateRack(BuildingModel.SelectedRack!,
										Position,
										Size,
										Shelves,
										Spacing,
										Rotation,
										false);

		foreach(KeyValuePair<string, ProductData> product in
					Products.Where(product =>
												product.Value
														.RackId
														.Equals(BuildingModel.SelectedRack))) {

			rack.UpdateProduct(product.Key,
								product.Value.Name,
								product.Value.Size,
								product.Value.Position);

		}

		BuildingModel.SelectedRack = null;

	}

	public virtual void OnDeleteRackButtonPressed() {

		if(ReallyWantsToDeleteRack) {

			Client.SendDeleteRack(BuildingModel.SelectedRack!);

			RackSettingsPanel.Hide();

			return;

		}

		ReallyWantsToDeleteRack = true;

		DeleteRackButton.Text = "Delete! :O";

	}

	public virtual void OnStopObservingRackButtonPressed() {

		StopObservingRackButton.Hide();

		BuildingModel.StopObservingRack();

	}

	public virtual void OnProductsGroupingChanged(int index) {

		GroupProductsBy = (ProductGroups) index;

		UpdateProductsTree();

		for(int i = 0; i < GroupByPopup.ItemCount; i++) {

			GroupByPopup.SetItemChecked(i, false);

		}

		GroupByPopup.SetItemChecked(index, true);

		UpdateGroupByPopupName();

	}
	#endregion

	#region Node Events
	public override void _Ready() {

		ResetBuildingSizeInputs();

		BuildingModel.SetSize(Building.Size);

		RackSettingsPanel.Hide();

		BuildingModel.RackSelected += () => {

			RackSettingsPanel.Show();

			ReallyWantsToDeleteRack = false;

			DeleteRackButton.Text = "Delete?";

			RackData rackData =
				Racks[BuildingModel.SelectedRack!];

			RackIdOutput.Text = BuildingModel.SelectedRack;

			RackXInput.SetValueNoSignal(rackData.Position.X);
			RackZInput.SetValueNoSignal(rackData.Position.Y);

			RackWidthInput.SetValueNoSignal(rackData.Size.X);
			RackLengthInput.SetValueNoSignal(rackData.Size.Y);

			RackShelvesInput.SetValueNoSignal(rackData.Shelves);
			RackSpacingInput.SetValueNoSignal(rackData.Spacing);

			RackRotationInput.SetValueNoSignal(rackData.Rotation);

		};

		BuildingModel.RackObserved +=
			StopObservingRackButton.Show;

		UpdateGroupByPopupName();

	}
	#endregion

	public virtual void ToggleBuildingSizeEditing(bool editing) {

		EditBuildingSizeButton.Visible = !editing;

		SaveBuildingSizeButton.Visible = editing;
		CancelEditingBuildingSizeButton.Visible = editing;

		SaveBuildingSizeButton.Disabled = true;

		BuildingWidthInput.Editable = editing;
		BuildingLengthInput.Editable = editing;
		BuildingHeightInput.Editable = editing;

	}

	public virtual void ResetBuildingSizeInputs() {

		BuildingWidthInput.Value = Building.Size.X;
		BuildingLengthInput.Value = Building.Size.Z;
		BuildingHeightInput.Value = Building.Size.Y;

	}

	public virtual void UpdateRack(string rackId,
									Vector2I position,
									Vector2I size,
									int shelves,
									float spacing,
									float rotation) {

		Racks[rackId] =
			new(position,
						size,
						shelves,
						spacing,
						rotation);

		Rack.Rack rack =
			BuildingModel.UpdateRack(rackId,
										position,
										size,
										shelves,
										spacing,
										rotation);

		foreach(KeyValuePair<string, ProductData> product in
					Products.Where(product =>
												product.Value
														.RackId
														.Equals(rackId))) {

			rack.UpdateProduct(product.Key,
								product.Value.Name,
								product.Value.Size,
								product.Value.Position);

		}

	}

	public virtual void UpdateProduct(string productId,
										Vector3 productSize,
										string productManufacturer,
										string rackId,
										Vector2I productPosition,
										string productName,
										string productCategory) {

		Products[productId] =
			new(productSize,
							productManufacturer,
							rackId,
							productPosition,
							productName,
							productCategory);

		BuildingModel.UpdateProduct(productId,
									rackId,
									productName,
									productSize,
									productPosition);

		UpdateProductsTree();

	}

	public virtual void RemoveRack(string rackId) {

		Racks.Remove(rackId);

		BuildingModel.RemoveRack(rackId);

	}

	public virtual void UpdateProductsTree() {

		ProductsTree.Clear();

		ProductsTree.CreateItem();

		ProductsTree.SetColumnTitle(0, "Name");

		switch(GroupProductsBy) {

			case ProductGroups.Category: {

				ProductsTree.SetColumnTitle(1, "Manufacturer");

				break;

			}

			case ProductGroups.Manufacturer: {

				ProductsTree.SetColumnTitle(1, "Category");

				break;

			}

		}

		ProductsTree.SetColumnTitle(2, "Size");
		ProductsTree.SetColumnTitle(3, "Location");
		ProductsTree.SetColumnTitle(4, "QR Code");

		for(int i = 0; i < ProductsTree.Columns; i++) {

			ProductsTree.SetColumnTitleAlignment(i, HorizontalAlignment.Left);

		}

		List<TreeItem> groupItems =
			Products.Values
					.Select(productInfo =>
										GroupProductsBy switch {

											ProductGroups.Category => productInfo.Category,
											ProductGroups.Manufacturer => productInfo.Manufacturer,
											_ => productInfo.Name

										})
					.Distinct()
					.Select(group => {

						TreeItem treeItem = ProductsTree.CreateItem();

						treeItem.SetText(0, group);

						return treeItem;

					}).ToList();

		foreach(KeyValuePair<string, ProductData> product in Products) {

			TreeItem treeItem =
				ProductsTree.CreateItem(groupItems.Single(groupItem =>
																			groupItem.GetText(0)
																						.Equals(GroupProductsBy switch {

																							ProductGroups.Category =>
																								product.Value.Category,
																							ProductGroups.Manufacturer =>
																								product.Value.Manufacturer,
																							_ => null

																						})));

			treeItem.SetText(0, product.Value.Name);

			treeItem.SetText(1,
								GroupProductsBy switch {

									ProductGroups.Category => product.Value.Manufacturer,
									ProductGroups.Manufacturer => product.Value.Category,
									_ => ""

								});

			Vector3 size = product.Value.Size;

			treeItem.SetText(2, $"{size.X} x {size.Y} x {size.Z}");

			Vector2I rackPosition = Racks[product.Value.RackId].Position;
			Vector2I productPosition = product.Value.Position;

			treeItem.SetText(3, $"Rack {rackPosition.X}:{rackPosition.Y}, Shelf {productPosition.X}, Spot {productPosition.Y}");

			treeItem.SetText(4, "Show");

			treeItem.SetMeta("productId", product.Key);

		}

		ProductsTree.CellSelected += () => {

			TreeItem selectedItem = ProductsTree.GetSelected();

			if(groupItems.Contains(selectedItem)) {

				ProductQRCodePanel.Hide();

				return;

			}

			if(ProductsTree.GetSelectedColumn() != 4) {

				return;

			}

			string productId = selectedItem.GetMeta("productId").AsString();

			QrCode qrCode = QrCode.EncodeText(productId, QrCode.Ecc.High);

			int size = qrCode.Size + 4;

			Image qrCodeImage =
				Image.CreateEmpty(size,
									size,
									false,
									Image.Format.L8);

			qrCodeImage.Fill(Colors.White);

			for(int i = 2; i < size - 2; i++) {

				for(int j = 2; j < size - 2; j++) {

					qrCodeImage.SetPixel(i,
											j,
											qrCode.GetModule(i - 2, j - 2) ?
												Colors.Black :
												Colors.White);

				}

			}

			ProductQRCodeTexture.Texture = ImageTexture.CreateFromImage(qrCodeImage);

			ProductQRCodeTexture.TextureFilter = TextureFilterEnum.Nearest;

			ProductQRCodePanel.Show();

			ProductIdOutput.Text = productId;

		};

		ProductsTree.EmptyClicked +=
			(position, mouseButtonIndex) =>
				ProductQRCodePanel.Hide();

	}

	public virtual void UpdateGroupByPopupName() =>
		GroupByPopup.Name = $"Group By {GroupProductsBy}";

	public record RackData(Vector2I Position,
							Vector2I Size,
							int Shelves,
							float Spacing,
							float Rotation);

	public record ProductData(Vector3 Size,
								string Manufacturer,
								string RackId,
								Vector2I Position,
								string Name,
								string Category);

	public enum ProductGroups {

		Category,
		Manufacturer

	}

}