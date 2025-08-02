using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Raylib.UI;

public static partial class RUI
{
    private const string shaderVS = @"
#version 330

// Input vertex attributes
in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec3 vertexNormal;
in vec4 vertexColor;

// Input uniform values
uniform mat4 mvp;

// Output vertex attributes (to fragment shader)
out vec2 fragTexCoord;
out vec4 fragColor;

// NOTE: Add here your custom variables

void main()
{
    // Send vertex attributes to fragment shader
    fragTexCoord = vertexTexCoord;
    fragColor = vertexColor;

    // Calculate final vertex position
    gl_Position = mvp*vec4(vertexPosition, 1.0);
}
";

    private const string shaderFS = @"
#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
uniform vec4 colDiffuse;

// Output fragment color
out vec4 finalColor;

void main() {
    vec4 texelColor = texture(texture0, fragTexCoord);
    finalColor = texelColor*colDiffuse*fragColor;
}
";
    
    internal static Texture2D white;
    internal static Rectangle whiteRect;
    internal static Shader shader;

    internal static void LoadShader()
    {
        var img = GenImageColor(1, 1, Color.White);
        white = LoadTextureFromImage(img);
        UnloadImage(img);
        whiteRect = new Rectangle(0, 0, white.Width, white.Height);
        
        shader = LoadShaderFromMemory(shaderVS, shaderFS);
    }
}