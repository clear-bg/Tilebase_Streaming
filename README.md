# Tilebase_Streaming

## 概要
このプロジェクトは、点群データ（PLYファイル）をタイル分割し、ストリーミング方式でUnity上でレンダリングするシステムです。  
Python (FastAPI) を用いて点群ファイルリクエストを処理し、Unityからの要求に応じてタイルごとのPLYファイルをレスポンスします。

```
## 構成
Project/
├── README.md
├── .gitignore
├── Server/
│   ├── app.py
├── Tili_distribute/
│   ├── Original_ply/
│   │   ├── 0.ply
│   │   ├── 1.ply
│   │   ├── ...
│   │   └── 299.ply
│   └── Split/
│       ├── 000/
│       │   ├── 000_tile_0_0_0.ply
│       │   ├── ...
│       │   └── 000_tile_1_2_1.ply
│       ├── 001/
│       ├── ...
|       ├── 299/
│       ├── delete_meta_files.py
│       └── split_ply.py
├── Tilibase_Streaming/ (Unityプロジェクト)
│   ├── Assets/
│   │   └── Scripts/
│   │       ├── Download.cs
│   │       └── Rendering.cs
│   ├── Download_ply/
│   └── Pcx/
└── ディレクトリ構造.txt
```