'''
uvicorn Server.app_single_response:app --host 0.0.0.0 --port 8000 --reload
'''
#---------------------------------------------------
from fastapi import FastAPI
from fastapi.responses import FileResponse
import os

app = FastAPI()

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