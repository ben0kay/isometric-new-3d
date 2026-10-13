"""Build the prototype explorer as a skinned glTF 2.0 asset. Python stdlib only.

Coordinates: metres, +Y up, -Z forward. Rigid armour sections use real skin
weights on a joint hierarchy. No animation moves the character through space.
Run: python PLAYER/Visuals/Source/build_player.py
"""
import json
import math
import struct
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Models' / 'PlayerExplorer.glb'
TAU = math.tau

# Material colours follow the supplied front and rear sprites.
MATERIALS = [
    ('SuitIvory', (0.66, 0.65, 0.59, 1), 0.0, 0.78),
    ('ArmourIvory', (0.86, 0.84, 0.76, 1), 0.18, 0.52),
    ('JointGraphite', (0.105, 0.13, 0.14, 1), 0.05, 0.9),
    ('VisorBlue', (0.025, 0.13, 0.19, 1), 0.65, 0.2),
    ('AccentCyan', (0.10, 0.69, 0.78, 1), 0.1, 0.35),
    ('FrameSteel', (0.30, 0.34, 0.34, 1), 0.5, 0.45),
    ('AmberIndicator', (0.85, 0.43, 0.10, 1), 0.0, 0.55),
]

# Every joint's bind orientation is identity; positions are world bind positions.
BONES = [
    ('Root', -1, (0, 0, 0)),
    ('Hips', 0, (0, 0.91, 0)),
    ('Spine', 1, (0, 1.12, 0)),
    ('Chest', 2, (0, 1.30, 0)),
    ('Head', 3, (0, 1.53, 0)),
]
for side, sign in [('Left', -1), ('Right', 1)]:
    p = len(BONES)
    BONES += [(side+'UpperArm', 3, (sign*0.31, 1.40, 0)),
              (side+'Forearm', p, (sign*0.35, 1.10, 0)),
              (side+'Hand', p+1, (sign*0.35, 0.84, -0.01))]
    p = len(BONES)
    BONES += [(side+'Thigh', 1, (sign*0.145, 0.89, 0)),
              (side+'Shin', p, (sign*0.145, 0.49, 0)),
              (side+'Foot', p+1, (sign*0.145, 0.12, 0))]
INDEX = {b[0]: i for i, b in enumerate(BONES)}
PARTS = []

def add_part(name, bone, material, positions, normals, indices):
    PARTS.append(dict(name=name, bone=INDEX[bone], material=material,
                      positions=positions, normals=normals, indices=indices))

def ellipsoid(name, bone, material, center, radius, rings=8, segments=12):
    positions, normals, indices = [], [], []
    for r in range(rings+1):
        phi = math.pi*r/rings
        for s in range(segments+1):
            theta = TAU*s/segments
            u = (math.sin(phi)*math.cos(theta), math.cos(phi), math.sin(phi)*math.sin(theta))
            positions.append(tuple(center[i]+radius[i]*u[i] for i in range(3)))
            n = tuple(u[i]/radius[i] for i in range(3))
            length = math.sqrt(sum(v*v for v in n))
            normals.append(tuple(v/length for v in n))
    for r in range(rings):
        for s in range(segments):
            a = r*(segments+1)+s
            b = a+segments+1
            # Outward-facing triangles.
            indices += [a, a+1, b, a+1, b+1, b]
    add_part(name, bone, material, positions, normals, indices)

def box(name, bone, material, center, size):
    p, n, idx = [], [], []
    for axis in range(3):
        a, b = (axis+1)%3, (axis+2)%3
        for sign in [-1, 1]:
            start = len(p)
            for u, v in [(-1,-1),(1,-1),(1,1),(-1,1)]:
                point = list(center)
                point[axis] += sign*size[axis]/2
                point[a] += u*size[a]/2
                point[b] += v*size[b]/2
                normal = [0,0,0]; normal[axis] = sign
                p.append(point); n.append(normal)
            face = [0,1,2,0,2,3] if sign == 1 else [0,2,1,0,3,2]
            idx += [start+i for i in face]
    add_part(name, bone, material, p, n, idx)

