using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WpfCube;

/// <summary>
/// Builds a unit cube (corners at ±1) as a <see cref="Model3DGroup"/> — one coloured
/// <see cref="GeometryModel3D"/> per face, each with an outward normal and correct winding.
/// </summary>
/// <remarks>
/// The original demo wrote all six faces out by hand in XAML, with mismatched winding so some faces
/// were lit from the inside. Building the mesh in code lets us state each face once, declaratively,
/// and guarantee the two rules WPF 3D cares about:
/// <list type="bullet">
///   <item>vertices wound counter-clockwise as seen from <em>outside</em> the cube, so the face is
///   front-facing and visible;</item>
///   <item>an explicit <see cref="MeshGeometry3D.Normals"/> per vertex, so lighting is flat and even
///   rather than guessed by the renderer.</item>
/// </list>
/// </remarks>
public static class CubeMesh
{
    private readonly record struct Face(Vector3D Normal, Color Colour, Point3D A, Point3D B, Point3D C, Point3D D);

    // Each face: outward normal, colour, then the four corners wound CCW when viewed from outside.
    // (B-A)×(C-A) equals the outward normal for every face — that is what makes them all face out.
    private static readonly Face[] Faces =
    {
        new(new(0, 0, 1),  Colors.IndianRed,      new(-1, -1, 1),  new(1, -1, 1),  new(1, 1, 1),   new(-1, 1, 1)),   // front  +Z
        new(new(0, 0, -1), Colors.ForestGreen,    new(1, -1, -1),  new(-1, -1, -1),new(-1, 1, -1),  new(1, 1, -1)),  // back   -Z
        new(new(-1, 0, 0), Colors.SteelBlue,      new(-1, -1, -1), new(-1, -1, 1), new(-1, 1, 1),   new(-1, 1, -1)), // left   -X
        new(new(1, 0, 0),  Colors.Goldenrod,      new(1, -1, 1),   new(1, -1, -1), new(1, 1, -1),   new(1, 1, 1)),   // right  +X
        new(new(0, 1, 0),  Colors.Moccasin,       new(-1, 1, 1),   new(1, 1, 1),   new(1, 1, -1),   new(-1, 1, -1)), // top    +Y
        new(new(0, -1, 0), Colors.MediumPurple,   new(-1, -1, -1), new(1, -1, -1), new(1, -1, 1),   new(-1, -1, 1)), // bottom -Y
    };

    /// <summary>Creates the cube model, one coloured face at a time.</summary>
    public static Model3DGroup Build()
    {
        var group = new Model3DGroup();
        foreach (var face in Faces)
            group.Children.Add(BuildFace(face));
        return group;
    }

    private static GeometryModel3D BuildFace(Face face)
    {
        var mesh = new MeshGeometry3D();
        foreach (var corner in new[] { face.A, face.B, face.C, face.D })
        {
            mesh.Positions.Add(corner);
            mesh.Normals.Add(face.Normal);
        }

        // Two triangles, A-B-C and A-C-D, sharing the A-C diagonal.
        foreach (var index in new[] { 0, 1, 2, 0, 2, 3 })
            mesh.TriangleIndices.Add(index);

        return new GeometryModel3D
        {
            Geometry = mesh,
            Material = new DiffuseMaterial(new SolidColorBrush(face.Colour)),
        };
    }
}
