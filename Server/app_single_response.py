'''
uvicorn Server.app_single_response:app --host 0.0.0.0 --port 8000 --reload
'''
#---------------------------------------------------
from fastapi import FastAPI
from fastapi.responses import FileResponse
import os

app = FastAPI()

tile_id_map = [
    "0_0_0", "0_0_1", "0_1_0", "0_1_1", "0_2_0", "0_2_1",
    "1_0_0", "1_0_1", "1_1_0", "1_1_1", "1_2_0", "1_2_1"
]

@app.get("/get_file/{tileID}/{frameIndex}")
async def get_file(tileID: int, frameIndex: int):
    if tileID < 0 or tileID >= len(tile_id_map):
        return {"error": "Invalid tileID"}
    tile_name = tile_id_map[tileID]
    frame_str = f"{frameIndex:03d}"
    file_path = f"./Server/get_file/{frame_str}/{frame_str}_tile_{tile_name}.ply"
    
    if not os.path.exists(file_path):
        return {"error": "File not found"}
    
    return FileResponse(file_path)