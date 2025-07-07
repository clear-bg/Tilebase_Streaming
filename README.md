# Tilebase_Streaming

## 概要

このプロジェクトは、点群データ（PLY ファイル）をタイル分割し、ストリーミング方式で Unity 上でレンダリングするシステムです。  
Python (FastAPI) を用いて点群ファイルリクエストを処理し、Unity からの要求に応じてタイルごとの PLY ファイルをレスポンスします。

## ディレクトリ構造

```
Streming/
├── README.md
├── .gitignore
├── directory.txt
├── Server/
│   ├── app.py
│   ├── endpoint.py
│   ├── manage_time.py
│   ├── merge_ply.py
│   ├── tile_index.py
│   ├── utils.py
│   ├── get_file/
│   │   ├── split_20_to_2_3_2/
│   │   │   ├── 000/
│   │   │   │    ├── 000_tile_0_0_0.ply
│   │   │   │    ├── ...
│   │   │   │    └── 000_tile_1_2_1.ply
│   │   │   ├── 001/
│   │   │   ├── ...
│   │   │   └── 299/
│   │   └── split_20_to_5_5_5/
│   │       └── *.ply              # into plyfile as same as 2x3x2
│   ├── Original_ply_20/
│   │   └── *.ply                  # 000.ply~299.ply
│   ├── merge_ply/
│   │   └── *.ply                  # 000.ply~299.ply
│   └── merge_logs/
│       └── *.csv                  # csv logs
├── Tile_distribute/
│   ├── split_ply.py
│   ├── output.csv
│   └── Original_ply_20/
│       └── *.ply                  # 000.ply~299.ply
└── Tilebase_Streaming/ (Unityプロジェクト)
    └── Assets/
        ├── Scripts/
        │   ├── Download.cs
        │   ├── Rendering.cs
        │   ├── PointCloudImporter.cs
        │   └── CameraController.cs
        └── Pcx/
```
