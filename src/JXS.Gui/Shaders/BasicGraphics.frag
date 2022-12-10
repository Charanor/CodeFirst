#version 460 core

in vec2 texCoords;

out vec4 fragColor;

uniform vec4 backgroundColor;
uniform sampler2D texture0;
uniform bool hasTexture;

uniform float borderLeft;
uniform float borderRight;
uniform float borderTop;
uniform float borderBottom;
uniform vec4 borderColor;

void main() {
    if (hasTexture) {
        fragColor = texture(texture0, texCoords);
    } else {
        fragColor = backgroundColor;
    }

    if (texCoords.x <= borderLeft
    || texCoords.x >= (1 - borderRight)
    || texCoords.y <= borderBottom
    || texCoords.y >= (1 - borderTop)) {
        fragColor = borderColor;
    }

    //float distanceX = max(0.0, max(borderLeft - texCoords.x, texCoords.x - (1 - borderRight)));
    //float distanceY = max(0.0, max(borderTop - texCoords.y, texCoords.y - (1 - borderBottom)));
    //float distance = sqrt(distanceX * distanceX + distanceY * distanceY);
    //fragColor = mix(fragColor, borderColor, distance);
}