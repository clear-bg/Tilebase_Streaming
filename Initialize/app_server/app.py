'''--- 実行方法 ---
uvicorn app:app --host 127.0.0.1 --port 8000 --reload

テスト用 ping
curl http://127.0.0.1:8000
curl "http://127.0.0.1:8000/get_file?frame=0&tiles=0,1,2&grid=2_3_2"
'''

from fastapi import FastAPI
from endpoint import register_endpoints

app = FastAPI()

# ルーティング登録
register_endpoints(app)