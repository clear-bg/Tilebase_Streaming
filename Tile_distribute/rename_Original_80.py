import os

# 対象フォルダのパスを指定（必要に応じて変更）
folder_path = r"Original_ply_80"

# ファイル名をリネーム
for i in range(300):
    old_name = f"loot_vox10_{i:04d}.ply"
    new_name = f"{i:03d}.ply"

    old_path = os.path.join(folder_path, old_name)
    new_path = os.path.join(folder_path, new_name)

    if os.path.exists(old_path):
        os.rename(old_path, new_path)
        print(f"Renamed: {old_name} → {new_name}")
    else:
        print(f"Not found: {old_name}")
