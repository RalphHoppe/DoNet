"""
Generates Assets/Backgrounds/login-background.png — the teal nebula starfield
used behind the DoNet sign-in screen.

Deterministic (fixed RNG seed), so re-running reproduces the exact same image.

Usage:  python tools/generate_background.py
Requires: pillow
"""

import math
import os
import random

from PIL import Image, ImageDraw, ImageFilter

WIDTH, HEIGHT = 2560, 1600
SEED = 20260101

OUT = os.path.join(
    os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
    "DoNet", "Assets", "Backgrounds", "login-background.png",
)

# Deep near-black base the whole scene sits on.
BASE = (6, 16, 15)

# Radial light sources: (centre x%, centre y%, radius as % of diagonal, rgb, peak strength)
GLOWS = [
    (-0.04, -0.08, 0.56, (40, 132, 117), 1.00),  # main teal bloom, top-left
    (0.00, 0.34, 0.34, (26, 98, 87), 0.46),      # secondary lobe down the left edge
    (0.01, 0.90, 0.36, (18, 74, 67), 0.38),      # cool spill, bottom-left
    (0.46, 0.00, 0.34, (12, 48, 44), 0.22),      # faint haze across the top
    (0.30, 1.04, 0.34, (10, 40, 37), 0.16),      # barely-there lift along the bottom
]

# Higher = tighter, faster falloff around each glow centre.
FALLOFF = 2.3


def build_gradient() -> Image.Image:
    """Paint the nebula at 1/8 scale then upsample — fast, and perfectly smooth."""
    sw, sh = WIDTH // 8, HEIGHT // 8
    img = Image.new("RGB", (sw, sh), BASE)
    px = img.load()
    diag = math.hypot(sw, sh)

    for y in range(sh):
        for x in range(sw):
            r, g, b = BASE
            for cx, cy, rad, (gr, gg, gb), strength in GLOWS:
                dx = x - cx * sw
                dy = y - cy * sh
                d = math.hypot(dx, dy) / (rad * diag)
                if d >= 1.0:
                    continue
                # smooth falloff, weighted toward the centre
                f = (1.0 - d) ** FALLOFF * strength
                r += gr * f
                g += gg * f
                b += gb * f
            px[x, y] = (min(int(r), 255), min(int(g), 255), min(int(b), 255))

    return img.resize((WIDTH, HEIGHT), Image.LANCZOS).filter(
        ImageFilter.GaussianBlur(radius=6)
    )


def scatter_stars(img: Image.Image) -> None:
    """Dust the sky with soft, unevenly-sized stars."""
    rng = random.Random(SEED)
    stars = Image.new("RGBA", img.size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(stars)

    for _ in range(2600):
        x = rng.uniform(0, WIDTH)
        y = rng.uniform(0, HEIGHT)

        # Stars read more strongly against the darker right/bottom of the frame.
        darkness = 0.35 + 0.65 * (x / WIDTH) * 0.7 + 0.3 * (y / HEIGHT)
        darkness = min(darkness, 1.0)

        roll = rng.random()
        if roll > 0.985:          # rare bright star
            radius = rng.uniform(1.9, 2.8)
            alpha = rng.uniform(150, 215)
        elif roll > 0.88:         # occasional mid star
            radius = rng.uniform(1.2, 1.9)
            alpha = rng.uniform(80, 140)
        else:                     # the fine background dust
            radius = rng.uniform(0.6, 1.2)
            alpha = rng.uniform(26, 78)

        alpha = int(alpha * darkness)
        if alpha <= 3:
            continue

        # Faintly cool-tinted white so the stars sit in the teal palette.
        tint = rng.random()
        colour = (
            int(236 + 19 * tint),
            int(250 + 5 * (1 - tint)),
            250,
            alpha,
        )
        draw.ellipse([x - radius, y - radius, x + radius, y + radius], fill=colour)

    stars = stars.filter(ImageFilter.GaussianBlur(radius=0.55))
    img.paste(stars, (0, 0), stars)


def add_vignette(img: Image.Image) -> Image.Image:
    """Pull the corners down so the card floats in the centre of the frame."""
    sw, sh = WIDTH // 8, HEIGHT // 8
    mask = Image.new("L", (sw, sh), 0)
    px = mask.load()
    cx, cy = sw / 2, sh / 2
    maxd = math.hypot(cx, cy)

    for y in range(sh):
        for x in range(sw):
            d = math.hypot(x - cx, y - cy) / maxd
            px[x, y] = int(max(0.0, (d - 0.42) / 0.58) ** 1.8 * 150)

    mask = mask.resize((WIDTH, HEIGHT), Image.LANCZOS).filter(
        ImageFilter.GaussianBlur(radius=40)
    )
    shade = Image.new("RGB", img.size, (2, 7, 7))
    return Image.composite(shade, img, mask)


def main() -> None:
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    img = build_gradient()
    img = add_vignette(img)
    scatter_stars(img)
    img.save(OUT, "PNG", optimize=True)
    print(f"wrote {OUT}  ({os.path.getsize(OUT) / 1024:.0f} KB)")


if __name__ == "__main__":
    main()
