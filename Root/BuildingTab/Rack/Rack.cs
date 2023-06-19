using Godot;

using System;
using System.Collections.Generic;

namespace SuperShedAdmin.Root.BuildingTab.Rack;

public partial class Rack : Node3D {

	[Export]
	public virtual Node3D? ModelContainer { get; set; }

	[Export]
	public virtual CollisionShape3D? CollisionBox { get; set; }

	[Export]
	public virtual Material? NormalMaterial { get; set; }

	[Export]
	public virtual Material? SelectedMaterial { get; set; }

	public virtual string? Id { get; set; }
	public virtual Vector2I Size { get; set; }
	public virtual int Shelves { get; set; }
	public virtual float Spacing { get; set; }

	public event Action? Selected;

	public virtual List<MeshInstance3D> Part { get; set; } = new();

	public virtual bool IsSelected { get; set; }

	public virtual void OnCollisionAreaInputEvent(Node camera,
													InputEvent inputEvent,
													Vector3 position,
													Vector3 normal,
													int shapeIndex) {

		if(inputEvent is not InputEventMouseButton inputEventMouseButton) {

			return;

		}

		if(inputEventMouseButton.ButtonIndex != MouseButton.Left) {

			return;

		}

		if(!inputEventMouseButton.Pressed) {

			return;

		}

		IsSelected = !IsSelected;

		foreach(MeshInstance3D part in Part) {

			for(int i = 0; i < part.Mesh.GetSurfaceCount(); i++) {

				part.Mesh.SurfaceSetMaterial(i, IsSelected ? SelectedMaterial : NormalMaterial);

			}

		}

		if(IsSelected) {

			Selected?.Invoke();

		}

	}

	public override void _Ready() {

		for(int i = 0; i < Shelves; i++) {

			const float shelveThickness = 0.05f;

			MeshInstance3D shelve = new() {

				Mesh = new BoxMesh() {

					Size = new(Size.X, shelveThickness, Size.Y),
					Material = NormalMaterial

				}

			};

			Part.Add(shelve);

			ModelContainer!.AddChild(shelve);

			shelve.Translate(new(0, ((i + 1) * Spacing) - (shelveThickness / 2), 0));

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

			Part.Add(leg);

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

	}

}