using Godot;

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

	public virtual void SetSize(Vector3I size) {

		(Floor!.Mesh as PlaneMesh)!.Size = new(size.X, size.Z);

		(NorthWall!.Mesh as PlaneMesh)!.Size = new(size.X, size.Y);
		NorthWall.Position = new(0, size.Y / 2f, -size.Z / 2f);

		(EastWall!.Mesh as PlaneMesh)!.Size = new(size.Z, size.Y);
		EastWall.Position = new(size.X / 2f, size.Y / 2f, 0);

		(SouthWall!.Mesh as PlaneMesh)!.Size = new(size.X, size.Y);
		SouthWall.Position = new(0, size.Y / 2f, size.Z / 2f);

		(WestWall!.Mesh as PlaneMesh)!.Size = new(size.Z, size.Y);
		WestWall.Position = new(-size.X / 2f, size.Y / 2f, 0);

	}

}