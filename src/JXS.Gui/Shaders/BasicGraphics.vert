#version 460 core

layout (location = VERTEX_POSITION_IDX) in vec2 vertexPosition;

out vec2 texCoords;

uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

void main() {
    gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(vertexPosition, 0.0, 1.0);
    texCoords = vertexPosition;
}