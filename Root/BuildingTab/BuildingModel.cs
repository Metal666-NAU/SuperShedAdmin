using Godot;

using System.Linq;

namespace SuperShedAdmin.Root.BuildingTab;

public partial class BuildingModel : Node3D {

	[Export]
	public virtual MeshInstance3D? Floor { get; set; }

	[Export]
	public virtual MeshInstance3D? NorthWall { get; set; }

	[Export]
	public virtual MeshInstance3D? EastWall { get; set; }

	[Export]
	public virtual MeshInstance3D? SouthWall { get; set; }

	[Export]
	public virtual MeshInstance3D? WestWall { get; set; }

	[Export]
	public virtual Node3D? RackContainer { get; set; }

	[Export]
	public virtual PackedScene? Rack { get; set; }

	public virtual string? SelectedRack { get; set; }

	[Signal]
	public delegate void RackSelectedEventHandler();

	public virtual void SetSize(Vector3I size) {

		const float wallThickness = 0.1f;

		(Floor!.Mesh as PlaneMesh)!.Size = new(size.X, size.Z);

		(NorthWall!.Mesh as BoxMesh)!.Size = new(size.X, size.Y, wallThickness);
		NorthWall.Position = new(0, size.Y / 2f, -(wallThickness / 2) - size.Z / 2f);

		(EastWall!.Mesh as BoxMesh)!.Size = new(wallThickness, size.Y, size.Z);
		EastWall.Position = new((wallThickness / 2) + size.X / 2f, size.Y / 2f, 0);

		(SouthWall!.Mesh as BoxMesh)!.Size = new(size.X, size.Y, wallThickness);
		SouthWall.Position = new(0, size.Y / 2f, (wallThickness / 2) + size.Z / 2f);

		(WestWall!.Mesh as BoxMesh)!.Size = new(wallThickness, size.Y, size.Z);
		WestWall.Position = new(-(wallThickness / 2) - size.X / 2f, size.Y / 2f, 0);

	}

	public void UpdateRack(string rackId,
							Vector2I position,
							Vector2I size,
							int shelves,
							float spacing,
							float rotation,
							bool isSelected = false) {

		Rack.Rack? rack = GetRack(rackId);

		bool exists = rack != null;

		if(!exists) {

			rack = Rack!.Instantiate<Rack.Rack>();

			rack.Id = rackId;

			rack.Clicked += () => {

				if(SelectedRack != null) {

					return;

				}

				SelectedRack = rackId;

				rack.SetSelected(true);

				EmitSignal(SignalName.RackSelected);

			};

		}

		rack!.Size = size;
		rack.Shelves = shelves;
		rack.Spacing = spacing;

		rack.UpdateVisuals(isSelected);

		if(!exists) {

			RackContainer!.AddChild(rack);

		}

		rack.Position = new(position.X, 0, -position.Y);
		rack.Rotation = new();
		rack.RotateY(Mathf.DegToRad(rotation));

	}

	public virtual Rack.Rack? GetRack(string? rackId = null) =>
		RackContainer!.GetChildren()
						.Cast<Rack.Rack>()
						.SingleOrDefault(rack =>
											rack.Id!.Equals(rackId ?? SelectedRack));

	public virtual void DeselectRack() {

		GetRack(SelectedRack)?.SetSelected(false);

		SelectedRack = null;

	}

	public virtual void RemoveRack(string rackId) =>
		GetRack(SelectedRack)?.QueueFree();

}