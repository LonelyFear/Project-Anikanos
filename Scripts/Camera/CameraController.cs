using System;
using Godot;

namespace Camera
{
    public partial class CameraController : Node3D
    {
        private Transform3D _thisTransform;

        private Transform3D _cameraTransform;

        [Export]
        public Node3D _cameraNode;
        private float _scrollAmount;

        [Export]
        private float _cameraMoveSpeed = 5.0f;

        [Export]
        private float _cameraRotateSpeed = 0.1f;

        [Export]
        private float _cameraZoomSpeed = 0.1f;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            _thisTransform = Transform3D.Identity;
            _cameraNode = GetNode<Node3D>("Camera");
            _cameraTransform = _cameraNode.Transform;
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            MoveCamera(delta);
            ZoomCamera(delta);
        }

        private void MoveCamera(double delta)
        {
            Vector3 newPositon = _thisTransform.Origin;
            if (Input.IsKeyPressed(Key.S))
            {
                newPositon += _thisTransform.Basis.Z * _cameraMoveSpeed * (float)delta;
                GD.Print("Moving Forward");
            }
            if (Input.IsKeyPressed(Key.W))
            {
                newPositon -= _thisTransform.Basis.Z * _cameraMoveSpeed * (float)delta;
                GD.Print("Moving Backward");
            }
            if (Input.IsKeyPressed(Key.D))
            {
                newPositon += _thisTransform.Basis.X * _cameraMoveSpeed * (float)delta;
                GD.Print("Moving Right");
            }
            if (Input.IsKeyPressed(Key.A))
            {
                newPositon -= _thisTransform.Basis.X * _cameraMoveSpeed * (float)delta;
                GD.Print("Moving Left");
            }
            _thisTransform.Origin = newPositon;
            this.SetTransform(_thisTransform);
        }

        public override void _Input(InputEvent @event)
        {
            GD.Print("Input event received: " + @event);
            if (@event is InputEventMouseButton mouseEvent)
            {
                GD.Print("Mouse button: " + mouseEvent.ButtonIndex);
                if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
                    _scrollAmount -= 1f;
                else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
                    _scrollAmount += 1f;
            }
        }

        private void ZoomCamera(double delta)
        {
            if (_scrollAmount == 0f)
                return;
            Vector3 moveDirection = _cameraTransform.Basis.Z;
            Vector3 zoomDirection = moveDirection * _scrollAmount * _cameraZoomSpeed * (float)delta;
            Vector3 targetPosition = _cameraTransform.Origin + zoomDirection;
            float t = Mathf.Clamp(_cameraZoomSpeed * (float)delta, 0f, 1f);

            Vector3 lerped = new Vector3(
                Mathf.Lerp(_cameraTransform.Origin.X, targetPosition.X, t),
                Mathf.Lerp(_cameraTransform.Origin.Y, targetPosition.Y, t),
                Mathf.Lerp(_cameraTransform.Origin.Z, targetPosition.Z, t)
            );
            _cameraTransform.Origin = lerped;
            _cameraNode.Transform = _cameraTransform;
            _scrollAmount = 0f;
        }
    }
}
