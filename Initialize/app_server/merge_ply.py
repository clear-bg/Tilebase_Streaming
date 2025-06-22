import os
import plyfile
import numpy as np

def merge_ply_files(tile_files, frame_num):
    all_vertices = []

    for file_path in tile_files:
        if not os.path.exists(file_path):
            print(f"スキップ: {file_path}（存在しない）")
            continue

        plydata = plyfile.PlyData.read(file_path)
        vertex = plydata['vertex']
        all_vertices.append(vertex.data)

    if not all_vertices:
        raise FileNotFoundError("No valid tile files found to merge.")

    merged_data = np.concatenate(all_vertices)

    merge_dir = os.path.join(os.path.dirname(__file__), 'merge_ply')
    os.makedirs(merge_dir, exist_ok=True)
    merged_path = os.path.join(merge_dir, f"{frame_num:03d}_merged.ply")

    merged_element = plyfile.PlyElement.describe(merged_data, 'vertex')
    merged_ply = plyfile.PlyData([merged_element], text=False)
    merged_ply.write(merged_path)

    return merged_path