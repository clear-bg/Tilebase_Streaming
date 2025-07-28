from fastapi import FastAPI, HTTPException
from fastapi.responses import FileResponse
from tile_index import get_index_list
from manage_time import log_merge_time
from merge_ply import merge_ply_files
from utils import get_tile_file_paths
import os
import time
from os.path import exists
import glob

def register_endpoints(app: FastAPI):

    @app.get("/get_xml")
    async def get_xml(frame: int, grid: str = "2_3_2"):
        dataset = f"split_20_to_{grid}"
        xml_path = os.path.join(
            os.path.dirname(__file__), "get_file", dataset,
            f"{frame:03d}", "tiles.xml"
        )

        if not os.path.exists(xml_path):
            raise HTTPException(status_code=404, detail="XML file not found")

        return FileResponse(
            xml_path,
            media_type= "application/xml",
            filename = f"{frame:03d}.xml"
        )

    @app.get("/get_file")
    async def get_file(dataset: str, frame: int, tiles: str, grid: str = "2_3_2"):
        try:
            tile_index = [int(x) for x in tiles.split(",")]
            gx, gy, gz = map(int, grid.split("_"))
        except Exception:
            raise HTTPException(status_code=400, detail="Invalid parameters")

        index2xyz = get_index_list(gx, gy, gz)
        if any(idx < 0 or idx >= len(index2xyz) for idx in tile_index):
            raise HTTPException(status_code=400, detail="Tile index out of range")

        base_dir = os.path.join(os.path.dirname(__file__), "get_file", dataset)
        file_list = get_tile_file_paths(frame, tile_index, index2xyz, base_dir, include_frame_in_name=False)
        file_list = [p for p in file_list if os.path.exists(p)]
        if not file_list:
            raise HTTPException(status_code=404, detail="No tile files found")

        start = time.time()
        try:
            merged_path = merge_ply_files(file_list, frame)
        except Exception as e:
            raise HTTPException(status_code=500, detail=str(e))
        end = time.time()

        # グリッドサイズに応じてログファイル名を切替
        grid_name = f"{gx}x{gy}x{gz}"
        tile_count = len(tile_index)
        log_filename = f"{grid_name}_{tile_count}tiles"

        # frame == 0 のときのみ、ログCSVを削除
        if frame == 0:
            csv_dir = os.path.join(os.path.dirname(__file__), "merge_logs")
            pattern = os.path.join(csv_dir, f"merge_time_{log_filename}.csv")
            for f in glob.glob(pattern):
                os.remove(f)

        # 初回のみ: マージ済みPLYファイルを削除
        if frame == 0:
            merged_dir = os.path.join(os.path.dirname(__file__), "merge_ply")
            for f in glob.glob(os.path.join(merged_dir, "*.ply")):
                os.remove(f)

        # log_merge_time(frame, start, end, endpoint_name=log_filename)

        return FileResponse(
            merged_path,
            media_type="application/octet-stream",
            filename=f"{frame:03d}_merged.ply"
        )


    @app.get("/merge_ply")
    async def merge_ply(frame: int):
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

    @app.get("/Original_ply_20")
    async def get_original_ply(frame: int):
        frame_str = f"{frame:03d}"
        original_dir = os.path.join(os.path.dirname(__file__), "Original_ply_20")
        original_path = os.path.join(original_dir, f"{frame_str}.ply")

        if not os.path.exists(original_path):
            raise HTTPException(status_code=404, detail="Original PLY file not found")

        return FileResponse(
            original_path,
            media_type="application/octet-stream",
            filename=f"{frame_str}.ply"
        )
