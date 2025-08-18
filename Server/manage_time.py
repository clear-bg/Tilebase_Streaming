import csv
import os
from config import CSV_LOG_ENABLED
import os, csv, time

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

def log_merge_time(frame: int, start: float, end: float, endpoint_name: str = "default"):
    if not CSV_LOG_ENABLED:
        return

def log_merge_time_for_request(frame: int, start: float, end: float, endpoint_name: str = "merge_only"):
    """merge処理専用のCSVロガー（CSV_LOG_ENABLEDには依存しない）"""
    csv_dir = os.path.join(os.path.dirname(__file__), "merge_logs")
    os.makedirs(csv_dir, exist_ok=True)

    out_path = os.path.join(csv_dir, f"merge_time_{endpoint_name}.csv")
    write_header = not os.path.exists(out_path)

    with open(out_path, "a", newline="", encoding="utf-8") as f:
        w = csv.writer(f)
        if write_header:
            w.writerow(["frame", "start", "end", "elapsed_ms"])
        w.writerow([frame, start, end, (end - start) * 1000.0])
