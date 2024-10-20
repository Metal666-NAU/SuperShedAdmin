using Godot;

using Metal666.GodotUtilities.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperShedAdmin.Root.BuildingTab;

public partial class BuildingModel : Node3D {

#nullable disable
	[Export]
	public virtual Node3D CameraOrigin { get; set; }

	[Export]
	public virtual Node3D CameraPivot { get; set; }

	[Export]
	public virtual Node3D CameraDolly { get; set; }

	[Export]
	public virtual Node3D CameraObserveOrigin { get; set; }

	[Export]
	public virtual Node3D CameraObservePivot { get; set; }

	[Export]
	public virtual Camera3D Camera { get; set; }

	[Export]
	public virtual MeshInstance3D Floor { get; set; }

	[Export]
	public virtual MeshInstance3D NorthWall { get; set; }

	[Export]
	public virtual MeshInstance3D EastWall { get; set; }

	[Export]
	public virtual MeshInstance3D SouthWall { get; set; }

	[Export]
	public virtual MeshInstance3D WestWall { get; set; }

	[Export]
	public virtual Node3D RacksContainer { get; set; }

	[Export]
	public virtual PackedScene RackScene { get; set; }

	[Export]
	public virtual float CameraZoomSpeed { get; set; }

	[Export]
	public virtual float CameraZoomSmoothing { get; set; }

	[Export]
	public virtual float CameraMoveSpeed { get; set; }

	[Export]
	public virtual float CameraMoveSmoothing { get; set; }
#nullable enable

	public virtual IEnumerable<Rack.Rack> Racks =>
		RacksContainer.GetChildren()
						.Cast<Rack.Rack>();

	public virtual float TargetCameraZoom { get; set; }

	public virtual Vector3 TargetCameraPosition { get; set; }

	public virtual bool IsMovingCamera { get; set; }

	public virtual string? SelectedRack { get; set; }

	public virtual string? ObservedRack { get; set; }

	public event Action? RackObserved;

	public event Action? RackSelected;

	#region Node Events
	public override void _Ready() {

		TargetCameraZoom = CameraDolly.Position.Z;

	}
	#endregion

	public override void _Process(double delta) {

		CameraDolly.Position =
			CameraDolly.Position
						.Lerp(new Vector3(0, 0, TargetCameraZoom),
								CameraZoomSmoothing);

		CameraOrigin.Position =
			CameraOrigin.Position
						.Lerp(TargetCameraPosition,
								CameraMoveSmoothing);

	}

	public virtual void SetSize(Vector3I size) {

		Vector3 halfSize = (Vector3) size / 2f;

		const float wallThickness = 0.1f;
		const float halfWallThickness = 0.1f / 2;

		(Floor.Mesh as PlaneMesh)!.Size = new(size.X, size.Z);

		(NorthWall.Mesh as BoxMesh)!.Size = new(size.X, size.Y, wallThickness);
		NorthWall.Position = new(0, halfSize.Y, -halfWallThickness - halfSize.Z);

		(EastWall.Mesh as BoxMesh)!.Size = new(wallThickness, size.Y, size.Z);
		EastWall.Position = new(halfWallThickness + halfSize.X, halfSize.Y, 0);

		(SouthWall.Mesh as BoxMesh)!.Size = new(size.X, size.Y, wallThickness);
		SouthWall.Position = new(0, halfSize.Y, halfWallThickness + halfSize.Z);

		(WestWall.Mesh as BoxMesh)!.Size = new(wallThickness, size.Y, size.Z);
		WestWall.Position = new(-halfWallThickness - halfSize.X, halfSize.Y, 0);

	}