def build_geometry():
    ellipsoid('HipSuit', 'Hips', 0, (0,0.94,0), (0.235,0.16,0.16))
    box('UtilityBelt', 'Hips', 2, (0,1.02,0), (0.48,0.075,0.34))
    box('BeltBuckle', 'Hips', 5, (0,1.02,-0.185), (0.10,0.075,0.045))
    ellipsoid('TorsoSuit', 'Spine', 0, (0,1.25,0), (0.255,0.28,0.17))
    box('ChestPlate', 'Chest', 1, (0,1.32,-0.165), (0.37,0.30,0.08))
    box('ChestPanel', 'Chest', 5, (0,1.29,-0.218), (0.20,0.16,0.025))
    box('ChestDisplay', 'Chest', 3, (0,1.32,-0.236), (0.13,0.058,0.013))
    box('ChestLight', 'Chest', 4, (0.054,1.32,-0.244), (0.018,0.045,0.009))
    box('ChestButton', 'Chest', 6, (-0.055,1.265,-0.239), (0.025,0.018,0.015))
    ellipsoid('Collar', 'Chest', 2, (0,1.49,0), (0.16,0.055,0.145))
    ellipsoid('Helmet', 'Head', 1, (0,1.67,0), (0.215,0.235,0.205), 12, 20)
    ellipsoid('VisorSeal', 'Head', 2, (0,1.66,-0.165), (0.182,0.17,0.075), 10, 16)
    ellipsoid('Visor', 'Head', 3, (0,1.675,-0.191), (0.16,0.142,0.07), 10, 16)
    box('VisorReflection', 'Head', 4, (0.095,1.72,-0.249), (0.012,0.068,0.008))
    box('ChinGuard', 'Head', 1, (0,1.535,-0.17), (0.21,0.065,0.085))
    box('BackpackFrame', 'Chest', 2, (0,1.28,0.205), (0.39,0.43,0.17))
    box('BackpackShell', 'Chest', 1, (0,1.30,0.255), (0.33,0.36,0.15))
    box('BackpackPanel', 'Chest', 5, (0,1.30,0.337), (0.16,0.23,0.03))
    box('BackpackDisplay', 'Chest', 4, (0,1.34,0.356), (0.065,0.10,0.015))
    for side, sign in [('Left', -1), ('Right', 1)]:
        x = sign*0.35
        ellipsoid(side+'HelmetComms', 'Head', 5, (sign*0.205,1.65,0), (0.043,0.085,0.085))
        ellipsoid(side+'CommsInset', 'Head', 4, (sign*0.238,1.65,0), (0.012,0.039,0.04))
        ellipsoid(side+'Shoulder', side+'UpperArm', 1, (sign*0.31,1.39,0), (0.115,0.14,0.13))
        ellipsoid(side+'Sleeve', side+'UpperArm', 0, (x,1.23,0), (0.09,0.155,0.095))
        ellipsoid(side+'Elbow', side+'Forearm', 2, (x,1.105,0), (0.086,0.07,0.087))
        ellipsoid(side+'ForearmArmour', side+'Forearm', 1, (x,0.97,-0.008), (0.085,0.125,0.10))
        box(side+'WristDisplay', side+'Forearm', 5, (x,0.94,-0.10), (0.10,0.09,0.026))
        box(side+'WristLight', side+'Forearm', 4, (x,0.94,-0.117), (0.043,0.055,0.01))
        ellipsoid(side+'Glove', side+'Hand', 2, (x,0.78,-0.025), (0.075,0.092,0.085))
        ellipsoid(side+'Knuckles', side+'Hand', 1, (x,0.79,-0.08), (0.064,0.046,0.035))
        x = sign*0.145
        ellipsoid(side+'Trouser', side+'Thigh', 0, (x,0.72,0), (0.118,0.19,0.135))
        box(side+'ThighPanel', side+'Thigh', 1, (x,0.73,-0.118), (0.17,0.20,0.065))
        ellipsoid(side+'KneeJoint', side+'Shin', 2, (x,0.49,0), (0.10,0.07,0.11))
        ellipsoid(side+'KneeGuard', side+'Shin', 1, (x,0.49,-0.105), (0.105,0.085,0.055))
        ellipsoid(side+'LowerSuit', side+'Shin', 0, (x,0.31,0), (0.095,0.165,0.10))
        box(side+'ShinArmour', side+'Shin', 1, (x,0.30,-0.09), (0.145,0.21,0.06))
        box(side+'BootSole', side+'Foot', 2, (x,0.037,-0.048), (0.22,0.065,0.32))
        ellipsoid(side+'Boot', side+'Foot', 5, (x,0.11,-0.045), (0.105,0.078,0.15))
        box(side+'BootToe', side+'Foot', 1, (x,0.10,-0.155), (0.17,0.055,0.095))
        box(side+'HipPouch', 'Hips', 5, (sign*0.235,0.97,0), (0.075,0.145,0.16))

def quaternion(x=0, y=0, z=0):
    # Euler XYZ represented by qz*qy*qx.
    sx,cx = math.sin(x/2),math.cos(x/2)
    sy,cy = math.sin(y/2),math.cos(y/2)
    sz,cz = math.sin(z/2),math.cos(z/2)
    return [sx*cy*cz-cx*sy*sz, cx*sy*cz+sx*cy*sz,
            cx*cy*sz-sx*sy*cz, cx*cy*cz+sx*sy*sz]

