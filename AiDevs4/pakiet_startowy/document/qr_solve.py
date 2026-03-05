"""
QR Code Split Puzzle Solver
Combines two partial QR cork images by trying all orientations and thresholds.
Dependencies: pillow, pyzbar, opencv-python
"""

from PIL import Image, ImageOps
import numpy as np
import cv2
from pyzbar.pyzbar import decode


def load_and_align(path1: str, path2: str):
    """Load both images as grayscale and crop to same size."""
    img1 = Image.open(path1).convert("L")
    img2 = Image.open(path2).convert("L")
    w = min(img1.width, img2.width)
    h = min(img1.height, img2.height)
    return img1.crop((0, 0, w, h)), img2.crop((0, 0, w, h))


def get_transforms(arr: np.ndarray) -> dict:
    """Return all 6 meaningful transforms of a 2D array."""
    return {
        "0":     arr,
        "90":    np.rot90(arr, 1),
        "180":   np.rot90(arr, 2),
        "270":   np.rot90(arr, 3),
        "flipH": np.fliplr(arr),
        "flipV": np.flipud(arr),
    }


def try_decode(combined_binary: np.ndarray, scale: int) -> str | None:
    """Upscale binary array, add quiet zone, attempt decode. Returns string or None."""
    img = Image.fromarray(np.where(combined_binary, 0, 255).astype(np.uint8))
    img = img.resize((img.width * scale, img.height * scale), Image.NEAREST)
    img = ImageOps.expand(img, border=40, fill=255)
    cv_img = np.array(img)

    # pyzbar
    results = decode(cv_img)
    if results:
        return results[0].data.decode()

    # OpenCV fallback
    retval, _, _ = cv2.QRCodeDetector().detectAndDecode(cv_img)
    if retval:
        return retval

    return None


def solve(path1: str, path2: str, output_path: str = "qr_final.png"):
    img1, img2 = load_and_align(path1, path2)
    arr1 = np.array(img1)
    arr2 = np.array(img2)

    transforms = get_transforms(arr2)

    for transform_name, arr2_t in transforms.items():
        if arr2_t.shape != arr1.shape:
            continue

        for threshold in range(70, 140, 5):
            combined = (arr1 < threshold) | (arr2_t < threshold)

            for scale in [2, 3, 4, 5]:
                result = try_decode(combined, scale)
                if result:
                    print(f"[SOLVED] transform={transform_name}, threshold={threshold}, scale={scale}")
                    print(f"[RESULT] {result}")

                    # Save final clean image
                    img_out = Image.fromarray(np.where(combined, 0, 255).astype(np.uint8))
                    img_out = img_out.resize((img_out.width * 4, img_out.height * 4), Image.NEAREST)
                    img_out = ImageOps.expand(img_out, border=60, fill=255)
                    img_out.save(output_path)
                    print(f"[SAVED]  {output_path}")
                    return result

    print("[FAILED] No valid QR code found in any combination.")
    return None


if __name__ == "__main__":
    solve(
        path1="image1.png",
        path2="image2.png",
        output_path="qr_final.png",
    )
