'''
requests_20250822_153908
'''

import os
import csv
import time
from Server.merge_ply import merge_ply_files

# ===== 設定 =====
BASE_DIR = os.path.dirname(os.path.abspath(__file__))   # Server/test_scripts
INPUT_CSV = os.path.join(BASE_DIR, "requests_20250822_153908.csv") # Unityで出力したリクエストログ
OUTPUT_CSV = os.path.join(BASE_DIR, "merge_results.csv") # 実験結果の保存先

# datasetのルートパスを適宜修正
DATASET_ROOT = os.path.join(os.path.dirname(BASE_DIR), "get_file", "split_20_to_2_3_2")

# ===== メイン処理 =====
# 設定
csv_path = os.path.join(os.path.dirname(__file__), "requests_20250822_153908.csv")
output_csv_path = os.path.join(os.path.dirname(__file__), "merge_time_log.csv")
tile_root = os.path.join(os.path.dirname(__file__), "../get_file/split_20_to_2_3_2")  # 適宜調整
grid_size = (2, 3, 2)

def run_merge_from_csv():
    with open(csv_path, newline='', encoding='utf-8-sig') as csvfile:
        reader = csv.DictReader(csvfile)

        with open(output_csv_path, mode='w', newline='', encoding='utf-8-sig') as outfile:
            writer = csv.writer(outfile)
            writer.writerow(["Frame", "TileCount", "Duration (s)"])

            for row in reader:
                frame = int(row["Frame"])
                tile_ids_str = row["Tiles"]
                tile_ids = [int(t.strip()) for t in tile_ids_str.split(",") if t.strip().isdigit()]

                if not tile_ids:
                    print(f"フレーム {frame} は対象タイルなし -> スキップ")
                    continue

                # 対象PLYファイルのパスを組み立て

                ply_files = []
                for tile_id in tile_ids:
                    x, y, z = tile_id_to_xyz(tile_id, grid_size)
                    filename = f"tile_{x}_{y}_{z}.ply"  # ✅ frame番号なし！
                    path = os.path.join(tile_root, f"{frame:03d}", filename)
                    ply_files.append(path)

                # 結合して時間を測定
                print(f"フレーム {frame} : {len(ply_files)} タイルを結合中...")
                start = time.time()
                _ = merge_ply_files(ply_files, frame)  # 戻り値は保存しない
                end = time.time()
                duration = round(end - start, 4)

                writer.writerow([frame, len(tile_ids), duration])

def tile_id_to_xyz(tile_id, grid_size):
    gx, gy, gz = grid_size
    z = tile_id % gz
    y = (tile_id // gz) % gy
    x = tile_id // (gy * gz)
    return x, y, z

if __name__ == "__main__":
    run_merge_from_csv()
