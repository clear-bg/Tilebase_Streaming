import csv
import os

def log_merge_time(frame: int, start: float, end: float, endpoint_name: str = "default") -> None:
    duration = end - start
    csv_dir = os.path.join(os.path.dirname(__file__), "merge_logs")
    os.makedirs(csv_dir, exist_ok=True)

    # 出力ファイル名：例 → merge_time_get_file.csv
    file_name = f"merge_time_{endpoint_name}.csv"
    file_path = os.path.join(csv_dir, file_name)

    with open(file_path, "a", newline="") as csvfile:
        writer = csv.writer(csvfile)
        writer.writerow([frame, start, end, duration])
