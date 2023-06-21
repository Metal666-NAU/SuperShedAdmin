using Godot;

using System.Linq;

namespace SuperShedAdmin.Root.BuildingTab;

public partial class BuildingModel : Node3D {

	[Export]
	public virtual Node3D? CameraOrigin { get; set; }

	[Export]
	public virtual Node3D? CameraPivot { get; set; }

	[Export]
	public virtual Camera3D? Camera { get; set; }

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

	[Export]
	public virtual float CameraZoomSpeed { get; set; }

	[Export]
	public virtual float CameraZoomSmoothing { get; set; }

	[Export]
	public virtual float CameraMoveSpeed { get; set; }

	[Export]
	public virtual float CameraMoveSmoothing { get; set; }

	public virtual float TargetCameraZoom { get; set; }

	public virtual Vector3 TargetCameraPosition { get; set; }

	public virtual bool IsMovingCamera { get; set; }

	public virtual string? SelectedRack { get; set; }

	[Signal]
	public delegate void RackSelectedEventHandler();

	public override void _Ready() {

		TargetCameraZoom = Camera!.Position.Z;

	}

	public override void _Process(double delta) {

		Camera!.Position =
			Camera.Position.Lerp(new Vector3(0, 0, TargetCameraZoom),
									CameraZoomSmoothing);

		CameraOrigin!.Position =
			CameraOrigin.Position.Lerp(TargetCameraPosition,
										CameraMoveSmoothing);

	}

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

	public virtual void Zoom(bool forward) {

		TargetCameraZoom += (forward ? -1 : 1) * CameraZoomSpeed;

		TargetCameraZoom = Mathf.Clamp(TargetCameraZoom, CameraZoomSpeed, 100);

	}

	public virtual void ToggleGrabCamera() =>
		IsMovingCamera = !IsMovingCamera;

	public virtual void MoveCamera(Vector2 relativePosition) {

		if(!IsMovingCamera) {

			return;

		}

		TargetCameraPosition -=
			new Vector3(relativePosition.X, 0, relativePosition.Y)
			* CameraMoveSpeed
			* Mathf.Sqrt(TargetCameraZoom);

	}

}