# temporal_base.py
# Temporal VR Project - First temporal modeling script
# This demonstrates the core concept: manipulating time as a parameter in 3D modeling

import bpy
import math
from typing import Dict, Tuple, List

class TemporalObject:
    """Base class for objects that change over time"""
    
    def __init__(self, name: str = "TemporalCube"):
        self.name = name
        self.temporal_keyframes: Dict[float, Dict] = {}
        self.obj = None
        
    def create_base_object(self):
        """Create the base 3D object"""
        # Remove existing object if it exists
        if self.name in bpy.data.objects:
            bpy.data.objects.remove(bpy.data.objects[self.name])
            
        # Create new cube
        bpy.ops.mesh.primitive_cube_add(location=(0, 0, 0))
        self.obj = bpy.context.active_object
        self.obj.name = self.name
        
        # Add custom properties for temporal data
        self.obj["is_temporal"] = True
        self.obj["temporal_range"] = [0, 100]  # Start and end frames
        
    def add_temporal_keyframe(self, time: float, properties: Dict):
        """Add a keyframe at specific time with properties"""
        self.temporal_keyframes[time] = properties
        
    def apply_temporal_evolution(self):
        """Apply the temporal changes to the object"""
        if not self.obj:
            return
            
        # Clear existing keyframes
        self.obj.animation_data_clear()
        
        # Define temporal evolution phases
        temporal_phases = [
            {
                'time': 0,
                'scale': (1, 1, 1),
                'rotation': (0, 0, 0),
                'location': (0, 0, 0),
                'description': 'Birth - Object emerges'
            },
            {
                'time': 25,
                'scale': (2, 2, 0.5),
                'rotation': (0, 0, math.radians(45)),
                'location': (0, 0, 1),
                'description': 'Growth - Expanding and rotating'
            },
            {
                'time': 50,
                'scale': (1.5, 1.5, 3),
                'rotation': (math.radians(30), 0, math.radians(90)),
                'location': (2, 0, 2),
                'description': 'Maturity - Complex transformation'
            },
            {
                'time': 75,
                'scale': (3, 0.5, 1),
                'rotation': (math.radians(60), math.radians(45), math.radians(180)),
                'location': (3, 2, 1),
                'description': 'Decay - Deforming'
            },
            {
                'time': 100,
                'scale': (0.1, 0.1, 0.1),
                'rotation': (math.radians(90), math.radians(90), math.radians(270)),
                'location': (0, 0, 0),
                'description': 'End - Returning to origin'
            }
        ]
        
        # Apply keyframes for each temporal phase
        for phase in temporal_phases:
            frame = int(phase['time'])
            
            # Set frame
            bpy.context.scene.frame_set(frame)
            
            # Apply transformations
            self.obj.location = phase['location']
            self.obj.rotation_euler = phase['rotation']
            self.obj.scale = phase['scale']
            
            # Insert keyframes
            self.obj.keyframe_insert(data_path="location", frame=frame)
            self.obj.keyframe_insert(data_path="rotation_euler", frame=frame)
            self.obj.keyframe_insert(data_path="scale", frame=frame)
            
            # Store in temporal data
            self.add_temporal_keyframe(phase['time'], phase)
            
        print(f"✅ Temporal evolution applied: {len(temporal_phases)} phases")
        
    def add_temporal_material(self):
        """Add material that changes over time"""
        # Create material
        mat = bpy.data.materials.new(name=f"{self.name}_TemporalMat")
        mat.use_nodes = True
        
        # Get nodes
        nodes = mat.node_tree.nodes
        links = mat.node_tree.links
        
        # Clear default nodes
        nodes.clear()
        
        # Add nodes
        output = nodes.new(type='ShaderNodeOutputMaterial')
        principled = nodes.new(type='ShaderNodeBsdfPrincipled')
        color_ramp = nodes.new(type='ShaderNodeValToRGB')
        time_input = nodes.new(type='ShaderNodeValue')
        
        # Position nodes
        output.location = (400, 0)
        principled.location = (200, 0)
        color_ramp.location = (0, 0)
        time_input.location = (-200, 0)
        
        # Connect nodes
        links.new(time_input.outputs[0], color_ramp.inputs[0])
        links.new(color_ramp.outputs[0], principled.inputs[0])  # Base Color
        links.new(principled.outputs[0], output.inputs[0])
        
        # Set up color gradient (time visualization)
        color_ramp.color_ramp.elements[0].color = (0.1, 0.1, 0.8, 1)  # Past = Blue
        color_ramp.color_ramp.elements[1].color = (0.8, 0.1, 0.1, 1)  # Future = Red
        
        # Assign material
        self.obj.data.materials.append(mat)
        
        # Animate the time value
        time_input.outputs[0].default_value = 0
        time_input.outputs[0].keyframe_insert(data_path="default_value", frame=0)
        time_input.outputs[0].default_value = 1
        time_input.outputs[0].keyframe_insert(data_path="default_value", frame=100)
        
    def create_temporal_visualization(self):
        """Create visual guides for temporal evolution"""
        # Create path visualization
        curve = bpy.data.curves.new(name=f"{self.name}_TimePath", type='CURVE')
        curve.dimensions = '3D'
        
        spline = curve.splines.new(type='NURBS')
        spline.points.add(len(self.temporal_keyframes) - 1)
        
        # Set points based on temporal keyframes
        for i, (time, props) in enumerate(sorted(self.temporal_keyframes.items())):
            point = spline.points[i]
            point.co = (*props['location'], 1)  # (x, y, z, w)
            
        # Create curve object
        path_obj = bpy.data.objects.new(f"{self.name}_TimePath", curve)
        bpy.context.collection.objects.link(path_obj)
        
        # Style the path
        curve.bevel_depth = 0.05
        curve.bevel_resolution = 4
        
        print("✅ Temporal path visualization created")

