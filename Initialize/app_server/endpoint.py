from fastapi import FastAPI, HTTPException
from fastapi.responses import FileResponse
from tile_index import get_index_list
from manage_time import log_merge_time
from merge_ply import merge_ply_files
from utils import get_tile_file_paths
import os
import time
from os.path import exists

def register_endpoints(app: FastAPI):

    @app.get("/get_file")
    async def get_file(frame: int, tiles: str, grid: str = "2_3_2"):
        try:
            tile_index = [int(x) for x in tiles.split(",")]
            gx, gy, gz = map(int, grid.split("_"))
        except Exception:
            raise HTTPException(status_code=400, detail="Invalid parameters")

        index2xyz = get_index_list(gx, gy, gz)
        if any(idx < 0 or idx >= len(index2xyz) for idx in tile_index):
            raise HTTPException(status_code=400, detail="Tile index out of range")

        base_dir = os.path.join(os.path.dirname(__file__), "get_file")
        file_list = get_tile_file_paths(frame, tile_index, index2xyz, base_dir)

        start = time.time()
        try:
            merged_path = merge_ply_files(file_list, frame)
        except Exception as e:
            raise HTTPException(status_code=500, detail=str(e))
        end = time.time()

        log_merge_time(frame, start, end)

        return FileResponse(
            merged_path,
            media_type="application/octet-stream",
            filename=f"{frame:03d}_merged.ply"
        )

    @app.get("/split_20_to_5_5_5")
    async def get_file_5x5x5(frame: int, tiles: str):
        try:
            tile_index = [int(x) for x in tiles.split(",")]
        except Exception:
            raise HTTPException(status_code=400, detail="Invalid tile indices")

        index2xyz = get_index_list(5, 5, 5)
        if any(idx < 0 or idx >= len(index2xyz) for idx in tile_index):
            raise HTTPException(status_code=400, detail="Tile index out of range")

        base_dir = os.path.join(os.path.dirname(__file__), "split_20_to_5_5_5")
        all_paths = get_tile_file_paths(frame, tile_index, index2xyz, base_dir)

        # ✅ 存在するファイルだけに絞る
        file_list = [path for path in all_paths if os.path.exists(path)]
        if not file_list:
            raise HTTPException(status_code=404, detail="No tile files found")

        start = time.time()
        try:
            merged_path = merge_ply_files(file_list, frame)
        except Exception as e:
            raise HTTPException(status_code=500, detail=str(e))
        end = time.time()

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
