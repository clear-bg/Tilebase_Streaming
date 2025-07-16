import os
import numpy as np
import plyfile
import xml.etree.ElementTree as ET

input_base_dir = "split_hierarchical_20_to_2_3_2"
x_splits, y_splits, z_splits = 2, 3, 2

for frame_num in range(300):
    frame_str = f"{frame_num:03d}"
    frame_dir = os.path.join(input_base_dir, frame_str)
    if not os.path.exists(frame_dir):
        print(f"[スキップ] フォルダなし: {frame_dir}")
        continue

    tile_bounds = []

    for yi in range(y_splits):
        for xi in range(x_splits):
            for zi in range(z_splits):
                file_name = f"{frame_str}_tile_{xi}_{yi}_{zi}.ply"
                file_path = os.path.join(frame_dir, file_name)

                if not os.path.exists(file_path):
                    print(f"[スキップ] ファイルなし: {file_path}")
                    continue

                plydata = plyfile.PlyData.read(file_path)
                vertex = plydata['vertex']
                if len(vertex.data) == 0:
                    # 空タイル → NaNで記録
                    min_x = min_y = min_z = float('nan')
                    max_x = max_y = max_z = float('nan')
                else:
                    min_x = vertex['x'].min()
                    min_y = vertex['y'].min()
                    min_z = vertex['z'].min()
                    max_x = vertex['x'].max()
                    max_y = vertex['y'].max()
                    max_z = vertex['z'].max()

                tile_bounds.append({
                    'x': xi, 'y': yi, 'z': zi,
                    'min': (min_x, min_y, min_z),
                    'max': (max_x, max_y, max_z)
                })

    # XML生成
    root = ET.Element("Tiles")
    for tb in tile_bounds:
        tile_elem = ET.SubElement(root, "Tile", attrib={
            'x': str(tb['x']), 'y': str(tb['y']), 'z': str(tb['z'])
        })
        ET.SubElement(tile_elem, "Min", attrib={
            'x': str(tb['min'][0]), 'y': str(tb['min'][1]), 'z': str(tb['min'][2])
        })
        ET.SubElement(tile_elem, "Max", attrib={
            'x': str(tb['max'][0]), 'y': str(tb['max'][1]), 'z': str(tb['max'][2])
        })

    xml_path = os.path.join(frame_dir, "tiles.xml")
    ET.ElementTree(root).write(xml_path, encoding='utf-8', xml_declaration=True)
    print(f"[出力] {xml_path}")
