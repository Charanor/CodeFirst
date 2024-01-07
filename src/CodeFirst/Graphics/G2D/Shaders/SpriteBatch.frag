#version 460 core

in vec2 texCoords;
in vec4 color;

out vec4 FragColor;

uniform sampler2D texture0;

void main() {
    FragColor = color * texture(texture0, texCoords);
}