def demonstrate_temporal_concept():
    """Main function to demonstrate temporal modeling"""
    
    print("🚀 Starting Temporal VR Demonstration...")
    
    # Set up scene
    bpy.context.scene.frame_start = 0
    bpy.context.scene.frame_end = 100
    bpy.context.scene.frame_set(0)
    
    # Create temporal object
    temporal_obj = TemporalObject("TemporalCube_Demo")
    
    # Build the demonstration
    temporal_obj.create_base_object()
    temporal_obj.apply_temporal_evolution()
    temporal_obj.add_temporal_material()
    temporal_obj.create_temporal_visualization()
    
    # Add info text
    add_scene_info()
    
    print("✅ Temporal demonstration complete!")
    print("💡 Press SPACE to play the animation and see time evolution")
    print("🎯 This is the foundation for VR temporal modeling")
    
    return temporal_obj

def add_scene_info():
    """Add text information to the scene"""
    # Create text object
    bpy.ops.object.text_add(location=(-3, -3, 3))
    text_obj = bpy.context.active_object
    text_obj.name = "TemporalInfo"
    
    # Set text
    text_obj.data.body = "Temporal VR Demo\nTime as 4th Dimension\nFrame: 0-100"
    text_obj.data.align_x = 'CENTER'
    text_obj.data.align_y = 'CENTER'
    
    # Scale and position
    text_obj.scale = (0.5, 0.5, 0.5)

# Utility functions for future VR integration
def export_temporal_data(obj_name: str, filepath: str):
    """Export temporal data for Unity import"""
    if obj_name not in bpy.data.objects:
        return None
        
    obj = bpy.data.objects[obj_name]
    
    # Prepare temporal data for export
    temporal_data = {
        'object_name': obj_name,
        'temporal_range': obj.get('temporal_range', [0, 100]),
        'keyframes': []
    }
    
    # Extract keyframe data
    # This will be expanded for Unity integration
    
    print(f"📤 Temporal data ready for export: {filepath}")
    return temporal_data

# Execute demonstration when script runs
if __name__ == "__main__":
    # Clear existing mesh objects (optional)
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False, confirm=False)
    
    # Run demonstration
    temporal_obj = demonstrate_temporal_concept()
    
    # Prepare for next phase
    print("\n📌 Next Steps:")
    print("1. Test this in Blender 4.4")
    print("2. Export to Unity for VR interaction")
    print("3. Add hand gesture controls for time manipulation")