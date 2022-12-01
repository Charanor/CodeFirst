#version 460 core

in vec2 texCoords;

out vec4 fragColor;

uniform vec4 backgroundColor;
uniform sampler2D texture0;
uniform bool hasTexture;

void main() {
    if(hasTexture) {
        fragColor = texture(texture0, texCoords);
    } else {
        fragColor = backgroundColor;
    }
}