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

# from search_file import search_file

app = FastAPI()
UPLOAD_DIR = "./files"
filename=""
number = -1
number_bl = 0
number_el1 = 0
number_el2 = 0
number_el3 = 0

# 指定ディレクトリ内の.plyファイルから最終更新日時が最新のファイルを返す
def search_file(dirpath):
    # print(dirpath)
    p = Path(".")
    # files = list(p.glob("*"))
    files = list(p.glob(os.path.join(dirpath, "*.ply")))
    # print(files)
    file_updates = {file_path: os.stat(file_path).st_mtime for file_path in files}

    newest_file_path = max(file_updates, key=file_updates.get)
    return os.path.basename(newest_file_path)

# @app.get("/get_file/{quality}/{filename:path}")
# async def get_file(filename: str, quality:int):
#     file_path = "./compressed_data/"
#     if quality == 0:
#         file_path = file_path + "base/"
#     else:
#         file_path = file_path + f"enhance{quality}/"
#     file_path = file_path + filename
#     try:
#         response = FileResponse(
#                                 path=file_path,
#                                 filename=filename
#                                 )
#         return response
#     except FileNotFoundError:
#         return {"Error": "ファイルが見つかりません。"}

# 帯域正弦関数（tcコマンド）
def run_command(command):
    try:
        # print(command)
        subprocess.run(command, check=True, shell=True)
    except subprocess.CalledProcessError as e:
        print(f"コマンド実行エラー: {e}")

def setup_bandwidth_limit(interface, limit, buffer, latency):
    # run_command(f"tc qdisc del dev {interface} root")
    run_command(f"tc qdisc add dev {interface} root tbf rate {limit}Mbit burst {buffer}Kb limit {latency}Kb")

def change_bandwidth_limit(interface, new_limit, new_buffer, new_latency):
    run_command(f"tc qdisc change dev {interface} root tbf rate {new_limit}Mbit burst {new_buffer}Kb limit {new_latency}Kb")

def tc_task():
    # with open('./app/report_bicycle_0001.csv', newline='') as file:
    with open('./app/BandwidthData2.csv', newline='') as file:
        reader = csv.reader(file)
        next(reader)
        data = list(reader)
        setup_bandwidth_limit('eth0', int(float(data[0][1])), int(float(data[0][1]))*0.1, int(float(data[0][1]))*0.1)
        # print(f"tc qdisc add dev eth0 root tbf rate {int(float(data[0][1]))*0.1*0.001}Kbit burst {int(float(data[0][1]))*0.1*0.001/8}Kb latency {0}ms")
        # print(data[0][1]+"kb")
        time.sleep(5)

    for i, row in enumerate(data):
        if i == 0:
            continue
        change_bandwidth_limit('eth0', int(float(row[1])), int(float(row[1]))*0.1, int(float(row[1]))*0.1)
        # print(row[1]+"mb")
        time.sleep(5)

@app.get("/tc_start")
async def tc_start(background_tasks: BackgroundTasks):
    print("tc start!")
    background_tasks.add_task(tc_task)
    return {"message": "tc start!"}

@app.get("/tc_stop")
async def tc_stop(background_tasks: BackgroundTasks):
    print("tc stop!")
    background_tasks.add_task(tc_task)
    return {"message": "tc start!"}


@app.get("/tc_set")
def tc_set():
    print("enter setup")
    setup_bandwidth_limit("eth0", 3.0, 3.0/8, 30)
    # print("enter change")
    # change_bandwidth_limit("eth0", 2.0, 2.0/8, 30)

# -------------------------------------------------------
# ファイルリクエストを受けた際の処理    
@app.get("/get_file/{quality}/{idx}")
async def get_file(quality:int, idx:int):
    global number
    file_path = "./app/compressed_data/"
    if quality == 0:
        file_path = file_path + "base"
    # elif quality == 1:
    #     file_path = file_path + f"enhance1"
    # elif quality == 2:
    #     file_path = file_path + f"enhance2"
    # elif quality == 3:
    #     file_path = file_path + f"enhance3"
    else:
        file_path = file_path + f"enhance" + str(quality)
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
        # number+=1
        return response
    except FileNotFoundError:
        return {"Error": "ファイルが見つかりません。"}
    
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

# ここから下はもう使ってない
#########################################################################
@app.get("/get_file/{quality}/")
async def get_file(quality:int):
    global number
    file_path = "./app/compressed_data/"
    if quality == 0:
        file_path = file_path + "base"
        number += 1
        response_file = f"{number}.ply"
    elif quality == 1:
        file_path = file_path + f"enhance1"
        response_file = f"{number}.ply"
    elif quality == 2:
        file_path = file_path + f"enhance2"
        response_file = f"{number}.ply"
    elif quality == 3:
        file_path = file_path + f"enhance3"
        response_file = f"{number}.ply"
    # response_idx = int(search_file(file_path)[0:-4])
    # response_file = str(response_idx-1) + ".ply"
    file_path = os.path.join(file_path, response_file)
    try:
        # print(file_path + "を送り返したよ")
        response = FileResponse(
                                path=file_path,
                                filename=response_file
                                )
        # number+=1
        return response
    except FileNotFoundError:
        return {"Error": "ファイルが見つかりません。"}
    
@app.get("/get_vgffile/{quality}/")
async def get_file(quality:int):
    file_path = "./app/compressed_data/"
    if quality == 0:
        file_path = file_path + "voxelgrid"
    elif quality == 1:
        file_path = file_path + "voxelgrid0003"
    elif quality == 2:
        file_path = file_path + "voxelgrid00015"
    # response_idx = int(search_file(file_path)[0:-4])
    # response_file = str(response_idx-1) + ".ply"
    response_file = "0.ply"
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


@app.post("/files/")
async def file(file: bytes = File(...)):
    content = file.decode('utf-8')
    formatfile = content.split('\n')
    return {'filedetail': formatfile}

@app.post("/uploadfile/")
async def upload_file(background_tasks: BackgroundTasks, file: UploadFile = File(...)):
    global filename
    if file:
        filename = file.filename
        fileobj = file.file
        upload_dir = open(os.path.join(UPLOAD_DIR, filename),'wb+')
        # shutil.copyfileobj(fileobj, upload_dir)
        upload_dir.close()
        background_tasks.add_task(scalable_coding, dirpath=UPLOAD_DIR, filename=filename, num=4)
        return {"アップロードファイル名": filename}
    return {"Error": "アップロードファイルが見つかりません。"}

def scalable_coding(dirpath,filename,num):
    pcd = o3d.io.read_point_cloud(os.path.join(dirpath,filename))
    target_pcd = pcd
    drop_index = []
    for i in range(num):
        if i == 0:
            dirname = "base"
        else:
            dirname = f"enhance{i}"
        down_sampling(target_pcd, dirname, filename, num)
        drop_index.append(i)
        target_pcd = pcd.select_by_index(drop_index, invert=True) # データのうち、最初のnum個を削除する
    # end_time = time.time()

def down_sampling(pcd, dirname, filename, num):
    pcd = pcd.uniform_down_sample(num) # num個おきにサンプリングする
    save_dir = os.path.join("compressed_data",dirname)
    if not os.path.exists(save_dir):
        os.mkdir(save_dir)
    o3d.io.write_point_cloud(os.path.join(save_dir,filename), pcd, write_ascii=False)
