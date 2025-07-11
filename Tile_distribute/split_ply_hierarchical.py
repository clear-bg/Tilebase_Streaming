import os
import numpy as np
import plyfile
import csv
import time

input_dir = "Original_ply_20"
output_base_dir = "split_hierarchical_20_to_2_3_2"
csv_path = "output.csv"

x_splits = 2
y_splits = 3
z_splits = 2

# CSV初期化
if os.path.exists(csv_path):
    os.remove(csv_path)

with open(csv_path, mode='w', newline='') as f:
    writer = csv.writer(f)
    writer.writerow(["Frame", "Start(ms)", "End(ms)", "Elapsed(ms)"])

for file_num in range(300):
    file_name = f"{file_num:03d}.ply"
    input_path = os.path.join(input_dir, file_name)

    if not os.path.exists(input_path):
        print(f"存在しません: {input_path}")
        continue

    start_time = time.time()

    number_str = f"{file_num:03d}"
    output_dir = os.path.join(output_base_dir, number_str)
    os.makedirs(output_dir, exist_ok=True)

    plydata = plyfile.PlyData.read(input_path)
    vertex = plydata['vertex']
    x, y, z = vertex['x'], vertex['y'], vertex['z']

    y_bounds = np.linspace(y.min(), y.max(), y_splits + 1)

    for yi in range(y_splits):
        # yの層に属する点群を抽出
        y_mask = (y >= y_bounds[yi]) & (y < y_bounds[yi + 1])
        if yi == y_splits - 1:
            y_mask |= (y == y_bounds[-1])
        sub_vertex = vertex[y_mask]

        if len(sub_vertex) == 0:
            # 空の領域：全てのxzタイルを空で出力
            for xi in range(x_splits):
                for zi in range(z_splits):
                    out_file = f"{number_str}_tile_{xi}_{yi}_{zi}.ply"
                    out_path = os.path.join(output_dir, out_file)
                    empty_dtype = np.dtype([('x', 'f4'), ('y', 'f4'), ('z', 'f4')])
                    empty_array = np.array([], dtype=empty_dtype)
                    empty_element = plyfile.PlyElement.describe(empty_array, 'vertex')
                    plyfile.PlyData([empty_element], text=False).write(out_path)
            continue

        x_sub = sub_vertex['x']
        z_sub = sub_vertex['z']
        x_median = np.median(x_sub)
        z_median = np.median(z_sub)

        for xi in range(x_splits):
            for zi in range(z_splits):
                x_condition = (x_sub < x_median) if xi == 0 else (x_sub >= x_median)
                z_condition = (z_sub < z_median) if zi == 0 else (z_sub >= z_median)
                mask = x_condition & z_condition

                tile_vertices = sub_vertex[mask]
                out_file_name = f"{number_str}_tile_{xi}_{yi}_{zi}.ply"
                out_path = os.path.join(output_dir, out_file_name)

                if len(tile_vertices) == 0:
                    empty_dtype = np.dtype([('x', 'f4'), ('y', 'f4'), ('z', 'f4')])
                    empty_array = np.array([], dtype=empty_dtype)
                    empty_element = plyfile.PlyElement.describe(empty_array, 'vertex')
                    plyfile.PlyData([empty_element], text=False).write(out_path)
                else:
                    new_ply = plyfile.PlyData(
                        [plyfile.PlyElement.describe(tile_vertices, 'vertex')],
                        text=False
                    )
                    new_ply.write(out_path)

                print(f"出力: {out_path}")

    end_time = time.time()
    elapsed_ms = (end_time - start_time) * 1000

    with open(csv_path, mode='a', newline='') as f:
        writer = csv.writer(f)
        writer.writerow([
            number_str,
            f"{start_time * 1000:.3f}",
            f"{end_time * 1000:.3f}",
            f"{elapsed_ms:.3f}"
        ])

print("全ファイル分割完了")