def pose(clip, t, duration):
    angles = {name:[0,0,0] for name,_,_ in BONES}
    hips = list(BONES[1][2])
    p = TAU*t/duration
    if clip == 'Idle':
        hips[1] += 0.004*math.sin(p)
        angles['Chest'][0] = 0.012*math.sin(p)
        angles['Head'][1] = 0.025*math.sin(p)
    elif clip.startswith('Walk') or clip.startswith('Run'):
        run = clip.startswith('Run')
        amplitude = 0.68 if run else 0.40
        hips[1] += (0.055 if run else 0.028)*(0.5-0.5*math.cos(2*p))
        angles['Spine'][0] = -0.13 if run else -0.025
        angles['Hips'][1] = 0.035*math.sin(p)
        angles['Chest'][1] = -0.055*math.sin(p)
        for side, phase in [('Left',p),('Right',p+math.pi)]:
            swing = math.sin(phase)
            angles[side+'Thigh'][0] = amplitude*swing
            angles[side+'Shin'][0] = -(0.90 if run else 0.60)*max(0,swing)
            angles[side+'Foot'][0] = -angles[side+'Shin'][0] - angles[side+'Thigh'][0]
            angles[side+'UpperArm'][0] = -(0.60 if run else 0.30)*swing
            angles[side+'Forearm'][0] = 0.85 if run else 0.18
        # Directional variants rotate the lower body towards travel while the
        # torso and helmet remain aimed at the cursor.
        if 'Back' in clip:
            for side in ['Left','Right']:
                angles[side+'Thigh'][0] *= -1
                angles[side+'Foot'][0] = -angles[side+'Shin'][0] - angles[side+'Thigh'][0]
        elif 'Left' in clip or 'Right' in clip:
            yaw = -math.pi/2 if 'Left' in clip else math.pi/2
            for side in ['Left','Right']:
                angles[side+'Thigh'][1] = yaw
    elif clip == 'JumpStart':
        k = math.sin(math.pi*min(t/duration,1))
        hips[1] -= 0.035*k
        for side in ['Left','Right']:
            angles[side+'Thigh'][0] = 0.30*k
            angles[side+'Shin'][0] = -0.6*k
            angles[side+'Foot'][0] = 0.3*k
            angles[side+'UpperArm'][0] = 0.45*t/duration
            angles[side+'Forearm'][0] = 0.55*t/duration
    elif clip in ['JumpAir','Fall']:
        for side in ['Left','Right']:
            angles[side+'Thigh'][0] = 0.22 if side=='Left' else 0.08
            angles[side+'Shin'][0] = -0.50 if side=='Left' else -0.30
            angles[side+'UpperArm'][0] = 0.40
            angles[side+'Forearm'][0] = 0.6
            angles[side+'UpperArm'][2] = 0.16 if side=='Left' else -0.16
        angles['Spine'][0] = -0.07 if clip=='JumpAir' else 0.03
    elif clip == 'Land':
        k = math.sin(math.pi*t/duration)
        hips[1] -= 0.065*k
        for side in ['Left','Right']:
            angles[side+'Thigh'][0] = 0.45*k
            angles[side+'Shin'][0] = -0.85*k
            angles[side+'Foot'][0] = 0.4*k
            angles[side+'UpperArm'][0] = 0.30*k
            angles[side+'Forearm'][0] = 0.4*k
        angles['Spine'][0] = -0.15*k
    return {n:quaternion(*a) for n,a in angles.items()}, hips

CLIPS = [('Idle',2.4), ('Walk',0.9), ('Run',0.62),
         ('WalkBack',0.9), ('RunBack',0.62),
         ('WalkLeft',0.9), ('WalkRight',0.9),
         ('RunLeft',0.62), ('RunRight',0.62),
         ('JumpStart',0.16), ('JumpAir',0.6), ('Fall',0.6), ('Land',0.22)]

