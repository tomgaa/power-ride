using Godot;
using System;

public partial class GuyOnBicycleCharacter : CharacterBody3D
{
	public const float Speed = 20.0f;
	public const float JumpVelocity = 4.5f;
	public const float RotationSpeed = 5.0f; // Speed of rotation interpolation
	public const float MinVelocityForRotation = 0.1f; // Minimum velocity to trigger rotation
	public const float BankingAmount = 30.0f; // Maximum banking angle in degrees when turning
	public const float BankingSpeed = 8.0f; // Speed of banking interpolation

	// Camera settings
	public const float CameraDistance = 8.0f; // Distance behind character
	public const float CameraHeight = 4.0f; // Height above character
	public const float CameraLookAhead = 2.0f; // How far ahead camera looks

	private AnimationPlayer _animationPlayer;
	private Node3D _cameraPivot;
	private Camera3D _camera;
	private Node3D _modelNode;

	public override void _Ready()
	{
		GD.Print("GuyOnBicycleCharacter: _Ready() called");
		
		// Find model node for banking
		_modelNode = GetNodeOrNull<Node3D>("Model");
		
		// Find camera components
		_cameraPivot = GetNodeOrNull<Node3D>("CameraPivot");
		if (_cameraPivot != null)
		{
			_camera = _cameraPivot.GetNodeOrNull<Camera3D>("Camera3D");
			if (_camera != null)
			{
				GD.Print("GuyOnBicycleCharacter: Camera found");
				// Set initial camera position (behind and above character)
				UpdateCameraPosition();
			}
		}
		
		// Find AnimationPlayer in the scene tree
		_animationPlayer = FindAnimationPlayer(this);
		
		if (_animationPlayer != null)
		{
			GD.Print("GuyOnBicycleCharacter: AnimationPlayer found");
			// Play ArmatureAction animation in loop mode
			if (_animationPlayer.HasAnimation("ArmatureAction"))
			{
				GD.Print("GuyOnBicycleCharacter: ArmatureAction animation found");
				try
				{
					// Get the animation resource and set loop mode
					Animation animation = _animationPlayer.GetAnimation("ArmatureAction");
					if (animation != null)
					{
						// Try to set loop mode: 0 = None, 1 = Linear, 2 = Loop
						// Use integer cast as fallback if enum values differ
						animation.LoopMode = (Animation.LoopModeEnum)2;
					}
					_animationPlayer.Play("ArmatureAction");
					GD.Print("GuyOnBicycleCharacter: Animation started");
				}
				catch (System.Exception e)
				{
					GD.PrintErr($"GuyOnBicycleCharacter: Error setting up animation: {e.Message}");
					// Try to play anyway without setting loop mode
					try
					{
						_animationPlayer.Play("ArmatureAction");
						GD.Print("GuyOnBicycleCharacter: Animation started (fallback)");
					}
					catch (System.Exception e2)
					{
						GD.PrintErr($"GuyOnBicycleCharacter: Error playing animation: {e2.Message}");
					}
				}
			}
			else
			{
				GD.PrintErr("GuyOnBicycleCharacter: Animation 'ArmatureAction' not found in AnimationPlayer");
			}
		}
		else
		{
			GD.PrintErr("GuyOnBicycleCharacter: AnimationPlayer not found in scene. Character will work without animation.");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Always apply gravity (fixes the issue where character doesn't fall)
		Vector3 gravity = GetGravity();
		velocity += gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_right", "ui_left", "ui_down", "ui_up");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();

		// Rotate character based on velocity direction
		RotateTowardsVelocity((float)delta);
		
		// Apply banking (tilting) when turning
		ApplyBanking((float)delta);
		
		// Update camera position to follow character rotation
		UpdateCameraPosition();
	}

	private void RotateTowardsVelocity(float delta)
	{
		// Get horizontal velocity (ignore Y component)
		Vector3 horizontalVelocity = new Vector3(Velocity.X, 0, Velocity.Z);
		
		// Only rotate if there's significant horizontal movement
		if (horizontalVelocity.Length() > MinVelocityForRotation)
		{
			// Calculate target rotation based on velocity direction
			float targetAngle = Mathf.Atan2(horizontalVelocity.X, horizontalVelocity.Z);
			targetAngle = Mathf.RadToDeg(targetAngle);
			
			// Smoothly interpolate rotation
			float currentAngle = RotationDegrees.Y;
			float newAngle = Mathf.LerpAngle(
				Mathf.DegToRad(currentAngle),
				Mathf.DegToRad(targetAngle),
				RotationSpeed * delta
			);
			
			// Apply rotation only around Y axis
			RotationDegrees = new Vector3(RotationDegrees.X, Mathf.RadToDeg(newAngle), RotationDegrees.Z);
		}
	}

	private void ApplyBanking(float delta)
	{
		if (_modelNode == null)
			return;
		
		// Get input direction to determine turning
		Vector2 inputDir = Input.GetVector("ui_right", "ui_left", "ui_down", "ui_up");
		
		// Calculate turning amount (left/right input)
		float turnAmount = inputDir.X;
		
		// Calculate target banking angle (negative for left turn, positive for right turn)
		float targetBankAngle = -turnAmount * BankingAmount;
		
		// Get current bank angle
		float currentBankAngle = _modelNode.RotationDegrees.Z;
		
		// Smoothly interpolate banking
		float newBankAngle = Mathf.Lerp(
			currentBankAngle,
			targetBankAngle,
			BankingSpeed * delta
		);
		
		// Apply banking rotation around Z axis (roll)
		_modelNode.RotationDegrees = new Vector3(
			_modelNode.RotationDegrees.X,
			_modelNode.RotationDegrees.Y,
			newBankAngle
		);
	}

	private void UpdateCameraPosition()
	{
		if (_cameraPivot == null || _camera == null)
			return;
		
		// Get character's forward direction (based on rotation)
		float yaw = Mathf.DegToRad(RotationDegrees.Y);
		Vector3 forward = new Vector3(Mathf.Sin(yaw), 0, Mathf.Cos(yaw));
		
		// Position camera behind and above character
		Vector3 cameraOffset = -forward * CameraDistance + Vector3.Up * CameraHeight;
		_cameraPivot.GlobalPosition = GlobalPosition + cameraOffset;
		
		// Make camera look at character (with look-ahead for smoother feel)
		Vector3 lookTarget = GlobalPosition + forward * CameraLookAhead;
		_camera.LookAt(lookTarget, Vector3.Up);
	}

	private AnimationPlayer FindAnimationPlayer(Node node)
	{
		// Recursively search for AnimationPlayer in the scene tree
		if (node is AnimationPlayer animationPlayer)
		{
			return animationPlayer;
		}

		foreach (Node child in node.GetChildren())
		{
			AnimationPlayer found = FindAnimationPlayer(child);
			if (found != null)
			{
				return found;
			}
		}

		return null;
	}
}
