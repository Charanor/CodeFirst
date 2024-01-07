#version 460 core

// Inputs
layout (location = VERTEX_POSITION_IDX) in vec3 vertexPosition;
layout (location = VERTEX_TEX_COORDS_IDX) in vec2 vertexTexCoords;
layout (location = VERTEX_COLOR_IDX) in vec4 vertexColor;

// Outputs
out vec2 texCoords;
out vec4 color;

// Matrices
uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

void main() {
    gl_Position = projectionMatrix * viewMatrix * vec4(vertexPosition, 1.0);
    texCoords = vertexTexCoords;
}