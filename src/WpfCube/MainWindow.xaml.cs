using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace WpfCube;

/// <summary>
/// Hosts the cube and drives it: builds the mesh, spins it on a timer, and lets the user rotate it
/// by dragging and zoom with the wheel.
/// </summary>
public partial class MainWindow : Window
{
    private const double DegreesPerPixel = 0.4;   // drag sensitivity
    private const double AutoSpinDegrees = 0.6;    // yaw added each auto-spin tick

    private readonly DispatcherTimer _spinTimer;
    private bool _autoSpin = true;
    private bool _dragging;
    private Point _lastPosition;

    public MainWindow()
    {
        InitializeComponent();

        // The geometry is built in code so each face is stated once, with a correct outward normal.
        CubeVisual.Content = CubeMesh.Build();

        // Start at a three-quarter view so three faces are visible straight away.
        Rotation.Quaternion = new Quaternion(new Vector3D(1, 0, 0), -22) *
                              new Quaternion(new Vector3D(0, 1, 0), 35);

        _spinTimer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(16) };
        _spinTimer.Tick += (_, _) => Spin(new Vector3D(0, 1, 0), AutoSpinDegrees);
        _spinTimer.Start();
    }

    // --- rotation -----------------------------------------------------------------------------

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        _dragging = true;
        _lastPosition = e.GetPosition(this);
        CaptureMouse();
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        _dragging = false;
        ReleaseMouseCapture();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_dragging)
            return;

        var position = e.GetPosition(this);
        var dx = position.X - _lastPosition.X;
        var dy = position.Y - _lastPosition.Y;
        _lastPosition = position;

        // Horizontal drag yaws about world up; vertical drag pitches about world right.
        Spin(new Vector3D(0, 1, 0), dx * DegreesPerPixel);
        Spin(new Vector3D(1, 0, 0), dy * DegreesPerPixel);
    }

    /// <summary>
    /// Composes a delta rotation onto the cube's current orientation. Pre-multiplying (delta first)
    /// applies the turn in the viewer's frame, which is what makes dragging feel like a trackball.
    /// </summary>
    private void Spin(Vector3D axis, double degrees)
    {
        var delta = new Quaternion(axis, degrees);
        Rotation.Quaternion = delta * Rotation.Quaternion;
    }

    // --- zoom ---------------------------------------------------------------------------------

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var direction = Camera.LookDirection;
        direction.Normalize();

        var moved = Camera.Position + direction * (e.Delta / 120.0) * 0.6;

        // Keep the camera a sensible distance from the cube at the origin.
        var distance = new Vector3D(moved.X, moved.Y, moved.Z).Length;
        if (distance is >= 3 and <= 12)
            Camera.Position = moved;
    }

    // --- keyboard -----------------------------------------------------------------------------

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Space)
            return;

        _autoSpin = !_autoSpin;
        if (_autoSpin)
            _spinTimer.Start();
        else
            _spinTimer.Stop();
    }
}
