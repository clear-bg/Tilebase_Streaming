r'''
サーバの起動は必ず Streming/Initialize ディレクトリから行うこと
uvicorn app_Ini:app --host 0.0.0.0 --port 8000 --reload

C:\Users\clear\AppData\Roaming\Python\Python310\Scripts\uvicorn.exe Initialize.app:app --host 0.0.0.0 --port 8000 --reload
'''

from fastapi import FastAPI, HTTPException
from fastapi.responses import FileResponse
import os

app = FastAPI()

# get_file フォルダの絶対パスを構成（Scripts_Reference から見て ../get_file）
BASE_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), 'get_file'))

# merge_ply フォルダの絶対パスを構成（Server ディレクトリの merge_ply）
MERGE_PLY_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), '../Server/merge_ply'))


@app.get("/get_file/{index}.ply")
async def get_file(index: int):
    file_path = os.path.join(BASE_DIR, f"{index}.ply")

    if not os.path.exists(file_path):
        raise HTTPException(status_code=404, detail="File not found")

    return FileResponse(file_path, media_type="application/octet-stream")

# --- 追加する部分 ---
@app.get("/get_merged/{index}.ply")
async def get_merged(index: int):
    # indexを3桁ゼロ埋め
    index_str = f"{index:03}"
    file_path = os.path.join(MERGE_PLY_DIR, f"{index_str}_merged.ply")
    if not os.path.exists(file_path):
        raise HTTPException(status_code=404, detail="File not found")
    return FileResponse(file_path, media_type="application/octet-stream")