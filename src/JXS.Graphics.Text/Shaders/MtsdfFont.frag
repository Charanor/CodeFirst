#version 460 core

in vec2 texCoords;

out vec4 fragmentColor;

uniform sampler2D fontAtlas;

uniform float distanceFieldRange;

uniform vec4 backgroundColor;
uniform vec4 foregroundColor;

float median(vec3 input);
float calculateScreenPixelRange();

void main() {
    vec4 mtsd = texture(fontAtlas, texCoords);
    float signedDistance = median(mtsd.rgb);
    float softDistance = mtsd.a;

    float screenDistance = calculateScreenPixelRange() * (signedDistance - 0.5);
    float opacity = clamp(screenDistance + 0.5, 0.0, 1.0);

    fragmentColor = mix(backgroundColor, foregroundColor, opacity);
}

float median(vec3 input) {
    return max(min(input.r, input.g), min(max(input.r, input.g), input.b));
}

float calculateScreenPixelRange() {
    vec2 unitRange = vec2(distanceFieldRange) / vec2(textureSize(fontAtlas, 0));
    vec2 screenTexSize = vec2(1.0) / fwidth(texCoords);
    return max(0.5 * dot(unitRange, screenTexSize), 1.0);
}