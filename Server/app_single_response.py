import uvicorn
from fastapi import FastAPI, Query, File, UploadFile, BackgroundTasks
from fastapi.responses import FileResponse
from fastapi.responses import StreamingResponse
from typing import List

import open3d as o3d
# import shutil
import os
import subprocess
import time
import csv
from pathlib import Path

app = FastAPI()


# ファイルリクエストを受けた際の処理
@app.get("/get_tile")
async def get_tile(idx: int, tile: List[str] = Query(...)):
    # idx: 点群番号 (例: 123)
    # tile: タイル番号 (例: "0_0_0")
    base_path = os.path.join(os.path.dirname(__file__), "../Tile_distribute/Split", f"{idx:03d}")
    filename = f"{idx:03d}_tile_{tile}.ply"
    file_path = os.path.join(base_path, filename)

    if os.path.exists(file_path):
        return FileResponse(file_path, filename=filename)
    else:
        return {"Error": f"ファイルが見つかりません {filename}"}
    


@app.get("/get_tile")
async def get_tile(idx: int, tile: List[str] = Query(...)):
    boundary = "----WebKitFormBoundary7MA4YWxkTrZu0gW"
    async def file_generator():
        for t in tile:
            base_path = os.path.join(os.path.dirname(__file__), "../Tile_distribute/Split", f"{idx:03d}")
            filename = f"{idx:03d}_tile_{t}.ply"
            file_path = os.path.join(base_path, filename)
            if os.path.exists(file_path):
                yield f"--{boundary}\r\n".encode()
                yield f'Content-Disposition: form-data; name="file"; filename="{filename}"\r\n'.encode()
                yield b"Content-Type: application/octet-stream\r\n\r\n"
                with open(file_path, "rb") as f:
                    while chunk := f.read(8192):
                        yield chunk
                yield b"\r\n"
            else:
                print(f"ファイルが見つかりません: {file_path}")
        yield f"--{boundary}--\r\n".encode()

    headers = {
        "Content-Type": f"multipart/mixed; boundary={boundary}",
        "Content-Disposition": f'attachment; filename="tiles_{idx}.ply"'
    }
    return StreamingResponse(file_generator(), headers=headers)


'''
UPLOAD_DIR = "./files"
filename=""
number = -1
number_bl = 0
number_el1 = 0
number_el2 = 0
number_el3 = 0

@app.get("/get_vgffile/{quality}/{idx}")
async def get_file(quality:int, idx:int):
    file_path = "./app/compressed_data/"
    if quality == 0:
        file_path = file_path + "voxelgrid00045"
    elif quality == 1:
        file_path = file_path + "voxelgrid0003"
    elif quality == 2:
        file_path = file_path + "voxelgrid00015"
    elif quality == 3:
        file_path = file_path + "raw"
    # response_idx = int(search_file(file_path)[0:-4])
    # response_file = str(response_idx-1) + ".ply"
    response_file = str(idx) + ".ply"
    file_path = os.path.join(file_path, response_file)
    try:
        # print(file_path + "を送り返したよ")
        response = FileResponse(
                                path=file_path,
                                filename=response_file
                                )
        return response
    except FileNotFoundError:
        return {"Error": "ファイルが見つかりません。"}
'''