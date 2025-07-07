import os
from typing import List, Tuple

def get_tile_file_paths(frame: int, tile_index: List[int], index2xyz: List[Tuple[int, int, int]], base_dir: str, include_frame_in_name: bool = True) -> List[str]:
    file_list = []
    frame_str = f"{frame:03d}"
    for idx in tile_index:
        x, y, z = index2xyz[idx]
        if include_frame_in_name:
            file_name = f"{frame_str}_tile_{x}_{y}_{z}.ply"
        else:
            file_name = f"tile_{x}_{y}_{z}.ply"
        file_path = os.path.join(base_dir, frame_str, file_name)
        file_list.append(file_path)
    return file_list

