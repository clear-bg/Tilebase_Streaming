import csv
import os

written_files = set()

def log_merge_time(frame: int, start: float, end: float, endpoint_name: str = "default") -> None:
    duration = (end - start) * 1000  # ミリ秒に変換
    start_ms = start * 1000
    end_ms = end * 1000

    csv_dir = os.path.join(os.path.dirname(__file__), "merge_logs")
    os.makedirs(csv_dir, exist_ok=True)

    file_name = f"merge_time_{endpoint_name}.csv"
    file_path = os.path.join(csv_dir, file_name)

    # 1回目の書き込み時のみ、ファイルを削除（上書き）
    if file_path not in written_files and os.path.exists(file_path):
        os.remove(file_path)
        written_files.add(file_path)

    write_header = not os.path.exists(file_path)

    with open(file_path, "a", newline="") as csvfile:
        writer = csv.writer(csvfile)
        if write_header:
            writer.writerow(["Frame", "MergeStartTime(ms)", "MergeEndTime(ms)", "Elapsed(ms)"])
        writer.writerow([frame, f"{start_ms:.3f}", f"{end_ms:.3f}", f"{duration:.3f}"])