	public virtual Rack.Rack UpdateRack(string rackId,
										Vector2I position,
										Vector2I size,
										int shelves,
										float spacing,
										float rotation,
										bool isSelected = false) {

		Rack.Rack? rack = GetRack(rackId);

		bool exists =
			rack != null;

		if(!exists) {

			rack = RackScene.Instantiate<Rack.Rack>();

			rack.Id = rackId;

			rack.Clicked += (long mouseButtonIndex) => {

				if(ObservedRack != null) {

					return;

				}

				switch((MouseButton) mouseButtonIndex) {

					case MouseButton.Left: {

						if(SelectedRack != null) {

							break;

						}

						ObservedRack = rackId;

						Vector3 rackRotation =
							rack.RotationDegrees - new Vector3(0, 90, 0);

						float rackHeight = rack.Shelves * rack.Spacing;
						float rackWidth = rack.Size.X;
						float rackLength = rack.Size.Y;

						float cameraTilt = -15.ToRad();

						CameraObserveOrigin.Position = rack.Position;
						CameraObserveOrigin.RotationDegrees = rackRotation;

						Camera.KeepAspect =
							(rackHeight + rackWidth) >= rackLength ?
								Camera3D.KeepAspectEnum.Height :
								Camera3D.KeepAspectEnum.Width;

						CameraObservePivot.Position =
							new(0,
										rackHeight,
										rackHeight / 2 / Mathf.Tan(Mathf.Abs(cameraTilt)));

						CameraObservePivot.RotateX(cameraTilt);

						Camera.Projection = Camera3D.ProjectionType.Orthogonal;

						Camera.Size = Mathf.Max(rackLength, rackHeight) + 1;

						CameraDolly.RemoveChild(Camera);
						CameraObservePivot.AddChild(Camera);

						foreach(Rack.Rack otherRack in Racks) {

							if(otherRack == rack) {

								continue;

							}

							otherRack.SetHidden(true);

						}

						RackObserved?.Invoke();

						break;

					}

					case MouseButton.Right: {

						if(SelectedRack != null) {

							break;

						}

						SelectedRack = rackId;

						rack.SetSelected(true);

						RackSelected?.Invoke();

						break;

					}

				}

			};

		}

		rack!.Size = size;
		rack.Shelves = shelves;
		rack.Spacing = spacing;

		rack.UpdateVisuals(isSelected);

		if(!exists) {

			RacksContainer.AddChild(rack);

		}

		rack.Position = new(position.X, 0, -position.Y);
		rack.Rotation = new();
		rack.RotateYDeg(rotation);

		return rack;

	}

	public virtual void UpdateProduct(string productId,
										string rackId,
										string productName,
										Vector3 size,
										Vector2I position) {

		Rack.Rack? rack = GetRack(rackId);

		if(rack == null) {

			GD.PushError("Failed to update a Product: Rack was not found!");

			return;

		}

		rack.UpdateProduct(productId, productName, size, position);

	}

	public virtual Rack.Rack? GetRack(string? rackId = null) =>
		Racks.SingleOrDefault(rack =>
										rack.Id
											.Equals(rackId ?? SelectedRack));

	public virtual void DeselectRack() {

		GetRack(SelectedRack)?.SetSelected(false);

		SelectedRack = null;

	}

	public virtual void RemoveRack(string rackId) {

		GetRack(SelectedRack)?.QueueFree();

		SelectedRack = null;

	}

	public virtual void Zoom(bool forward) {

		if(ObservedRack != null) {

			return;

		}

		TargetCameraZoom += (forward ? -1 : 1) * CameraZoomSpeed;

		TargetCameraZoom = Mathf.Clamp(TargetCameraZoom, CameraZoomSpeed, 100);

	}

	public virtual void ToggleGrabCamera() {

		if(ObservedRack != null) {

			return;

		}

		IsMovingCamera = !IsMovingCamera;

	}

	public virtual void MoveCamera(Vector2 relativePosition) {

		if(ObservedRack != null) {

			return;

		}

		if(!IsMovingCamera) {

			return;

		}

		TargetCameraPosition -=
			new Vector3(relativePosition.X, 0, relativePosition.Y) *
				CameraMoveSpeed *
				Mathf.Sqrt(TargetCameraZoom);

	}

	public virtual void StopObservingRack() {

		ObservedRack = null;

		Camera.Projection = Camera3D.ProjectionType.Perspective;
		Camera.KeepAspect = Camera3D.KeepAspectEnum.Height;

		CameraObservePivot.RemoveChild(Camera);
		CameraDolly.AddChild(Camera);

		CameraObservePivot.RotationDegrees = new();

		foreach(Rack.Rack rack in Racks) {

			rack.SetHidden(false);

		}

	}

}