def build_glb():
    build_geometry()
    binary = bytearray()
    doc = dict(asset={'version':'2.0','generator':'Ben explorer prototype generator'},
               scene=0, scenes=[{'nodes':[0]}], nodes=[], meshes=[], skins=[],
               materials=[], animations=[], buffers=[], bufferViews=[], accessors=[])
    def accessor(values, kind, component=5126, target=None, bounds=False):
        widths = {'SCALAR':1,'VEC3':3,'VEC4':4,'MAT4':16}
        width = widths[kind]
        flattened = [x for row in values for x in row] if width>1 else values
        while len(binary)%4: binary.append(0)
        offset = len(binary)
        fmt = {5126:'f',5123:'H'}[component]
        binary.extend(struct.pack('<'+fmt*len(flattened),*flattened))
        view = {'buffer':0,'byteOffset':offset,'byteLength':len(binary)-offset}
        if target: view['target'] = target
        doc['bufferViews'].append(view)
        a = dict(bufferView=len(doc['bufferViews'])-1,componentType=component,count=len(values),type=kind)
        if bounds:
            rows = values if width>1 else [[v] for v in values]
            a['min']=[min(r[i] for r in rows) for i in range(width)]
            a['max']=[max(r[i] for r in rows) for i in range(width)]
        doc['accessors'].append(a)
        return len(doc['accessors'])-1
    for name,color,metal,rough in MATERIALS:
        m = {'name':name,'pbrMetallicRoughness':dict(baseColorFactor=color,metallicFactor=metal,roughnessFactor=rough)}
        if name=='AccentCyan': m['emissiveFactor']=[0.01,0.13,0.16]
        doc['materials'].append(m)
    doc['nodes'].append({'name':'PlayerExplorer','children':[1,len(BONES)+1]})
    for i,(name,parent,pos) in enumerate(BONES):
        pp = BONES[parent][2] if parent>=0 else (0,0,0)
        node = {'name':name,'translation':[pos[j]-pp[j] for j in range(3)]}
        children = [k+1 for k,b in enumerate(BONES) if b[1]==i]
        if children: node['children']=children
        doc['nodes'].append(node)
    # One skinned mesh, with material-grouped primitives to keep draw calls low.
    primitives = []
    for mat in range(len(MATERIALS)):
        ps,ns,js,ws,indices=[],[],[],[],[]
        for part in PARTS:
            if part['material']!=mat: continue
            offset=len(ps)
            ps += part['positions']; ns += part['normals']
            js += [[part['bone'],0,0,0] for _ in part['positions']]
            ws += [[1,0,0,0] for _ in part['positions']]
            indices += [i+offset for i in part['indices']]
        primitives.append(dict(attributes={
            'POSITION':accessor(ps,'VEC3',target=34962,bounds=True),
            'NORMAL':accessor(ns,'VEC3',target=34962),
            'JOINTS_0':accessor(js,'VEC4',5123,34962),
            'WEIGHTS_0':accessor(ws,'VEC4',target=34962)},
            indices=accessor(indices,'SCALAR',5123,34963),material=mat))
    doc['meshes'].append({'name':'ExplorerArmour','primitives':primitives})
    doc['nodes'].append({'name':'ExplorerMesh','mesh':0,'skin':0})
    inverses=[]
    for _,_,(x,y,z) in BONES:
        inverses.append([1,0,0,0,0,1,0,0,0,0,1,0,-x,-y,-z,1])
    doc['skins'].append(dict(name='ExplorerSkeleton',skeleton=1,
                            joints=list(range(1,len(BONES)+1)),inverseBindMatrices=accessor(inverses,'MAT4')))
    for clip,duration in CLIPS:
        count=max(3,math.ceil(duration*30)+1)
        times=[duration*i/(count-1) for i in range(count)]
        samples=[pose(clip,t,duration) for t in times]
        input_idx=accessor(times,'SCALAR',bounds=True)
        anim={'name':clip,'samplers':[],'channels':[]}
        for i,(bone,_,_) in enumerate(BONES):
            values=[p[0][bone] for p in samples]
            output_idx=accessor(values,'VEC4')
            sampler=len(anim['samplers'])
            anim['samplers'].append(dict(input=input_idx,output=output_idx,interpolation='LINEAR'))
            anim['channels'].append(dict(sampler=sampler,target={'node':i+1,'path':'rotation'}))
        output_idx=accessor([p[1] for p in samples],'VEC3')
        sampler=len(anim['samplers'])
        anim['samplers'].append(dict(input=input_idx,output=output_idx,interpolation='LINEAR'))
        anim['channels'].append(dict(sampler=sampler,target={'node':2,'path':'translation'}))
        doc['animations'].append(anim)
    doc['buffers']=[{'byteLength':len(binary)}]
    json_data=json.dumps(doc,separators=(',',':')).encode()
    while len(json_data)%4: json_data+=b' '
    while len(binary)%4: binary.append(0)
    body=struct.pack('<II',len(json_data),0x4E4F534A)+json_data+struct.pack('<II',len(binary),0x004E4942)+binary
    OUT.parent.mkdir(parents=True,exist_ok=True)
    OUT.write_bytes(struct.pack('<III',0x46546C67,2,12+len(body))+body)
    print(f'Built {OUT.name}: {len(BONES)} joints, {len(CLIPS)} clips, {sum(len(p["positions"]) for p in PARTS)} vertices')
    return doc,binary

if __name__=='__main__':
    build_glb()
