import uvicorn
from fastapi import FastAPI, File, UploadFile, BackgroundTasks
from fastapi.responses import FileResponse

import open3d as o3d
# import shutil
import os
import subprocess
import time
import csv
from pathlib import Path

app = FastAPI()
UPLOAD_DIR = "./files"
filename=""
number = -1
number_bl = 0
number_el1 = 0
number_el2 = 0
number_el3 = 0

# ファイルリクエストを受けた際の処理    
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