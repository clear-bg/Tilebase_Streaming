import os

# カレントディレクトリ内のファイル一覧を取得
for filename in os.listdir():
    if filename.endswith(".ply"):
        name_without_ext = os.path.splitext(filename)[0]
        if name_without_ext.isdigit():
            number = int(name_without_ext)
            if 0 <= number <= 299:
                new_name = f"{number:03}.ply"
                if filename != new_name:
                    os.rename(filename, new_name)
                    print(f"Renamed: {filename} -> {new_name}")
