import os
import json
import numpy as np


# Simple linear resampling
def resample_points(points, num_points):
    """
    Resamples a list of points to exactly num_points
        - linear interpolation over cumulative path distance.
        - For 1D gestures/strokes
    """
    pts = np.array(points)
    N = len(pts)

    if N == 0:
        return np.zeros((num_points, 3))  # edge case
    if N == 1:
        return np.repeat(pts, num_points, axis=0) # repeat

    # Compute cumulative distance along the path
    deltas = pts[1:] - pts[:-1]
    segment_lengths = np.linalg.norm(deltas, axis=1)
    cumulative = np.insert(np.cumsum(segment_lengths), 0, 0)
    total_len = cumulative[-1]

    # Create equally spaced target distances
    target_distances = np.linspace(0, total_len, num_points)

    # Interpolate separately for x, y, z
    resampled = np.zeros((num_points, 3))
    for dim in range(3):
        resampled[:, dim] = np.interp(
            target_distances,
            cumulative,
            pts[:, dim]
        )

    return resampled.tolist()


def sample_points_fixed(points, N=128, seed=None):
    """
    Uniformly sample a point list to exactly N points.
    - If len(points) > N: uniform downsample
    - If len(points) < N: randomly repeat some points
    """
    if seed is not None:
        np.random.seed(seed)

    points = np.array(points)
    L = len(points)

    if L == 0:
        raise ValueError("Cannot sample from an empty point list.")

    # Case 1: More than N, downsample
    if L > N:
        idx = np.linspace(0, L - 1, N, dtype=int)
        return points[idx].tolist()

    # Case 2: Less than N, pad with random repeats
    if L < N:
        needed = N - L
        repeat_idx = np.random.choice(L, size=needed, replace=True)

        padded = np.concatenate([points, points[repeat_idx]], axis=0)
        return padded.tolist()
    
    # Case 3: Already exactly N
    return points.tolist()

import numpy as np


