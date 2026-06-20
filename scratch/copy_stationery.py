import shutil
import os

files_to_copy = {
    r"C:\Users\Admin\.gemini\antigravity-ide\brain\5b456f18-6fb0-40c6-9f2a-1b02693102e1\casio_calculator_1781927130887.png": "CasioCalculator.png",
    r"C:\Users\Admin\.gemini\antigravity-ide\brain\5b456f18-6fb0-40c6-9f2a-1b02693102e1\colored_pencils_1781927141875.png": "ColoredPencils.png",
    r"C:\Users\Admin\.gemini\antigravity-ide\brain\5b456f18-6fb0-40c6-9f2a-1b02693102e1\writing_pens_1781927151760.png": "BallpointPen.png",
    r"C:\Users\Admin\.gemini\antigravity-ide\brain\5b456f18-6fb0-40c6-9f2a-1b02693102e1\wooden_ruler_1781927163790.png": "WoodenRuler.png",
    r"C:\Users\Admin\.gemini\antigravity-ide\brain\5b456f18-6fb0-40c6-9f2a-1b02693102e1\spiral_notebook_1781927151760.png": "NotebookSpiral.png" # Let's handle notebook specifically below
}

# Wait, let's list the actual files in directory to make sure we have the correct timestamps
brain_dir = r"C:\Users\Admin\.gemini\antigravity-ide\brain\5b456f18-6fb0-40c6-9f2a-1b02693102e1"
all_files = os.listdir(brain_dir)
png_files = [f for f in all_files if f.endswith('.png')]
print("Found PNG files in brain dir:", png_files)

mapping = {}
for f in png_files:
    if "casio_calculator" in f:
        mapping[f] = "CasioCalculator.png"
    elif "colored_pencils" in f:
        mapping[f] = "ColoredPencils.png"
    elif "writing_pens" in f:
        mapping[f] = "BallpointPen.png"
    elif "wooden_ruler" in f:
        mapping[f] = "WoodenRuler.png"
    elif "spiral_notebook" in f:
        mapping[f] = "NotebookSpiral.png"

src_dir = r"c:\Tuan6\Images"
www_dir = r"c:\Tuan6\wwwroot\images"

os.makedirs(src_dir, exist_ok=True)
os.makedirs(www_dir, exist_ok=True)

for src_name, dest_name in mapping.items():
    src_path = os.path.join(brain_dir, src_name)
    
    dest_src_path = os.path.join(src_dir, dest_name)
    dest_www_path = os.path.join(www_dir, dest_name)
    
    print(f"Copying {src_path} to {dest_src_path} and {dest_www_path}...")
    shutil.copy2(src_path, dest_src_path)
    shutil.copy2(src_path, dest_www_path)

print("Done copying stationery images.")
