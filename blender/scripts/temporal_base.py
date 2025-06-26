import bpy

# Temporal Modeling Proof-of-Concept Script
# This script demonstrates how an object's properties can evolve over time (temporal modeling)
# by animating a cube's location, scale, and rotation over 100 frames.

# 1. Remove all existing objects for a clean slate
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)

# 2. Create a simple cube
bpy.ops.mesh.primitive_cube_add(size=2, location=(0, 0, 0))
cube = bpy.context.active_object
cube.name = "TemporalCube"

# 3. Define temporal transformations over 100 frames
start_frame = 1
end_frame = 100
bpy.context.scene.frame_start = start_frame
bpy.context.scene.frame_end = end_frame

for frame in range(start_frame, end_frame + 1):
    # Set the current frame
    bpy.context.scene.frame_set(frame)
    
    # Temporal concept: The cube evolves over time
    # Example: It moves upward, grows, and spins as time progresses
    t = (frame - start_frame) / (end_frame - start_frame)  # Normalized time [0, 1]
    
    # Location: Move up along Z axis
    cube.location = (0, 0, t * 10)
    cube.keyframe_insert(data_path="location", index=-1)
    
    # Scale: Grow from 1 to 2
    cube.scale = (1 + t, 1 + t, 1 + t)
    cube.keyframe_insert(data_path="scale", index=-1)
    
    # Rotation: Spin around Z axis
    cube.rotation_euler = (0, 0, t * 6.28319)  # 0 to 2*PI radians (360 degrees)
    cube.keyframe_insert(data_path="rotation_euler", index=-1)

# ---
# Temporal Modeling Concept:
# Each frame represents a different point in time. By keyframing properties (location, scale, rotation),
# we define how the object 'evolves' as time progresses. This is the foundation for temporal modeling in Blender.
# --- 