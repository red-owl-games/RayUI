using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Raylib.UI;

public static partial class RUI
{
    private const string shaderVS = @"
#version 330

in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec4 vertexColor;

out vec2 fragTexCoord;
out vec4 fragColor;

uniform mat4 mvp;

void main()
{
    fragTexCoord = vertexTexCoord;
    fragColor = vertexColor;
    gl_Position = mvp * vec4(vertexPosition, 1.0);
}
                                    ";

    private const string shaderFS = @"
#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform int hasTexture;
uniform vec2 size;
uniform float radius;

out vec4 finalColor;

void main()
{
    vec2 pixelPos = fragTexCoord * size;
    vec2 dist = min(pixelPos, size - pixelPos);

    // Rounded corner mask
    if (dist.x < radius && dist.y < radius)
    {
        vec2 cornerVec = dist - vec2(radius);
        if (dot(cornerVec, cornerVec) > radius * radius)
            discard;
    }

    finalColor = texture(texture0, fragTexCoord) * fragColor;
}
";
    //

    internal static Texture2D white;
    internal static Rectangle whiteRect;
    internal static Shader shader;
    private static int _locSize;
    private static int _locRadius;

    internal static void LoadShader()
    {
        var img = GenImageColor(1, 1, Color.White);
        white = LoadTextureFromImage(img);
        UnloadImage(img);
        whiteRect = new Rectangle(0, 0, white.Width, white.Height);
        
        shader = LoadShaderFromMemory(shaderVS, shaderFS);
        _locSize      = GetShaderLocation(shader, "size");
        _locRadius    = GetShaderLocation(shader, "radius");
    }
    
    internal static void SetShaderSize(Vector2 size) =>
        SetShaderValue(shader, _locSize, new[] {
            size.X, size.Y
        }, ShaderUniformDataType.Vec2);
    
    internal static void SetShaderRadius(float radius) =>
        SetShaderValue(shader, _locRadius, new float[] {
            radius
        }, ShaderUniformDataType.Float);
}