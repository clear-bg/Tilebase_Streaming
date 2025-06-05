r'''
uvicorn Initialize.app:app --host 0.0.0.0 --port 8000 --reload

C:\Users\clear\AppData\Roaming\Python\Python310\Scripts\uvicorn.exe Initialize.app:app --host 0.0.0.0 --port 8000 --reload
'''

from fastapi import FastAPI, HTTPException
from fastapi.responses import FileResponse
import os

app = FastAPI()

# get_file フォルダの絶対パスを構成（Scripts_Reference から見て ../get_file）
BASE_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), 'get_file'))


@app.get("/get_file/{index}.ply")
async def get_file(index: int):
    file_path = os.path.join(BASE_DIR, f"{index}.ply")
    
    if not os.path.exists(file_path):
        raise HTTPException(status_code=404, detail="File not found")

    return FileResponse(file_path, media_type="application/octet-stream")

