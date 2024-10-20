using Godot;

using Metal666.GodotUtilities.Extensions;

using System.Collections.Generic;
using System.Linq;

namespace SuperShedAdmin.Root.BuildingTab.Rack;

public partial class Rack : Node3D {

#nullable disable
	[Export]
	public virtual Node3D ModelContainer { get; set; }

	[Export]
	public virtual Node3D ProductsContainer { get; set; }

	[Export]
	public virtual CollisionShape3D CollisionBox { get; set; }

	[Export]
	public virtual Material NormalMaterial { get; set; }

	[Export]
	public virtual Material SelectedMaterial { get; set; }

	[Export]
	public virtual Material HiddenMaterial { get; set; }

	[Export]
	public virtual Material ProductMaterial { get; set; }

	public virtual string Id { get; set; }
	public virtual Vector2I Size { get; set; }
	public virtual int Shelves { get; set; }
	public virtual float Spacing { get; set; }
#nullable enable

	public virtual IEnumerable<MeshInstance3D> Products =>
		ProductsContainer.GetChildren()
							.Cast<MeshInstance3D>();

	public virtual List<MeshInstance3D> Parts { get; set; } = [];

	[Signal]
	public delegate void ClickedEventHandler(long mouseButtonIndex);

	#region Signal Handlers
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
	#endregion

	public virtual void UpdateVisuals(bool setSelected = false) {

		Parts.Clear();

		foreach(Node modelPart in ModelContainer.GetChildren()) {

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

			ModelContainer.AddChild(shelf);

			shelf.Translate(Vector3.Up * (((i + 1) * Spacing) - (shelfThickness / 2)));

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

			ModelContainer.AddChild(leg);

			int xMultiplier = i % 2 == 0 ? 1 : -1;
			int zMultiplier = i >= 2 ? -1 : 1;

			leg.Translate(new((xMultiplier * (Size.X / 2f)) - (xMultiplier * (legSide / 2)),
											height / 2,
											(zMultiplier * (Size.Y / 2f)) - (zMultiplier * (legSide / 2))));

		}

		float collisionBoxHeight = Shelves * Spacing;

		(CollisionBox.Shape as BoxShape3D)!.Size =
			new(Size.X, collisionBoxHeight, Size.Y);

		CollisionBox.Position = Vector3.Up * collisionBoxHeight / 2;

		if(setSelected) {

			SetSelected(true);

		}

		ProductsContainer.Position = Vector3.Forward * (Size.Y / 2f);

	}

	public virtual void SetSelected(bool isSelected) {

		Material partMaterial =
			isSelected ? SelectedMaterial : NormalMaterial;

		foreach(MeshInstance3D part in Parts) {

			for(int i = 0; i < part.Mesh.GetSurfaceCount(); i++) {

				part.Mesh.SurfaceSetMaterial(i, partMaterial);

			}

		}

		Material productMaterial =
			isSelected ? SelectedMaterial : ProductMaterial;

		foreach(MeshInstance3D product in Products) {

			for(int i = 0; i < product.Mesh.GetSurfaceCount(); i++) {

				product.Mesh.SurfaceSetMaterial(i, productMaterial);

			}

		}

	}

	public virtual void SetHidden(bool isHidden) =>
		Visible = !isHidden;

	public virtual void UpdateProduct(string productId, string productName, Vector3 size, Vector2I position) {

		MeshInstance3D? productModel =
			Products.FirstOrDefault(product =>
												product.Name == productId);

		Label3D? productInfo =
			productModel?.GetChildOrNull<Label3D>(0);

		bool exists = productModel != null;

		if(!exists) {

			productModel = new MeshInstance3D();

			productModel.RotateYDeg(-90);

			productInfo = new() {

				FontSize = 40,
				OutlineSize = 16,
				PixelSize = 0.0015f,
				Width = 350,
				AutowrapMode = TextServer.AutowrapMode.Word,
				NoDepthTest = true

			};

			productModel.AddChild(productInfo);

			productInfo.Position =
				Vector3.Back * (Size.X / 2f);

		}

		productModel!.Name = productId;

		productModel.Mesh = new BoxMesh() {

			Size = size,
			Material = ProductMaterial

		};

		productModel.Position =
			new(0,
						position.X * Spacing + size.Y / 2,
						(position.Y * 0.5f) - 0.25f + ((size.X * 2).FloorToInt() * 0.25f));

		productInfo!.Text =
			$"{productName}\n" +
			$"{size.X} x {size.Y} x {size.Z}\n" +
			$"Shelf {position.X}\n" +
			$"Spot {position.Y}";

		if(!exists) {

			ProductsContainer.AddChild(productModel);

		}

	}

}