https://ag3nts.org/list4

sekretny kod: REBELIA


The logic is a brute-force search across 3 dimensions:

- **6 transforms** of image 2 (rotations + flips)
- **14 threshold values** (70–135, step 5) — controls how aggressively cork texture is filtered out
- **4 upscale factors** (2–5x) — QR decoders notoriously fail on small images; scaling up with `NEAREST` (no blurring) helps

Exits early on first successful decode. The winning combo was `flipV + threshold=70 + scale=2`.