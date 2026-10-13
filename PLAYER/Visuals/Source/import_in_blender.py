"""Import the generated model into Blender and save an editable .blend source.

Run from Blender's Scripting workspace, or:
blender --background --python PLAYER/Visuals/Source/import_in_blender.py
The GLB contains mesh, materials, skeleton, skin weights and all animation clips.
"""
from pathlib import Path
import bpy

HERE = Path(__file__).resolve().parent
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(HERE.parent / 'Models' / 'PlayerExplorer.glb'))
bpy.ops.wm.save_as_mainfile(filepath=str(HERE / 'PlayerExplorer.blend'))
print('Saved editable Blender character: PlayerExplorer.blend')
