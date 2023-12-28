#version 460 core

const float BORDER_ANTI_ALIASING = 1.0;

in vec2 texCoords;

out vec4 fragColor;

uniform vec4 backgroundColor;
uniform sampler2D texture0;
uniform bool hasTexture;

uniform float borderTopLeftRadius;
uniform float borderTopRightRadius;
uniform float borderBottomLeftRadius;
uniform float borderBottomRightRadius;

uniform vec2 size;

vec4 radii = vec4(borderTopRightRadius, borderBottomRightRadius, borderTopLeftRadius, borderBottomLeftRadius);

float signedDistance(vec2 inPosition, vec2 inSize) {
    // This 'magic' here is picking the correct corner radius
    vec2 leftRight = (inPosition.x > 0.0) ? radii.xy : radii.zw;
    float radius = (inPosition.y > 0.0) ? leftRight.x : leftRight.y;
    vec2 q = abs(inPosition) - (inSize / 2.0) + radius;
    return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - radius;
}

void main() {
    vec2 position = size * texCoords;
    vec4 color = hasTexture ? texture(texture0, texCoords) : backgroundColor;
    float dist = signedDistance(position - size / 2.0, size);
    fragColor = (dist < 0.0) ? color : vec4(0.0);
}
