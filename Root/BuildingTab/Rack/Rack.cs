using Godot;

using System.Collections.Generic;
using System.Linq;

namespace SuperShedAdmin.Root.BuildingTab.Rack;

public partial class Rack : Node3D {

	[Export]
	public virtual Node3D? ModelContainer { get; set; }

	[Export]
	public virtual Node3D? ProductsContainer { get; set; }

	[Export]
	public virtual CollisionShape3D? CollisionBox { get; set; }

	[Export]
	public virtual Material? NormalMaterial { get; set; }

	[Export]
	public virtual Material? SelectedMaterial { get; set; }

	[Export]
	public virtual Material? HiddenMaterial { get; set; }

	[Export]
	public virtual Material? ProductMaterial { get; set; }

	[Signal]
	public delegate void ClickedEventHandler(long mouseButtonIndex);

	public virtual string? Id { get; set; }
	public virtual Vector2I Size { get; set; }
	public virtual int Shelves { get; set; }
	public virtual float Spacing { get; set; }

	public virtual List<MeshInstance3D> Parts { get; set; } = new();

	public virtual void OnCollisionAreaInputEvent(Node camera,
													InputEvent inputEvent,
													Vector3 position,
													Vector3 normal,
													int shapeIndex) {

		if(inputEvent is not InputEventMouseButton inputEventMouseButton) {

			return;

		}

		if(!inputEventMouseButton.Pressed) {

			return;

		}

		EmitSignal(SignalName.Clicked, (long) inputEventMouseButton.ButtonIndex);

	}

	public virtual void UpdateVisuals(bool setSelected = false) {

		Parts.Clear();

		foreach(Node modelPart in ModelContainer!.GetChildren()) {

			modelPart.QueueFree();

		}

		for(int i = 0; i < Shelves; i++) {

			const float shelfThickness = 0.05f;

			MeshInstance3D shelf = new() {

				Mesh = new BoxMesh() {

					Size = new(Size.X, shelfThickness, Size.Y),
					Material = NormalMaterial

				}

			};

			Parts.Add(shelf);

			ModelContainer!.AddChild(shelf);

			shelf.Translate(new(0, ((i + 1) * Spacing) - (shelfThickness / 2), 0));

		}

		for(int i = 0; i < 4; i++) {

			const float legSide = 0.05f;

			float height = Shelves * Spacing;

			MeshInstance3D leg = new() {

				Mesh = new BoxMesh() {

					Size = new(legSide, height, legSide),
					Material = NormalMaterial

				}

			};

			Parts.Add(leg);

			ModelContainer!.AddChild(leg);

			int xMultiplier = i % 2 == 0 ? 1 : -1;
			int zMultiplier = i >= 2 ? -1 : 1;

			leg.Translate(new((xMultiplier * (Size.X / 2f)) - (xMultiplier * (legSide / 2)),
											height / 2,
											(zMultiplier * (Size.Y / 2f)) - (zMultiplier * (legSide / 2))));

		}

		float collisionBoxHeight = Shelves * Spacing;

		(CollisionBox!.Shape as BoxShape3D)!.Size = new(Size.X, collisionBoxHeight, Size.Y);

		CollisionBox.Position = new(0, collisionBoxHeight / 2, 0);

		if(setSelected) {

			SetSelected(true);

		}

		ProductsContainer!.Position = new(0, 0, -Size.Y / 2f);

	}

	public virtual void SetSelected(bool isSelected) =>
		ApplyMaterial((isSelected ? SelectedMaterial : NormalMaterial)!,
						(isSelected ? SelectedMaterial : ProductMaterial)!);

	public virtual void SetHidden(bool isHidden) =>
		ApplyMaterial((isHidden ? HiddenMaterial : NormalMaterial)!,
						(isHidden ? HiddenMaterial : ProductMaterial)!);

	protected virtual void ApplyMaterial(Material partMaterial, Material productMaterial) {

		foreach(MeshInstance3D part in Parts) {

			for(int i = 0; i < part.Mesh.GetSurfaceCount(); i++) {

				part.Mesh.SurfaceSetMaterial(i, partMaterial);

			}

		}

		foreach(MeshInstance3D product in ProductsContainer!.GetChildren().Cast<MeshInstance3D>()) {

			for(int i = 0; i < product.Mesh.GetSurfaceCount(); i++) {

				product.Mesh.SurfaceSetMaterial(i, productMaterial);

			}

		}

	}

	public virtual void UpdateProduct(string productId, Vector3 size, Vector2I position) {

		MeshInstance3D? productModel = ProductsContainer!.GetNode(productId) as MeshInstance3D;

		bool exists = productModel != null;

		if(!exists) {

			productModel = new MeshInstance3D();

		}

		productModel!.Name = productId;

		productModel.Mesh = new BoxMesh() {

			Size = size,
			Material = ProductMaterial

		};

		productModel.Position = new(0, position.X * Spacing, ((position.Y - 1) * 0.5f) + 0.25f);

		productModel.Translate(new(0, size.Y / 2, 0));

		if(!exists) {

			ProductsContainer.AddChild(productModel);

		}

	}

}