"""Validate GLB structure and render its actual skinned geometry for visual QA.

Optional development dependencies: numpy, Pillow. Not needed by Godot.
Run after build_player.py. Outputs a contact sheet and walking preview in Models.
"""
import json
import math
import struct
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
raw=(ROOT/'Models/PlayerExplorer.glb').read_bytes()
magic,version,length=struct.unpack_from('<III',raw)
assert magic==0x46546C67 and version==2 and length==len(raw)
jlen,jtype=struct.unpack_from('<II',raw,12)
assert jtype==0x4E4F534A
doc=json.loads(raw[20:20+jlen])
blen,btype=struct.unpack_from('<II',raw,20+jlen)
assert btype==0x004E4942
binary=raw[28+jlen:28+jlen+blen]

def data(index):
    a=doc['accessors'][index]; v=doc['bufferViews'][a['bufferView']]
    width={'SCALAR':1,'VEC3':3,'VEC4':4,'MAT4':16}[a['type']]
    dtype={5126:'<f4',5123:'<u2'}[a['componentType']]
    offset=v.get('byteOffset',0)+a.get('byteOffset',0)
    size=np.dtype(dtype).itemsize*a['count']*width
    assert offset+size<=len(binary)
    values=np.frombuffer(binary,dtype=dtype,count=a['count']*width,offset=offset).reshape(-1,width)
    assert np.isfinite(values).all()
    return values

def rotation(q):
    x,y,z,w=np.array(q)/np.linalg.norm(q)
    return np.array([[1-2*(y*y+z*z),2*(x*y-z*w),2*(x*z+y*w)],
                     [2*(x*y+z*w),1-2*(x*x+z*z),2*(y*z-x*w)],
                     [2*(x*z-y*w),2*(y*z+x*w),1-2*(x*x+y*y)]])

parents={child:i for i,n in enumerate(doc['nodes']) for child in n.get('children',[])}
skin=doc['skins'][0]
ibm=data(skin['inverseBindMatrices']).reshape(-1,4,4).transpose(0,2,1)

def matrices(clip=None,t=0):
    translations=[np.array(n.get('translation',[0,0,0]),float) for n in doc['nodes']]
    rotations=[np.array(n.get('rotation',[0,0,0,1]),float) for n in doc['nodes']]
    if clip:
        anim=next(a for a in doc['animations'] if a['name']==clip)
        for ch in anim['channels']:
            s=anim['samplers'][ch['sampler']]; times=data(s['input']).ravel(); values=data(s['output'])
            i=max(0,min(len(times)-2,int(np.searchsorted(times,t)-1)))
            w=np.clip((t-times[i])/(times[i+1]-times[i]),0,1)
            value=values[i]*(1-w)+values[i+1]*w
            dest=translations if ch['target']['path']=='translation' else rotations
            dest[ch['target']['node']]=value
    world=[]
    for i,n in enumerate(doc['nodes']):
        m=np.eye(4); m[:3,:3]=rotation(rotations[i]); m[:3,3]=translations[i]
        if i in parents: m=world[parents[i]]@m
        world.append(m)
    return np.array([world[j]@ibm[k] for k,j in enumerate(skin['joints'])])

primitives=doc['meshes'][0]['primitives']
assert len(skin['joints'])==17
for a in doc['accessors']: data(doc['accessors'].index(a))
for p in primitives:
    attrs=p['attributes']; joints=data(attrs['JOINTS_0']); weights=data(attrs['WEIGHTS_0'])
    assert joints.max()<17 and np.allclose(weights.sum(axis=1),1)
    assert data(p['indices']).max()<len(data(attrs['POSITION']))
bind=matrices()
assert np.allclose(bind,np.eye(4),atol=1e-6), 'Invalid inverse bind matrices'
loop_names=['Idle','Walk','Run','WalkBack','RunBack','WalkLeft','WalkRight','RunLeft','RunRight','JumpAir','Fall']
for anim in doc['animations']:
    for ch in anim['channels']:
        s=anim['samplers'][ch['sampler']]; times=data(s['input']).ravel(); values=data(s['output'])
        assert np.all(np.diff(times)>0) and len(times)==len(values)
        if ch['target']['path']=='rotation': assert np.allclose(np.linalg.norm(values,axis=1),1,atol=1e-5)
        if anim['name'] in loop_names: assert np.allclose(values[0],values[-1],atol=1e-5)

