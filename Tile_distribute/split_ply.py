import os
import numpy as np
import plyfile

# 入力ディレクトリと出力ディレクトリ
# input_dir = r"C:\Users\clear\Project\Tile_distribute\Original_Ply"
input_dir = "Original_ply_80"
# output_base_dir = r"C:\Users\clear\Project\Tile_distribute\Split"
output_base_dir= "split_80"

# 分割数
x_splits, y_splits, z_splits = 2, 3, 2

for file_num in range(300):
    file_name = f"{file_num:03d}.ply"
    input_path = os.path.join(input_dir, file_name)

    if not os.path.exists(input_path):
        print(f"存在しません: {input_path}")
        continue

    # 出力ディレクトリ（Split/number/）
    number_str = f"{file_num:03d}"
    output_dir = os.path.join(output_base_dir, number_str)
    os.makedirs(output_dir, exist_ok=True)

    # PLY読み込み
    plydata = plyfile.PlyData.read(input_path)
    vertex = plydata['vertex']
    x = vertex['x']
    y = vertex['y']
    z = vertex['z']

    x_min, x_max = x.min(), x.max()
    y_min, y_max = y.min(), y.max()
    z_min, z_max = z.min(), z.max()

    x_bounds = np.linspace(x_min, x_max, x_splits + 1)
    y_bounds = np.linspace(y_min, y_max, y_splits + 1)
    z_bounds = np.linspace(z_min, z_max, z_splits + 1)

    for xi in range(x_splits):
        for yi in range(y_splits):
            for zi in range(z_splits):
                mask = (
                    (x >= x_bounds[xi]) & (x < x_bounds[xi + 1]) &
                    (y >= y_bounds[yi]) & (y < y_bounds[yi + 1]) &
                    (z >= z_bounds[zi]) & (z < z_bounds[zi + 1])
                )
                if xi == x_splits - 1:
                    mask |= (x == x_max)
                if yi == y_splits - 1:
                    mask |= (y == y_max)
                if zi == z_splits - 1:
                    mask |= (z == z_max)

                sub_vertices = vertex[mask]
                if len(sub_vertices) == 0:
                    continue

                out_file_name = f"{number_str}_tile_{xi}_{yi}_{zi}.ply"
                out_path = os.path.join(output_dir, out_file_name)

                new_ply = plyfile.PlyData([plyfile.PlyElement.describe(sub_vertices, 'vertex')], text=False)
                new_ply.write(out_path)
                print(f"出力: {out_path}")

print("全ファイル分割完了")
