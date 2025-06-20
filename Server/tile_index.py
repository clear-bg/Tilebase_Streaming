def get_index_list(gx: int, gy: int, gz: int):
    return [(x, y, z) for x in range(gx) for y in range(gy) for z in range(gz)]