def render(clip,t,view=(3,2,-4),width=360,height=440):
    transforms=matrices(clip,t)
    towards=np.array(view,float); towards/=np.linalg.norm(towards)
    right=np.cross([0,1,0],towards); right/=np.linalg.norm(right)
    up=np.cross(towards,right)
    basis=np.stack([right,up,towards],axis=1)
    pixels=np.full((height,width,3),[27,36,44],dtype=np.uint8)
    zbuffer=np.full((height,width),-np.inf)
    light=np.array([-0.5,0.8,-0.3]); light/=np.linalg.norm(light)
    # Render every triangle from the exported mesh and skin, not source shapes.
    for primitive in primitives:
        a=primitive['attributes']; pos=data(a['POSITION']); normal=data(a['NORMAL'])
        joints=data(a['JOINTS_0'])[:,0].astype(int)
        homogeneous=np.concatenate([pos,np.ones((len(pos),1))],axis=1)
        skinned=np.einsum('nij,nj->ni',transforms[joints],homogeneous)[:,:3]
        normals=np.einsum('nij,nj->ni',transforms[joints,:3,:3],normal)
        projected=(skinned-np.array([0,0.98,0]))@basis
        projected[:,0]=projected[:,0]*185+width/2
        projected[:,1]=-projected[:,1]*185+height/2
        material=doc['materials'][primitive['material']]
        colour=np.array(material['pbrMetallicRoughness']['baseColorFactor'][:3])
        for tri in data(primitive['indices']).ravel().reshape(-1,3):
            xyz=projected[tri]
            x0,y0=np.maximum(np.floor(xyz[:,:2].min(axis=0)),[0,0]).astype(int)
            x1,y1=np.minimum(np.ceil(xyz[:,:2].max(axis=0)),[width-1,height-1]).astype(int)
            if x1<x0 or y1<y0: continue
            x,y=np.meshgrid(np.arange(x0,x1+1)+0.5,np.arange(y0,y1+1)+0.5)
            aa,bb,cc=xyz
            den=(bb[1]-cc[1])*(aa[0]-cc[0])+(cc[0]-bb[0])*(aa[1]-cc[1])
            if abs(den)<1e-9: continue
            w0=((bb[1]-cc[1])*(x-cc[0])+(cc[0]-bb[0])*(y-cc[1]))/den
            w1=((cc[1]-aa[1])*(x-cc[0])+(aa[0]-cc[0])*(y-cc[1]))/den
            w2=1-w0-w1
            z=w0*aa[2]+w1*bb[2]+w2*cc[2]
            section=zbuffer[y0:y1+1,x0:x1+1]
            mask=(w0>=0)&(w1>=0)&(w2>=0)&(z>section)
            if not mask.any(): continue
            norm=normals[tri].mean(axis=0); norm/=max(np.linalg.norm(norm),1e-8)
            brightness=0.42+0.58*max(0,float(norm@light))
            rgb=np.clip(np.power(colour*brightness,1/2.2)*255,0,255).astype(np.uint8)
            pixels[y0:y1+1,x0:x1+1][mask]=rgb; section[mask]=z[mask]
    return Image.fromarray(pixels)

if __name__=='__main__':
    # Catch backwards ankles and ground penetration across sampled gait poses.
    for clip in ['Idle','Walk','Run','WalkBack','RunBack','WalkLeft','WalkRight','RunLeft','RunRight','Land']:
        anim=next(a for a in doc['animations'] if a['name']==clip)
        duration=float(data(anim['samplers'][0]['input'])[-1,0])
        for t in np.linspace(0,duration,25):
            transforms=matrices(clip,float(t))
            for primitive in primitives:
                attrs=primitive['attributes']; pos=data(attrs['POSITION'])
                joints=data(attrs['JOINTS_0'])[:,0].astype(int)
                h=np.concatenate([pos,np.ones((len(pos),1))],axis=1)
                minimum=np.einsum('nij,nj->ni',transforms[joints],h)[:,1].min()
                assert minimum>-0.015, (clip,float(t),'ground penetration',minimum)
    views=[('Front / idle','Idle',0,(3,2,-4)),('Rear / backpack','Idle',0,(-3,2,4)),
           ('Walk','Walk',0.225,(3,2,-4)),('Run','Run',0.155,(3,2,-4)),
           ('Jump / airborne','JumpAir',0.2,(3,2,-4)),('Landing','Land',0.11,(3,2,-4))]
    sheet=Image.new('RGB',(360*3,480*2),(27,36,44)); draw=ImageDraw.Draw(sheet)
    for i,(label,clip,t,view) in enumerate(views):
        x=(i%3)*360; y=(i//3)*480
        sheet.paste(render(clip,t,view),(x,y+30))
        draw.text((x+20,y+12),label,fill=(210,224,234))
    sheet.save(ROOT/'Models/PlayerExplorerPreview.png')
    frames=[render('Walk',i*0.9/24) for i in range(24)]
    frames[0].save(ROOT/'Models/PlayerExplorerWalk.gif',save_all=True,append_images=frames[1:],duration=38,loop=0)
    print('PASS: GLB bounds, skin weights, bind pose, animation samples, quaternions and loop continuity.')
