#version 460 core

layout (location = VERTEX_POSITION_IDX) in vec2 vertexPosition;
layout (location = VERTEX_TEX_COORD_IDX) in vec2 vertexTexCoord;

out vec2 texCoords;

void main() {
    gl_Position = vec4(vertexPosition, 0.0, 1.0);
    texCoords = vertexTexCoord;
}