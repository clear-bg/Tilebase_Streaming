import os

def rename_ply_files_in_directory(base_dir):
    for root, dirs, files in os.walk(base_dir):
        for file in files:
            if file.endswith(".ply") and "_" in file:
                # 例: 000_tile_1_2_3.ply → tile_1_2_3.ply
                parts = file.split("_", 1)
                if len(parts) == 2 and parts[1].startswith("tile"):
                    old_path = os.path.join(root, file)
                    new_path = os.path.join(root, parts[1])
                    os.rename(old_path, new_path)
                    print(f"Renamed: {old_path} -> {new_path}")

# 使用例（変更したいベースディレクトリに合わせて修正）
base_directory = r"C:\Users\clear\Project\Streming\Server\get_file\split_20_to_5_5_5"
rename_ply_files_in_directory(base_directory)
