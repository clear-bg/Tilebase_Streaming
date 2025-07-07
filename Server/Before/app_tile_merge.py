r'''
サーバ起動は必ず Streming/Server ディレクトリから行う
uvicorn app_tile_merge:app --host 127.0.0.1 --port 8000 --reload

C:\Users\clear\AppData\Roaming\Python\Python310\Scripts\uvicorn.exe Initialize.app:app --host 0.0.0.0 --port 8000 --reload
'''

import os
import time
from fastapi import FastAPI, HTTPException
from fastapi.responses import FileResponse

from merge_ply import merge_ply_files

app = FastAPI()

LOG_PATH = os.path.join(os.path.dirname(__file__), "merge_log.csv")

# 起動時にログを初期化
if os.path.exists(LOG_PATH):
    os.remove(LOG_PATH)

# インデックス→x, y, z変換表（0〜11）
index2xyz = [
    (0, 0, 0), (0, 0, 1), (0, 1, 0), (0, 1, 1),
    (0, 2, 0), (0, 2, 1), (1, 0, 0), (1, 0, 1),
    (1, 1, 0), (1, 1, 1), (1, 2, 0), (1, 2, 1)
]
# x: 左右     [0: 左, 1: 右]
# y: 上下     [0: 下, 1: 真ん中, 2: 上]
# z: 奥,手前  [0: 奥, 1: 手前]


@app.get("/get_file")
async def get_file(frame: int, tiles: str):
    try:
        tile_index = [int(x) for x in tiles.split(",")]
    except Exception:
        raise HTTPException(status_code=400, detail="Invalid tile indices format")

    file_list = get_tile_file_paths(frame, tile_index)

    start = time.time()  # マージ開始時刻

    # merge_ply.pyの関数を呼び出し
    try:
        merged_path = merge_ply_files(file_list, frame)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

    end = time.time()    # マージ終了時刻
    log_merge_time(frame, start, end)

    return FileResponse(
        merged_path,
        media_type="application/octet-stream",
        filename=f"{frame:03d}_merged.ply"
    )


@app.get("/merge_ply")
async def merge_ply(frame: int):
    # merge_ply/000_merged.ply の形式でファイルパスを作る
    frame_str = f"{frame:03d}"
    merged_dir = os.path.join(os.path.dirname(__file__), "merge_ply")
    merged_path = os.path.join(merged_dir, f"{frame_str}_merged.ply")

    if not os.path.exists(merged_path):
        raise HTTPException(status_code=404, detail="Merged file not found")

    return FileResponse(
        merged_path,
        media_type="application/octet-stream",
        filename=f"{frame_str}_merged.ply"
    )


def get_tile_file_paths(frame: int, tile_index):
    frame_str = f"{frame:03d}"
    file_list = []
    for idx in tile_index:
        xi, yi, zi = index2xyz[idx]
        path = os.path.join('get_file', frame_str, f"{frame_str}_tile_{xi}_{yi}_{zi}.ply")
        file_list.append(path)
    return file_list

def log_merge_time(frame, start, end):
    elapsed = (end - start) * 1000
    start_ms = start * 1000
    end_ms = end * 1000
    file_exists = os.path.exists(LOG_PATH)
    with open(LOG_PATH, "a", encoding="utf-8") as f:
        if not file_exists:
            f.write("Frame,MergeStartTime(ms),MergeEndTime(ms),Elapsed(ms)\n")
        f.write(f"{frame},{start_ms:.3f},{end_ms:.3f},{elapsed:.3f}\n")