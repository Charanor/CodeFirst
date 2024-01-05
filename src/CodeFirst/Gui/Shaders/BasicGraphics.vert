#version 460 core

layout (location = VERTEX_POSITION_IDX) in vec2 vertexPosition;

out vec2 texCoords;

uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

uniform bool flipX;
uniform bool flipY;
uniform bool flipAxis;

void main() {
    gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(vertexPosition, 0.0, 1.0);
    float texCoordX = flipX ? 1 - vertexPosition.x : vertexPosition.x;
    float texCoordY = flipY ? 1 - vertexPosition.y : vertexPosition.y;
    texCoords = vec2(
        flipAxis ? texCoordY : texCoordX,
        flipAxis ? texCoordX : texCoordY    
    );
}