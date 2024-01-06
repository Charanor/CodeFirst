#version 460 core

in vec2 texCoords;
in vec4 color;

out vec4 FragColor;

uniform sampler2D tex;

void main() {
    FragColor = color * texture(tex, texCoords);
}