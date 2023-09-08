#version 460 core

layout (location = VERTEX_POSITION_IDX) in vec2 vertexPosition;

uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 modelMatrix;
uniform vec4 uvBounds;

out vec2 texCoords;

float map01(float value, float min2, float max2);

void main() {
    gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(vertexPosition, 0.0, 1.0);
    texCoords = vec2(
        map01(vertexPosition.x, uvBounds.x, uvBounds.z),
        map01(vertexPosition.y, uvBounds.y, uvBounds.w)
    );
}

float map01(float value, float min2, float max2) {
    return min2 + (max2 - min2) * value;
}