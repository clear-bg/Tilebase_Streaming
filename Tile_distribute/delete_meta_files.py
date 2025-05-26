import os

# 対象ディレクトリ
dir_path = r"C:\Users\clear\Project\Tile_distribute\Original_Ply"

# 0～299までループしてファイル削除
for i in range(300):
    filename = f"{i}.ply.meta"
    filepath = os.path.join(dir_path, filename)
    
    if os.path.exists(filepath):
        try:
            os.remove(filepath)
            print(f"削除しました: {filepath}")
        except Exception as e:
            print(f"削除失敗: {filepath} エラー: {e}")
    else:
        print(f"存在しません: {filepath}")
