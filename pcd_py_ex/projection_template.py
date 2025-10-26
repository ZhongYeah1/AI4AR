import numpy as np
import cv2
import open3d as o3d
from matplotlib import pyplot as plt

#to do: Read in 3D point cloud data, and use index.txt to select the relevant points. Return an array 
def read_selected_points(pcd_path, index_path):
    pcd = o3d.io.read_point_cloud(pcd_path)
    points = np.asarray(pcd.points)

    indices = []
    with open(index_path, 'r') as f:
        for line in f: 
            line = line.strip()
            if line: 
                indices.append(int(line))
    raw_points_3d = points[indices]
    return np.array(raw_points_3d) # (n, 3) array

#outputs the rotation matrix for rotating a point about the z-axis by "angle" degrees
def make_rot_z(angle):
    cost = np.cos(np.deg2rad(angle))
    sint = np.sin(np.deg2rad(angle))
    rot = np.array([[cost, -sint, 0],
                   [sint, cost, 0],
                   [0, 0, 1]])
    return rot

#outputs the rotation matrix for rotating a point about the y-axis by "angle" degrees
def make_rot_y(angle):
    cost = np.cos(np.deg2rad(angle))
    sint = np.sin(np.deg2rad(angle))
    rot = np.array([[cost, 0, sint],
                   [0, 1, 0],
                   [-sint, 0, cost]])
    return rot

#outputs the rotation matrix for rotating a point about the x-axis by "angle" degrees
def make_rot_x(angle):
    cost = np.cos(np.deg2rad(angle))
    sint = np.sin(np.deg2rad(angle))
    rot = np.array([[1, 0, 0],
                   [0, cost, -sint],
                   [0, sint, cost]])
    return rot

def main():
    # camera intrinsic parameters
    fx, fy = 480.48747618769386, 481.6413211455262
    cx, cy = 325.2603816359703, 176.02738209603658

    #various paths
    root = ""
    pcd_path = root + "point_cloud.pcd"
    index_path = root + "index.txt"
    img_path = root + "image.png"

    #read in selected points from point cloud
    points_3d = read_selected_points(pcd_path, index_path)

    #camera intrinsice matrix for projectPoints
    intrinsic_matrix = np.array([
        [fx, 0, cx],
        [0, fy, cy],
        [0, 0, 1]
    ],np.float32)

    
    #to do: project the points onto 2D image
    alpha = -90 # x
    beta = 90 # y
    gamma = 0 # z 

    rotation_matrix = make_rot_z(-gamma) @ make_rot_y(-beta) @ make_rot_x(-alpha)
    rvec, _ = cv2.Rodrigues(rotation_matrix)
    
    tvec = np.array([0., 0., 0.], np.float32)

    points_2d, _ = cv2.projectPoints(points_3d,
                                     rvec,
                                     tvec,
                                     intrinsic_matrix,
                                     None
                                    )

    #to do: Load the scene image, draw the projected points on the image, and save the results to disk
    img = cv2.imread(img_path)

    for point in points_2d: 
        x, y = int(point[0][0]), int(point[0][1])
        cv2.circle(img, (x, y), 5, (0, 0, 255), 2)
    
    cv2.imwrite(root + "results.png", img)

    #bonus to do: Remove some outliers and convert the dots into a bounding box, and write the resuts to disk
    img_box = cv2.imread(img_path)

    points_2d_array = points_2d.reshape(-1, 2)

    mean_x, std_x = np.mean(points_2d_array[:, 0]), np.std(points_2d_array[:, 0])
    mean_y, std_y = np.mean(points_2d_array[:, 1]), np.std(points_2d_array[:, 1])
    
    z_scores_x = np.abs((points_2d_array[:, 0] - mean_x) / (std_x + 1e-6))
    z_scores_y = np.abs((points_2d_array[:, 1] - mean_y) / (std_y + 1e-6))

    threshold = 2.0
    mask = (z_scores_x < threshold) & (z_scores_y < threshold)
    filtered_points = points_2d_array[mask]

    if len(filtered_points) > 0:
        min_x = int(np.min(filtered_points[:, 0]))
        max_x = int(np.max(filtered_points[:, 0]))
        min_y = int(np.min(filtered_points[:, 1]))
        max_y = int(np.max(filtered_points[:, 1]))

        cv2.rectangle(img_box, (min_x, min_y), (max_x, max_y), (0, 0, 255), 2)
    
    cv2.imwrite(root + "results_box.png", img_box)

if __name__ == '__main__':
    main()
