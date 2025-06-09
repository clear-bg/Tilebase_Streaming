import os
import time
import plyfile
import numpy as np

start_time = time.time()

tile_files = [
    "get_file/000/000_tile_0_1_0.ply",
    "get_file/000/000_tile_0_1_1.ply",
    "get_file/000/000_tile_0_2_0.ply",
    "get_file/000/000_tile_0_2_1.ply",
    "get_file/000/000_tile_1_1_0.ply",
    "get_file/000/000_tile_1_1_1.ply",
    "get_file/000/000_tile_1_2_0.ply",
    "get_file/000/000_tile_1_2_1.ply"
]

all_vertices = []

for file_path in tile_files:
    if not os.path.exists(file_path):
        print(f"スキップ: {file_path}（存在しない）")
        continue

    plydata = plyfile.PlyData.read(file_path)
    vertex = plydata['vertex']
    all_vertices.append(vertex.data)

merged_data = np.concatenate(all_vertices)

output_path = "merged_output.ply"
merged_element = plyfile.PlyElement.describe(merged_data, 'vertex')
merged_ply = plyfile.PlyData([merged_element], text=False)
merged_ply.write(output_path)

end_time = time.time()
elapsed_time = end_time - start_time

print(f"統合完了: {output_path}")
print(f"処理時間: {elapsed_time * 1000:.2f} ms")