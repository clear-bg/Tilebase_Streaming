import os

def rename_ply_files():
    try:
        # スクリプトが存在するディレクトリを取得
        folder_path = os.path.dirname(os.path.abspath(__file__))
        
        # フォルダ内のファイルを取得
        files = [f for f in os.listdir(folder_path) if f.endswith('.ply')]

        # ファイル名を昇順にソート
        files.sort(key=lambda x: int(x.split('.')[0]))

        for i, file_name in enumerate(files):
            # 新しいファイル名を作成
            new_name = f"loot_vox10_{i:04d}.ply"

            # 元のファイルパスと新しいファイルパスを作成
            old_file_path = os.path.join(folder_path, file_name)
            new_file_path = os.path.join(folder_path, new_name)

            # ファイル名を変更
            os.rename(old_file_path, new_file_path)

        print(f"{len(files)}個のファイル名を変更しました。")
    except Exception as e:
        print(f"エラーが発生しました: {e}")

# 実行
rename_ply_files()
