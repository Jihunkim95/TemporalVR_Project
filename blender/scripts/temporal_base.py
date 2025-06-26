"""
Temporal Base Script for Blender 4.4
Demonstrates temporal modeling: an object's properties evolve over time (frames).
This is the foundation for VR temporal modeling in the Temporal VR Project.
"""

import bpy
from mathutils import Vector
from typing import Any

# --- CONFIGURABLE PARAMETERS ---
START_FRAME: int = 1
END_FRAME: int = 100
CUBE_NAME: str = "TemporalCube"

# --- CLEANUP: Remove existing object with the same name ---
def remove_object(name: str) -> None:
    obj = bpy.data.objects.get(name)
    if obj:
        bpy.data.objects.remove(obj, do_unlink=True)

remove_object(CUBE_NAME)

# --- CREATE CUBE ---
bpy.ops.mesh.primitive_cube_add(size=2, location=(0, 0, 0))
cube = bpy.context.active_object
cube.name = CUBE_NAME

# --- TEMPORAL TRANSFORMATION FUNCTION ---
def temporal_transform(obj: Any, frame: int) -> None:
    """
    Apply temporal transformation to the object at a given frame.
    For demonstration: cube moves up and scales over time.
    """
    # Example: Move up and scale with time
    t = (frame - START_FRAME) / (END_FRAME - START_FRAME)  # Normalized time [0,1]
    obj.location = Vector((0, 0, t * 10))  # Move up along Z
    obj.scale = Vector((1 + t, 1 + t, 1 + t))  # Uniformly scale up

# --- ANIMATE OVER TIME ---
for frame in range(START_FRAME, END_FRAME + 1):
    bpy.context.scene.frame_set(frame)
    temporal_transform(cube, frame)
    # Insert keyframes for location and scale
    cube.keyframe_insert(data_path="location", frame=frame)
    cube.keyframe_insert(data_path="scale", frame=frame)

# --- EXPLANATION (for users) ---
"""
Temporal Modeling Concept:
- The cube's position and size change over 100 frames, representing its evolution through time.
- This script is a foundation for more complex temporal modeling, where users can define how objects evolve.
- In VR, users will manipulate these temporal properties interactively.
""" 