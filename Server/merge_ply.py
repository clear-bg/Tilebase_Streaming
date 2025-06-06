import os
import glob
import plyfile
import numpy as np

# 統合対象のファイルリスト（重複を除く）
tile_files = [
    ".get_file/000_tile_0_1_0.ply",
    ".get_file/000_tile_0_1_1.ply",
    ".get_file/000_tile_0_2_0.ply",
    ".get_file/000_tile_0_2_1.ply",
    ".get_file/000_tile_1_1_0.ply",
    ".get_file/000_tile_1_1_0.ply",  # 重複あり
    ".get_file/000_tile_1_2_0.ply",
    ".get_file/000_tile_1_2_0.ply"   # 重複あり
]

# 重複を除外
tile_files = list(dict.fromkeys(tile_files))

# マージ用リスト
all_vertices = []

for file_path in tile_files:
    if not os.path.exists(file_path):
        print(f"スキップ: {file_path}（存在しない）")
        continue
    
    plydata = plyfile.PlyData.read(file_path)
    vertex = plydata['vertex']
    all_vertices.append(vertex.data)

# 結合
merged_data = np.concatenate(all_vertices)

# 出力ファイル
output_path = "merged_output.ply"
merged_element = plyfile.PlyElement.describe(merged_data, 'vertex')
merged_ply = plyfile.PlyData([merged_element], text=False)
merged_ply.write(output_path)

print(f"統合完了: {output_path}")