def furthest_point_sampling(points, N=128, seed=None, jitter_ratio=1e-4, jitter_upscale=False):
    """
    Perform Furthest Point Sampling (FPS) on a set of 3D points.

    If less points than N, add random "jitter" points, then perform furthest point sampling.
     - e.g. if N=128, M = 120, add random jittered points until M=196 THEN perform fps
     - (Effectiveness not thoroughly tested, but this avoids zero-values in the distance matrix)

    Args:
        points (array-like): (M, 3) list or numpy array of 3D points
        N (int): number of points to sample
        seed (int, optional): random seed for reproducibility

    Returns:
        sampled_points (list): list of N sampled 3D points
    """
    points = np.asarray(points, dtype=np.float32)
    M = points.shape[0]

    if seed is not None:
        np.random.seed(seed)

    if M == 0:
        raise ValueError("Point list is empty")

    # If less points than N, add random "jitter" points then do fps
    if M <= N:
        # Compute scale of the point cloud
        bbox_min = points.min(axis=0)
        bbox_max = points.max(axis=0)
        scale = np.linalg.norm(bbox_max - bbox_min)

        # Fallback if all points are identical
        if scale == 0:
            scale = 1.0

        repeat_count = N - M
        # Upscale to N + (N / 2)
        if (jitter_upscale):
            repeat_count = (N - M) + (N // 2)
        
        repeat_idx = np.random.choice(M, size=repeat_count, replace=True)
        repeated = points[repeat_idx].copy()

        # Apply jitter ONLY to repeated points
        jitter = np.random.normal(
            loc=0.0,
            scale=jitter_ratio * scale,
            size=repeated.shape
        )
        repeated += jitter

        padded = np.concatenate([points, repeated], axis=0)
        l = padded.tolist()

        # Use normal furthest point sampling on jitter points
        if (jitter_upscale):
            return furthest_point_sampling(l, N, seed=seed)
        else:
            return l
        
    
    # # Choose an initial point randomly
    # idx = np.random.randint(M)

    # Choose the initial point as the furthest point from the centroid
    centroid = np.mean(points, axis=0)
    dists_from_center = np.linalg.norm(points - centroid, axis=1)
    idx = np.argmax(dists_from_center) # Always start at the furthest point
    
    selected = [idx]

    # Initialize distances to infinity
    distances = np.full(M, np.inf)

    for _ in range(1, N):
        last_point = points[selected[-1]]

        # Compute distance to the last selected point
        d = np.linalg.norm(points - last_point, axis=1)

        # Update minimum distances to the selected set
        distances = np.minimum(distances, d)

        # Select the point with the maximum distance
        next_idx = np.argmax(distances)
        selected.append(next_idx)

    return points[selected].tolist()


def set_first_as_origin(points):
    """Treat first point as (0,0,0) - Subtracts first point value from every point."""
    points_copy = points.copy()
    origin = (points_copy[0][0], points_copy[0][1], points_copy[0][2])

    for i in range(len(points_copy)):
        modified = (
            points_copy[i][0] - origin[0],
            points_copy[i][1] - origin[1],
            points_copy[i][2] - origin[2]
        )
        points_copy[i] = modified

    return points_copy
    

def load_drawing_xyz(filepath, 
                     use_relative_origin=True, 
                     resample=True, 
                     resample_size=128, 
                     use_fps=True, # Furthest Point Sampling (fps)
                     use_fixed_resample=False # Will not perform if use_fps is true
                     ):
    """Load a single gesture JSON file and return list of (x, y, z) points."""
    with open(filepath, 'r') as f:
        data = json.load(f)
    
    points = [(p['x'], p['y'], p['z']) for p in data['points']]

    # Preprocessing + normalization steps
    if (use_relative_origin):
        points = set_first_as_origin(points) # Treat 1st point as origin (0,0,0), update other points relative
    
    if (not resample):
        return points

    if (use_fps):
        points = furthest_point_sampling(points, 128, jitter_ratio=1e-2, jitter_upscale=True)
    elif (use_fixed_resample):
        points = sample_points_fixed(points, resample_size) # Resize points via resampling, default size of 128
    
    return points

def apply_augmentation(points, scale_factor=0.05, shear_strength=0.15):
    """
    Applies jitter, elastic scaling, and shearing.
    shear_strength: Max percentage of lean in the xz direction (0.15 = up to 15% lean)
    """
    pts = np.array(points, dtype=np.float32)
    
    # 1. Random Jitter (Noise)
    noise = np.random.normal(0, scale_factor, pts.shape)
    pts += noise
    
    # 2. Elastic Scale (Stretch/Squash axes independently)
    scaler = np.random.uniform(0.85, 1.15, size=(1, 3))
    pts = pts * scaler

    # 3. Shearing (Leaning variance)
    # We modify x and z based on y to make the object "lean"
    # x' = x + (shear_x * y)
    # z' = z + (shear_z * y)
    if np.random.random() < 0.7:  # Apply to 70% of augmentations
        shear_x = np.random.uniform(-shear_strength, shear_strength)
        shear_z = np.random.uniform(-shear_strength, shear_strength)
        
        pts[:, 0] += shear_x * pts[:, 1]
        pts[:, 2] += shear_z * pts[:, 1]
    
    return pts.tolist()



def build_npz_dataset(raw_data_root, output_file='gesture_dataset.npz', augment_factor=50):
    """
    Loads dataset and creates 'augment_factor' copies of each drawing.
    """
    X = []
    y = []
    class_to_label = {}
    current_label = 0
    
    print(f"Building dataset from {raw_data_root} with {augment_factor}x augmentation...")

    for gesture_class in sorted(os.listdir(raw_data_root)):
        class_path = os.path.join(raw_data_root, gesture_class)
        if not os.path.isdir(class_path):
            continue 
            
        class_to_label[gesture_class] = current_label
        
        for file_name in os.listdir(class_path):
            if file_name.endswith('.json'):
                filepath = os.path.join(class_path, file_name)
                
                # Load the base points (Normalized, FPS applied)
                base_points = load_drawing_xyz(filepath)

                if base_points:
                    # 1. Add original
                    X.append(base_points)
                    y.append(current_label)

                    # 2. Add Augmented Copies
                    # We augment ONLY if it's the training set. 
                    # (You can check the path or just augment everything for now to test)
                    for _ in range(augment_factor):
                        aug_points = apply_augmentation(base_points)
                        X.append(aug_points)
                        y.append(current_label)
        
        current_label += 1


    script_dir = os.path.dirname(os.path.abspath(__file__))
    output_path = os.path.join(script_dir, '..', output_file)
    
    np.savez(
        output_path,
        X=np.array(X, dtype=object),
        y=np.array(y),
        class_to_label=class_to_label
    )
    
    print(f"Saved {output_file} with {len(X)} total samples.")
    return X, y, class_to_label


if __name__ == "__main__":
    raw_data_root = 'dataset_RAW/training'
    validation_root = 'dataset_RAW/validation'
    
    # Augment Training x50
    build_npz_dataset(raw_data_root, output_file='drawing_dataset.npz', augment_factor=50)
    build_npz_dataset(validation_root, output_file='validation_dataset.npz', augment_factor=0)