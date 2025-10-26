import open3d as o3d

def main(): 
    pcd = o3d.io.read_point_cloud("point_cloud.pcd")

    o3d.visualization.draw_geometries_with_vertex_selection([pcd])

if __name__ == '__main__': 
    main()