import os
import numpy as np
import plyfile
import csv
import time

input_dir = "Original_ply_20"
output_base_dir = "split_20_to_2_3_2"
csv_path = "output.csv"

x_splits, y_splits, z_splits = 2, 3, 2

# 既存CSV削除
if os.path.exists(csv_path):
    os.remove(csv_path)

# ヘッダを書き込み
with open(csv_path, mode='w', newline='') as f:
    writer = csv.writer(f)
    writer.writerow(["Frame", "Start(ms)", "End(ms)", "Elapsed(ms)"])

for file_num in range(300):
    file_name = f"{file_num:03d}.ply"
    input_path = os.path.join(input_dir, file_name)

    if not os.path.exists(input_path):
        print(f"存在しません: {input_path}")
        continue

    # タイマー開始
    start_time = time.time()

    number_str = f"{file_num:03d}"
    output_dir = os.path.join(output_base_dir, number_str)
    os.makedirs(output_dir, exist_ok=True)

    plydata = plyfile.PlyData.read(input_path)
    vertex = plydata['vertex']
    x, y, z = vertex['x'], vertex['y'], vertex['z']

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
                out_file_name = f"{number_str}_tile_{xi}_{yi}_{zi}.ply"
                out_path = os.path.join(output_dir, out_file_name)

                if len(sub_vertices) == 0:
                    empty_dtype = np.dtype([('x', 'f4'), ('y', 'f4'), ('z', 'f4')])
                    empty_array = np.array([], dtype=empty_dtype)
                    empty_element = plyfile.PlyElement.describe(empty_array, 'vertex')
                    plyfile.PlyData([empty_element], text=False).write(out_path)
                else:
                    new_ply = plyfile.PlyData(
                        [plyfile.PlyElement.describe(sub_vertices, 'vertex')],
                        text=False
                    )
                    new_ply.write(out_path)

                print(f"出力: {out_path}")

    # タイマー終了
    end_time = time.time()
    elapsed_ms = (end_time - start_time) * 1000

    # CSVに追記
    with open(csv_path, mode='a', newline='') as f:
        writer = csv.writer(f)
        writer.writerow([
            number_str,
            f"{start_time * 1000:.3f}",
            f"{end_time * 1000:.3f}",
            f"{elapsed_ms:.3f}"
        ])

print("全ファイル分割完了")
