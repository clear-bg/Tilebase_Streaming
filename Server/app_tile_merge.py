r'''
uvicorn Server.app_tile_merge:app --host 0.0.0.0 --port 8000 --reload

uvicorn app_tile_merge:app --host 0.0.0.0 --port 8000 --reload

C:\Users\clear\AppData\Roaming\Python\Python310\Scripts\uvicorn.exe Initialize.app:app --host 0.0.0.0 --port 8000 --reload
'''

from fastapi import FastAPI, HTTPException
from fastapi.responses import FileResponse
import os

from Server.merge_ply import merge_ply_files

app = FastAPI()

# get_file フォルダの絶対パスを構成（Scripts_Reference から見て ../get_file）
# BASE_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), 'get_file'))

# インデックス→x, y, z変換表（0〜11）
index2xyz = [
    (0, 0, 0), (0, 0, 1), (0, 1, 0), (0, 1, 1),
    (0, 2, 0), (0, 2, 1),
    (1, 0, 0), (1, 0, 1), (1, 1, 0), (1, 1, 1), (1, 2, 0), (1, 2, 1)
]

@app.get("/get_file")
async def get_file(frame: int, tiles: str):
    try:
        tile_index = [int(x) for x in tiles.split(",")]
    except Exception:
        raise HTTPException(status_code=400, detail="Invalid tile indices format")

    file_list = get_tile_file_paths(frame, tile_index)

    # merge_ply.pyの関数を呼び出し
    try:
        merged_path = merge_ply_files(file_list, frame)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

    return FileResponse(
        merged_path,
        media_type="application/octet-stream",
        filename=f"{frame:03d}_merged.ply"
    )

def get_tile_file_paths(frame: int, tile_index):
    frame_str = f"{frame:03d}"
    file_list = []
    for idx in tile_index:
        xi, yi, zi = index2xyz[idx]
        path = os.path.join('get_file', frame_str, f"{frame_str}_tile_{xi}_{yi}_{zi}.ply")
        file_list.append(path)
    return file_list